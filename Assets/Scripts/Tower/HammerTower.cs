using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HammerTower : BaseTower
{
    private Animator _animator;
    private List<GameObject> _enemiesInRange = new List<GameObject>();

    public override void Start()
    {
        base.Start();
        _animator = GetComponent<Animator>();
        _animator.SetBool("IsHammering", false);
    }

    protected List<GameObject> GetEnemiesInRange()
    {
        var enemiesInRange = _enemyManager.QueryEnemiesCircle(towerCenter.position, Range);
        return enemiesInRange;
    }

    public void OnHammerAnimationHit()
    {
        _animator.SetBool("IsHammering", false);
        List<GameObject> newEnemiesInRange = GetEnemiesInRange();
        if (_enemiesInRange != null)
        {
            _enemiesInRange = _enemiesInRange.Intersect(newEnemiesInRange).ToList();
        } else
        {
            _enemiesInRange = newEnemiesInRange;
        }

        for (int i = 0; i < _enemiesInRange.Count; i++)
        {
            if (_enemiesInRange[i] != null)
                DamageEnemy(_enemiesInRange[i]);
        }
        _enemiesInRange = null;
    }

    protected override void TowerUpdate()
    {
        _shootDelayTimer -= Time.deltaTime;
        List<GameObject> enemies = GetEnemiesInRange();
        if (_shootDelayTimer <= 0.0f)
        {
            if (enemies != null && enemies.Count > 0)
            {
                _enemiesInRange = enemies;
                _animator.SetBool("IsHammering", true);
                _shootDelayTimer = ShootDelay;
            }
        } else
        {
            // Not sure why but without this, the animation sometimes plays twice.
            _animator.SetBool("IsHammering", false);
        }
    }

    void DamageEnemy(GameObject enemy)
    {
        enemy.GetComponent<BaseEnemy>().TakeDamage(BulletDamage);
    }
}
