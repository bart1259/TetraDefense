using UnityEngine;
using System.Linq;

public class LaserTower : BaseTower
{
    public GameObject laserHead;
    public GameObject laserBeamStart;
    public LineRenderer lineRenderer;
    public MeshRenderer[] otherBeams;
    public float shootDelay = 0.5f;
    private float _shootTimer = 0;

    public override void Start()
    {
        base.Start();

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

    }

    void Update()
    {
        TowerUpdate();
    }

    protected override void TowerUpdate()
    {
        // No shooting delay
        // _shootDelayTimer -= Time.deltaTime;
        GameObject enemy = GetEnemyInRange();

        if (enemy != null)
        {
            foreach (var beam in otherBeams)
            {
                beam.enabled = true;
            }
            _shootTimer -= Time.deltaTime;
            if (_shootTimer <= 0.0f)
                ShootAtEnemy(enemy);
            
        } else
        {
            _shootTimer += Time.deltaTime;
            foreach (var beam in otherBeams)
            {
                beam.enabled = false;
            }
            lineRenderer.enabled = false;
        }

        _shootTimer = Mathf.Clamp(_shootTimer, 0.0f, shootDelay);
    }

    protected override void ShootAtEnemy(GameObject enemy)
    {
        lineRenderer.enabled = true;
        Vector3 startPos = laserBeamStart.transform.position;
        Vector3 endPos = enemy.transform.position;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        laserHead.transform.eulerAngles = new Vector3(
            -90,
            -(Mathf.Atan2(
                endPos.z - startPos.z,
                endPos.x - startPos.x
            ) * Mathf.Rad2Deg),
            0
        );

        enemy.GetComponent<BaseEnemy>().TakeDamage(BulletDamage * Time.deltaTime);
    }

}
