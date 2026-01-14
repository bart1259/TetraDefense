using UnityEngine;
using System.Linq;

public class BaseTower : MonoBehaviour
{
    public float Range => _towerData.Range;
    public float ShootDelay => _towerData.FireTimer;
    public float BulletDamage => _towerData.BulletDamage;
    public GameObject bulletPrefab;
    public Transform towerCenter;
    public Transform bulletSpawnLocation;
    protected TowerSO _towerData;
    protected EnemyManager _enemyManager;
    protected float _shootDelayTimer = 0.0f;


    public virtual void Start()
    {
        // Find enemyManager
        _enemyManager = FindFirstObjectByType<EnemyManager>();
        _towerData = GetComponentInParent<TowerSOHolder>().TowerSOData;
    }


    protected virtual GameObject GetEnemyInRange()
    {
        var enemiesInRange = _enemyManager.QueryEnemiesCircle(towerCenter.position, Range);
        enemiesInRange = enemiesInRange.Where(e => !e.GetComponent<BaseEnemy>().GoingToDie).OrderBy(e => -e.GetComponent<EnemyMotor>().Progress).ToList(); // Get furthest enemies
        if (enemiesInRange.Count > 0)
        {
            return enemiesInRange[0];
        }
        return null;
    }

    void Update()
    {
        TowerUpdate();
    }

    protected virtual void TowerUpdate()
    {
        _shootDelayTimer -= Time.deltaTime;
        GameObject enemy = GetEnemyInRange();
        if (enemy != null)
            TargetEnemy(enemy);
        if (_shootDelayTimer <= 0.0f)
        {
            if (enemy != null)
            {
                ShootAtEnemy(enemy);
                _shootDelayTimer = ShootDelay;
            }
        }
    }

    protected virtual void ShootAtEnemy(GameObject enemy)
    {
        GameObject bulletGO = GameObject.Instantiate(bulletPrefab, bulletSpawnLocation.position, Quaternion.identity);
        bulletGO.GetComponent<Bullet>().SetTarget(enemy);
        bulletGO.GetComponent<Bullet>().Damage = BulletDamage;
        if (enemy.GetComponent<BaseEnemy>().Health - BulletDamage <= 0)
        {
            enemy.GetComponent<BaseEnemy>().DamageInFlight += BulletDamage;
        }
    }

    protected virtual void TargetEnemy(GameObject enemy)
    {
        
    }
}
