using UnityEngine;

public class InstantBullet : Bullet
{
    protected override void StepTowardsEnemy()
    {
        BulletHitTarget();
    }
}
