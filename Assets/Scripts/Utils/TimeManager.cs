using UnityEngine;

public class TimeManager : MonoBehaviour
{
    void Start()
    {
        EventBus.Instance.Register<EnemyWaveEndEvent>(OnEnemyWaveEndHandler);
        EventBus.Instance.Register<GameOverEvent>(OnGameOverHandler);
    }

    void OnEnemyWaveEndHandler(EnemyWaveEndEvent evnt)
    {
        Time.timeScale = 1.0f;
    }

    void OnGameOverHandler(GameOverEvent evnt)
    {
        Time.timeScale = 1.0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 1.0f;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 2.0f;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Time.timeScale = 3.0f;
        }
    }
}