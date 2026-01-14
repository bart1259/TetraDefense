using UnityEngine;

public class EnemyWaddle : MonoBehaviour
{
    public float waddleSpeed = 1.0f;
    public float maxWaddleAngle = 30.0f;

    private float _randomOffset;

    void Start()
    {
        _randomOffset = Random.Range(0.0f, 1.0f) * 1000.0f;
    }

    void Update()
    {
        float angle = maxWaddleAngle * Mathf.Sin(Time.time * waddleSpeed + _randomOffset);
        transform.localRotation = Quaternion.Euler(angle-90, 90, 90);
    }
}
