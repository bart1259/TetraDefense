using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    public TMP_Text WaveText;
    public TMP_Text BuildTimerText;

    private EnemyManager _enemyManager;

    void Start()
    {
        _enemyManager = FindFirstObjectByType<EnemyManager>();
    }


    // Update is called once per frame
    void Update()
    {
        WaveText.text = "Wave: " + _enemyManager.CurrentWaveIndex.ToString();

        if (_enemyManager.WaveState == EnemySpawnState.WaitingToStartNextWave)
        {
            BuildTimerText.gameObject.SetActive(true);
            int minutes = Mathf.FloorToInt(_enemyManager.BuildTimer / 60f);
            int seconds = Mathf.CeilToInt(_enemyManager.BuildTimer % 60f);
            BuildTimerText.text = "Next Wave In: " + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
        else
        {
            BuildTimerText.gameObject.SetActive(false);
        }
    }
}
