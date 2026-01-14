using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PiecePlaceManager : MonoBehaviour
{
    // Singelton
    public static PiecePlaceManager Instance { get; private set; }

    public Material tileMaterial;
    public Material pieceMaterial;

    public LayerMask PlatformPlaceMask;
    public LayerMask TowerPlaceMask;


    private TowerPlacementService _towerPlacementService;
    private PathFindingService _pathFindingService;
    private Economy _economy;
    private GameObject _shadowCursorPrefab;
    private PieceShape shape;
    private PlatformSO _platformBeingPlaced;
    private TowerSO _towerBeingPlaced;

    private bool _arePlacing = false;
    private World _world;
    private int _rotation = 0;

    public PiecePlaceManager()
    {
        if (Instance == null)
            Instance = this;

        _world = GameStateManager.GetInstance().world;
        _economy = GameStateManager.GetInstance().Economy;
        _pathFindingService = GameStateManager.GetInstance().PathfindingService;
        _towerPlacementService = GameStateManager.GetInstance().TowerPlacementService;

    }

    void Start()
    {
        // Create world
        Mesh planeMesh = MeshUtils.CreatePlaneMesh(_world.Width, _world.Height, 1.0f);
        GameObject platformPlaneGO = MeshUtils.CreateGameObjectFromMesh(planeMesh, tileMaterial, "PlatformPlane", true);
        platformPlaneGO.transform.SetParent(transform);
        platformPlaneGO.tag = "PlatformPlane";
        platformPlaneGO.layer = Utils.GetBitIndex(PlatformPlaceMask);

        GameObject towerPlaneGO = MeshUtils.CreateGameObjectFromMesh(planeMesh, tileMaterial, "TowerPlane", true);
        towerPlaneGO.transform.SetParent(transform);
        towerPlaneGO.tag = "TowerPlane";
        towerPlaneGO.transform.position += new Vector3(0, 1.0f, 0);
        towerPlaneGO.GetComponent<MeshRenderer>().enabled = false;
        towerPlaneGO.layer = Utils.GetBitIndex(TowerPlaceMask);


        EventBus.Instance.Register<PlatformCardSelectEvent>(OnPlatformCardSelectEvent);
        EventBus.Instance.Register<TowerSelectEvent>(OnTowerSelectEvent);
        EventBus.Instance.Register<EnemyWaveStartEvent>(OnEnemyWaveStart);
    }

    public void OnEnemyWaveStart(EnemyWaveStartEvent evnt)
    {
        if (_arePlacing && _platformBeingPlaced != null)
            StopPlacing();
    }

    public void OnPlatformCardSelectEvent(PlatformCardSelectEvent evnt)
    {
        // Clear old placing
        StopPlacing();

        _platformBeingPlaced = evnt.Platform;
        _towerBeingPlaced = null;
        _arePlacing = true;
        shape = PieceShape.FromString(evnt.Platform.PlatformShape);
        UpdateShadowPiece(shape);
        // initally keep the cursor hidden
        HideCursor();
    }

    public void OnTowerSelectEvent(TowerSelectEvent evnt)
    {
        // Clear old placing
        StopPlacing();

        _towerBeingPlaced = evnt.Tower;
        _platformBeingPlaced = null;
        _arePlacing = true;
        _rotation = 0;
        shape = PieceShape.FromString(evnt.Tower.TowerShape);
        UpdateShadowPiece(_towerBeingPlaced.TowerCursorPrefab, 0);
        // HideCursor();
    }

    private void UpdateShadowPiece(GameObject prefab, int rotation=0)
    {
        if (_shadowCursorPrefab != null)
            Destroy(_shadowCursorPrefab);

        _shadowCursorPrefab = GameObject.Instantiate(prefab);
        _shadowCursorPrefab.transform.SetParent(transform);
        HydrateTowerWithSO(_shadowCursorPrefab, _towerBeingPlaced);
        ActivateRotationVisuals(_shadowCursorPrefab, rotation);
    }

    private void UpdateShadowPiece(PieceShape shape)
    {
        if (_shadowCursorPrefab != null)
            Destroy(_shadowCursorPrefab);

        _shadowCursorPrefab = MeshUtils.CreateGameObjectFromMesh(MeshUtils.CreateInsetPieceMesh(shape, 1.0f), pieceMaterial, "ShadowPiece");
        _shadowCursorPrefab.GetComponent<MeshRenderer>().material.color = _platformBeingPlaced.PlatformColor;
        _shadowCursorPrefab.transform.SetParent(transform);
    }

    private struct WorldToGridOutput{
        public Vector3 worldPosition;
        public GridCoord pieceOrigin;
    }

    private WorldToGridOutput WorldToGrid(Vector3 hitPosition, PieceShape pieceShape)
    {
        WorldToGridOutput output = new WorldToGridOutput();
        hitPosition -= new Vector3(
            (pieceShape.Width / 2.0f) - 0.5f,
            0,
            (pieceShape.Height / 2.0f) - 0.5f
        ) ;
        Vector3 roundedPosition = new Vector3(
            Mathf.Round(hitPosition.x),
            0.5f,
            Mathf.Round(hitPosition.z)
        );
        output.worldPosition = roundedPosition;
        output.pieceOrigin = new GridCoord(
            (int)Mathf.Round(roundedPosition.x),
            (int)Mathf.Round(roundedPosition.z)
        );

        return output;
    }

    private void StopPlacing()
    {
        _arePlacing = false;
        if (_shadowCursorPrefab != null)
            Destroy(_shadowCursorPrefab);
        _shadowCursorPrefab = null;
        _platformBeingPlaced = null;
        _towerBeingPlaced = null;
    }

    private void HideCursor()
    {
        if (_shadowCursorPrefab != null && _shadowCursorPrefab.activeSelf)
            _shadowCursorPrefab.SetActive(false);
    }

    private void ShowCursor()
    {
        if (_shadowCursorPrefab != null && !_shadowCursorPrefab.activeSelf)
            _shadowCursorPrefab.SetActive(true);
    }

    private void ActivateRotationVisuals(GameObject go, int rotation)
    {
        foreach (Transform child in go.transform)
        {
            if (child.name == "RotationOffset")
            {
                Vector3 rot = child.transform.eulerAngles;
                rot.y = rotation;
                child.transform.eulerAngles = rot;
                if (rotation == 0)
                {
                    child.transform.localPosition = Vector3.zero;
                } else if (rotation == 90)
                {
                    child.transform.localPosition = new Vector3(0.0f, 0.0f, shape.Height - 1);
                } else if (rotation == 180)
                {
                    child.transform.localPosition = new Vector3(shape.Width - 1, 0.0f, shape.Height - 1);
                } else if (rotation == 270)
                {
                    child.transform.localPosition = new Vector3(shape.Width - 1, 0.0f, 0.0f);
                } else
                {
                    throw new System.Exception("Unsupported rotation " + rotation);
                }
                return;
            }
        }
    }

    private void HydrateTowerWithSO(GameObject towerGO, TowerSO towerSO)
    {
        towerGO.GetComponent<TowerSOHolder>().SetTowerData(towerSO);
    }


    void Update()
    {
        if (!_arePlacing)
            return;

        if (Input.GetKeyDown(KeyCode.Escape)) {
            EventBus.Instance.Publish<CancelPlaceEvent>(new CancelPlaceEvent(_platformBeingPlaced, _towerBeingPlaced));
            StopPlacing();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (_platformBeingPlaced != null)
            {
                shape = shape.GetRotation(90);
                UpdateShadowPiece(shape);
            }
            else if (_towerBeingPlaced != null)
            {
                shape = shape.GetRotation(90);
                _rotation = (_rotation + 90) % 360;
                UpdateShadowPiece(_towerBeingPlaced.TowerCursorPrefab, _rotation);
            }
        }

        // Raycast from mouse
        RaycastHit hit;

        if (_platformBeingPlaced != null) {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f, PlatformPlaceMask))
            {
                if (hit.collider.gameObject.CompareTag("PlatformPlane"))
                {
                    ShowCursor();
                    if (_shadowCursorPrefab != null)
                    {
                        Vector3 hitPosition = hit.point;
                        WorldToGridOutput transformedPoint = WorldToGrid(hitPosition, shape);
                        _shadowCursorPrefab.transform.position = transformedPoint.worldPosition;

                        TowerPlacementResult result = _towerPlacementService.CanPlacePiece(new Piece(shape, transformedPoint.pieceOrigin), CellContent.Platform);

                        if (Input.GetMouseButtonDown(0) && result != TowerPlacementResult.Success)
                            switch (result)
                            {
                                case TowerPlacementResult.Collision:
                                    WarningUI.Instance.ShowWarning("Cannot place platform here: Overlaps with another piece.");
                                    break;
                                case TowerPlacementResult.OutOfBounds:
                                    WarningUI.Instance.ShowWarning("Cannot place platform here: Out of world bounds.");
                                    break;
                                case TowerPlacementResult.BlocksPathfinding:
                                    WarningUI.Instance.ShowWarning("Cannot place platform here: Would block enemy path.");
                                    break;
                            }
                            
                        if (Input.GetMouseButtonDown(0) && _economy.HasEnoughMoney(_platformBeingPlaced.Price) == false)
                        {
                            WarningUI.Instance.ShowWarning("Cannot place platform here: Not enough funds.");
                        }

                        if(Input.GetMouseButtonDown(0) && result == TowerPlacementResult.Success && _economy.HasEnoughMoney(_platformBeingPlaced.Price))
                        {

                            bool success = _towerPlacementService.TryPlacePiece(new Piece(shape, transformedPoint.pieceOrigin), CellContent.Platform);
                            if (!success)
                                throw new System.Exception("CanPlace and TryPlace not consistent!");

                            _economy.SpendMoney(_platformBeingPlaced.Price);
                            
                            GameObject newPieceGO = MeshUtils.CreateGameObjectFromMesh(MeshUtils.CreateInsetPieceMesh(shape, 1.0f), pieceMaterial, "Platform");
                            newPieceGO.GetComponent<MeshRenderer>().material.color = _platformBeingPlaced.PlatformColor;
                            newPieceGO.transform.SetParent(transform);
                            newPieceGO.transform.position = transformedPoint.worldPosition;

                            // ORDER: We need to publish first then stop placing to not erase the platform being placed
                            EventBus.Instance.Publish<PlatformPlaceEvent>(new PlatformPlaceEvent(_platformBeingPlaced, newPieceGO));
                            StopPlacing();
                        }
                    }
                } 
                else
                {
                    HideCursor();
                }
            } else
            {
                HideCursor();
            }
        }

        if (_towerBeingPlaced != null)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f, TowerPlaceMask))
            {
                if (hit.collider.gameObject.CompareTag("TowerPlane"))
                {
                    ShowCursor();
                    Vector3 hitPosition = hit.point;
                    WorldToGridOutput transformedPoint = WorldToGrid(hitPosition, shape);
                    _shadowCursorPrefab.transform.position = transformedPoint.worldPosition + new Vector3(0.0f, 1.0f, 0.0f);

                    TowerPlacementResult result = _towerPlacementService.CanPlacePiece(new Piece(shape, transformedPoint.pieceOrigin), CellContent.Tower);

                    if (Input.GetMouseButtonDown(0) && result != TowerPlacementResult.Success)
                        switch (result)
                        {
                            case TowerPlacementResult.Collision:
                                WarningUI.Instance.ShowWarning("Cannot place tower here: No supporting Platform.");
                                break;
                            case TowerPlacementResult.OutOfBounds:
                                WarningUI.Instance.ShowWarning("Cannot place tower here: Out of world bounds.");
                                break;
                            case TowerPlacementResult.BlocksPathfinding:
                                WarningUI.Instance.ShowWarning("Cannot place tower here: Would block enemy path.");
                                break;
                        }

                    if (Input.GetMouseButtonDown(0) && _economy.HasEnoughMoney(_towerBeingPlaced.Price) == false)
                    {
                        WarningUI.Instance.ShowWarning("Cannot place tower here: Not enough funds.");
                    }

                    if(Input.GetMouseButtonDown(0) && result == TowerPlacementResult.Success && _economy.HasEnoughMoney(_towerBeingPlaced.Price))
                    {

                        bool success = _towerPlacementService.TryPlacePiece(new Piece(shape, transformedPoint.pieceOrigin), CellContent.Tower);
                        if (!success)
                            throw new System.Exception("CanPlace and TryPlace not consistent!");

                        _economy.SpendMoney(_towerBeingPlaced.Price);
                        
                        GameObject newTowerGO = GameObject.Instantiate(_towerBeingPlaced.TowerPrefab);
                        newTowerGO.transform.SetParent(transform);
                        newTowerGO.transform.position = transformedPoint.worldPosition + new Vector3(0.0f, 1.0f, 0.0f);
                        ActivateRotationVisuals(newTowerGO, _rotation);
                        HydrateTowerWithSO(newTowerGO, _towerBeingPlaced);

                        // ORDER: We need to publish first then stop placing to not erase the tower being placed
                        EventBus.Instance.Publish<TowerPlaceEvent>(new TowerPlaceEvent(_towerBeingPlaced, newTowerGO));
                        StopPlacing();
                    }

                } 
                else
                {
                    HideCursor();
                }
            } else
            {
                HideCursor();
            }
        }
    }
}
