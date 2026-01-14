using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathfindingVisual : MonoBehaviour
{
    public Material material;
    public float height = 0.1f;

    private World _world;
    private PathFindingService _pathFindingService;
    private GameObject _lineGO;
    private LineRenderer _lineRenderer;
    public Transform startLocation;
    public Transform endLocation;

    public PathfindingVisual()
    {
        _world = GameStateManager.GetInstance().world;
        // Long term we shouldn't need the pathfinding service here, but for now it's fine for now
        _pathFindingService = GameStateManager.GetInstance().PathfindingService;

        EventBus.Instance.Register<PlatformPlaceEvent>(OnPlatformPlaceHandler);
    }

    void OnPlatformPlaceHandler(PlatformPlaceEvent evnt)
    {
        UpdateVisual();
    }

    void Start()
    {
        UpdateVisual();
    }

    void UpdateVisual()
    {
        GridPath path = _pathFindingService.FindBestPath();

        if (_lineGO == null)
        {
            _lineGO = new GameObject("PathfindingLine");
            // _lineGO.transform.SetParent(transform);
            _lineRenderer = _lineGO.AddComponent<LineRenderer>();
        }

        _lineRenderer.material = material;
        _lineRenderer.positionCount = path.Cells.Count + 2;
        _lineRenderer.startWidth = 0.1f;
        _lineRenderer.endWidth = 0.1f;

        int index = 0;
        _lineRenderer.SetPosition(index, new Vector3(startLocation.position.x, height, startLocation.position.z));
        foreach (var coord in path.Cells)
        {
            _lineRenderer.SetPosition(index+1, new Vector3(coord.X, height, coord.Y));
            index += 1;
        }
        _lineRenderer.SetPosition(index+1, new Vector3(endLocation.position.x, height, endLocation.position.z));
    }
}
