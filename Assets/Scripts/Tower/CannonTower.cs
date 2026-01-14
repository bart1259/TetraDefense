using UnityEngine;
using System.Linq;

public class CannonTower : BaseTower
{
    public GameObject CannonHolder;
    public GameObject Cannons;
    public float heightOffsetAngle = 10.0f;
    public float rotationOffsetAngle = 0.0f;

    public CannonTower()
    {

    }

    protected override void TargetEnemy(GameObject enemy)
    {
        Quaternion target = Quaternion.Euler(new Vector3(
            -90,
            180-(Mathf.Atan2(
                enemy.transform.position.z - transform.position.z,
                enemy.transform.position.x - transform.position.x
            ) * Mathf.Rad2Deg) + rotationOffsetAngle,
            0
        ));

        CannonHolder.transform.rotation = Quaternion.Slerp(
            CannonHolder.transform.rotation,
            target,
            Time.deltaTime * 10.0f
        );

        Quaternion localTarget = Quaternion.Euler(new Vector3(
            0,
            heightOffsetAngle + Mathf.Atan2(
                enemy.transform.position.y - transform.position.y,
                Vector3.Distance(
                    new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z),
                    new Vector3(transform.position.x, 0, transform.position.z)
                )
            ) * Mathf.Rad2Deg,
            0
        ));

        Cannons.transform.localRotation = Quaternion.Slerp(
            Cannons.transform.localRotation,
            localTarget,
            Time.deltaTime * 10.0f
        );
    }
}
