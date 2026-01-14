using UnityEngine;

public class DestroyIn : MonoBehaviour
{
    public float Seconds;
    private float _timer;

    public void Start()
    {
        _timer = Seconds;
    }

    public void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0.0f) {
            Destroy(gameObject);
        }
    }
}
