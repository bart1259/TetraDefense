using UnityEngine;

public class RangeVisualizer : MonoBehaviour
{

    public Material material;

    private float _range;
    private TowerSO _towerSO;

    void Start()
    {
        _towerSO = GetComponentInParent<TowerSOHolder>().TowerSOData;
        _range = _towerSO.Range;
        VisualizeCircleRange(transform.position, _range);
    }

    public void VisualizeCircleRange(Vector3 position, float radius)
    {
        GameObject vizGO = MeshUtils.CreateGameObjectFromMesh(MeshUtils.MeshCreateCylinderMesh(radius, 2.0f, 32), material);
        vizGO.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        vizGO.transform.position = position + new Vector3(0.0f, 0.1f, 0.0f);
        vizGO.transform.SetParent(transform);
    }
}
