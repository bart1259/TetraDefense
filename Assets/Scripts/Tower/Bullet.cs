using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float Speed = 10.0f;
    public float Damage = 10.0f;
    protected GameObject targetEnemy;
    protected Vector3 lastPosition;
    public GameObject destroyPrefab;

    public void SetTarget(GameObject enemy)
    {
        targetEnemy = enemy;
        lastPosition = transform.position;
    }

    protected virtual void StepTowardsEnemy()
    {
        Vector3 target = targetEnemy != null ? targetEnemy.transform.position : lastPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, Speed * Time.deltaTime);
        float distanceToTarget = (target - transform.position).magnitude;
        if (distanceToTarget < 0.1f)
        {
            BulletHitTarget();
        }
    }

    protected virtual void BulletHitTarget()
    {
        if (targetEnemy != null)
        {
            targetEnemy.GetComponent<BaseEnemy>().TakeDamage(Damage);
        }
        if (destroyPrefab != null) 
        {
            GameObject destroyGO = GameObject.Instantiate(destroyPrefab);
            destroyGO.transform.position = transform.position;
        }
        Destroy(gameObject);
    }

    void Update()
    {
        StepTowardsEnemy();
        if (targetEnemy != null)
        {
            lastPosition = targetEnemy.transform.position;
        }
    }
}
