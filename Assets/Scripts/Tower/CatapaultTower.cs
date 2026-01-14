using UnityEngine;
using System.Linq;

public class CatapaultTower : BaseTower
{
    private Animator _animator;
    private GameObject _currentTarget;

    public override void Start()
    {
        base.Start();
        _animator = GetComponent<Animator>();
        _animator.SetBool("IsThrowing", false);
    }


    protected override void ShootAtEnemy(GameObject enemy)
    {
        _currentTarget = enemy;
        _animator.SetBool("IsThrowing", true);
        if (enemy.GetComponent<BaseEnemy>().Health - BulletDamage <= 0)
        {
            enemy.GetComponent<BaseEnemy>().DamageInFlight += BulletDamage;
        }
    }

    public void ThrowRock()
    {
        GameObject enemy = GetEnemyInRange();
        if (_currentTarget != null)
        {
            GameObject rockGO = GameObject.Instantiate(bulletPrefab, bulletSpawnLocation.position, Quaternion.identity);
            rockGO.GetComponent<Bullet>().SetTarget(_currentTarget);
            rockGO.GetComponent<Bullet>().Damage = BulletDamage;
        }
        _animator.SetBool("IsThrowing", false);
    }

}
