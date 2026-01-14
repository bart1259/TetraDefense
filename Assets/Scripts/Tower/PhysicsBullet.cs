using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsBullet : Bullet
{
    public float InitalYVelocity = 20.0f;
    public float Gravity = -9.81f;
    public bool AOE = false;
    public float AOEMultiplier = 0.8f;
    public float AOERange = 0.5f;

    private float _yVelocity;
    private float _flightTime;
    private Vector3 _plannedTarget = Vector3.zero;
    private float _horizontalSpeed;

    void Start()
    {
        _yVelocity = InitalYVelocity;
        _flightTime = PhysicsUtils.GetFlightTime(transform.position.y, _yVelocity, Gravity, targetEnemy.transform.position.y);
        _plannedTarget = PhysicsUtils.GetPositionAtTime(
            targetEnemy.GetComponent<EnemyMotor>().Path,
            _flightTime,
            targetEnemy.GetComponent<EnemyMotor>().Progress,
            targetEnemy.GetComponent<EnemyMotor>().Speed
        );
        _horizontalSpeed = Vector3.Distance(
            new Vector3(transform.position.x, 0.0f, transform.position.z),
            new Vector3(_plannedTarget.x, 0.0f, _plannedTarget.z)
        ) / _flightTime;
    }

    //FIXME: Eventually we may want to add better predictive calculations and shoot where the 
    // enemy will be (probably the burden of the enemy motor)
    protected override void StepTowardsEnemy()
    {
        Vector3 target = _plannedTarget;

        // Update x and y position seperately
        _yVelocity += Gravity * Time.deltaTime;

        // Horizontal motion
        Vector3 current = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Vector3 horizontalTarget = new Vector3(target.x, 0.0f, target.z);

        Vector3 horizontalNewPos = Vector3.MoveTowards(current, horizontalTarget, _horizontalSpeed * Time.deltaTime);
        float newYPos = transform.position.y + _yVelocity * Time.deltaTime;

        transform.position = new Vector3(horizontalNewPos.x, newYPos, horizontalNewPos.z);

        if (transform.position.y < target.y)
        {
            BulletHitTarget();
        }
    }

    protected override void BulletHitTarget()
    {
        if (!AOE)
            base.BulletHitTarget();
        else {
            EnemyManager enemyManager = FindFirstObjectByType<EnemyManager>();
            List<GameObject> enemies = enemyManager.QueryEnemiesCircle(transform.position, AOERange);
            foreach (var enemy in enemies) {
                enemy.GetComponent<BaseEnemy>().TakeDamage(AOEMultiplier * Damage);
            }
            base.BulletHitTarget();
        }
    }
}
