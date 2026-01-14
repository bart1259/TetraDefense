using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnemySpawnState
{
    WaitingToStartNextWave,
    SpawningEnemies,
    EndGame
}

public class EnemyManager : MonoBehaviour
{

    public EnemySpawnState WaveState { get { return _enemySpawnState; } }
    public float BuildTimer { get { return _buildTimer; } }
    public float InitalBuildTime = 45.0f;

    private EnemySpawnState _enemySpawnState = EnemySpawnState.SpawningEnemies;
    private int _currentWaveIndex = 0; // 1-based index. 0 means before first wave
    public int CurrentWaveIndex { get { return _currentWaveIndex; } }
    private int _currentEnemySpawnIndex = 0;
    private int _currentEnemySpawnIndexIndex = 0;
    private float _enemySpawnTimer = 0.0f;
    private float _buildTimer = 0.0f;
    private EnemyWaveJSON _waveData;

    private PathFindingService _pathFindingService;
    public Transform SpawnLocation;
    public Transform TargetLocation;

    private List<GameObject> _enemies = new List<GameObject>();
    private List<EnemySO> _enemyTypes = new List<EnemySO>();

    private Economy _economy;


    void Start()
    {
        _pathFindingService = GameStateManager.GetInstance().PathfindingService;
        _enemyTypes = new List<EnemySO>(Resources.LoadAll<EnemySO>(""));
        _currentWaveIndex = 0;
        _currentEnemySpawnIndexIndex = 0;
        _economy = GameStateManager.GetInstance().Economy;
        _waveData = LoadWavesFromJSON();
        _buildTimer = InitalBuildTime;
        _enemySpawnState = EnemySpawnState.WaitingToStartNextWave;

        EventBus.Instance.Register<EnemyWaveEndEvent>(OnWaveEndHandler);
        EventBus.Instance.Register<GameOverEvent>(OnGameOverHandler);
    }

    private EnemySO GetEnemySOByName(string enemyName)
    {
        foreach (EnemySO enemySO in _enemyTypes)
        {
            if (enemySO.EnemyName == enemyName)
            {
                return enemySO;
            }
        }
        return null;
    }

    void OnWaveEndHandler(EnemyWaveEndEvent evnt)
    {
        if(evnt.gameWon)
            PauseEnemySpawning();
    }

    void OnGameOverHandler(GameOverEvent evnt)
    {
        PauseEnemySpawning();
    }

    public void PauseEnemySpawning()
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (_enemies[i] != null)
            {
                _enemies[i].GetComponent<EnemyMotor>().PauseMovement();
            }
        }
        _enemySpawnState = EnemySpawnState.EndGame;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && _enemySpawnState == EnemySpawnState.WaitingToStartNextWave)
        {
            // Skip to the next wave
            _buildTimer = 0.0f;
        }

        switch (_enemySpawnState)
        {
            case EnemySpawnState.SpawningEnemies:
                {
                    WaveInfoJSON currentWave = _waveData.Waves[_currentWaveIndex-1];

                    if (_currentEnemySpawnIndex < currentWave.Enemies.Length)
                    {
                        // Keep Spawning enemies
                        EnemySpawnInfoJSON currentSpawnInfo = currentWave.Enemies[_currentEnemySpawnIndex];
                        if (_enemySpawnTimer <= 0.0f)
                        {
                            SpawnEnemy(GetEnemySOByName(currentSpawnInfo.EnemyName));
                            _enemySpawnTimer = currentSpawnInfo.TimeBetweenReleaseSeconds;
                            _currentEnemySpawnIndexIndex += 1;
                            if (_currentEnemySpawnIndexIndex >= currentSpawnInfo.Quantity)
                            {
                                _currentEnemySpawnIndex += 1;
                                _currentEnemySpawnIndexIndex = 0;
                            }
                        }
                        else
                        {
                            _enemySpawnTimer -= Time.deltaTime;
                        }
                    }
                    else
                    {
                        // Ensure all enemies are dead before starting next wave
                        CleanUpDeadEnemies();
                        if (_enemies.Count > 0)
                            break;
                        // Finished spawning all enemies for this wave
                        EventBus.Instance.Publish(new EnemyWaveEndEvent(_currentWaveIndex, _currentWaveIndex == _waveData.Waves.Length));
                        _economy.AddMoney(currentWave.WaveReward);
                        _enemySpawnState = EnemySpawnState.WaitingToStartNextWave;
                        _buildTimer = currentWave.BuildDurationSeconds;
                    }
                }
                break;
            case EnemySpawnState.WaitingToStartNextWave:
                {
                    if (_buildTimer <= 0.0f)
                    {
                        // Start next wave
                        _currentWaveIndex += 1;
                        _currentEnemySpawnIndex = 0;
                        _currentEnemySpawnIndexIndex = 0;
                        _enemySpawnState = EnemySpawnState.SpawningEnemies;
                        
                        EventBus.Instance.Publish(new EnemyWaveStartEvent(_currentWaveIndex));
                    }
                    else
                    {
                        _buildTimer -= Time.deltaTime;
                    }
                }
                break;
            case EnemySpawnState.EndGame:
                break;
        }
    }

    void CleanUpDeadEnemies()
    {
        _enemies.RemoveAll(e => e == null);
    }

    /// <summary>
    /// Queries enemties in circle, ignores y axis
    /// </summary>
    public List<GameObject> QueryEnemiesCircle(Vector3 center, float radius)
    {
        center = Vector3.Scale(center, new Vector3(1, 0, 1));

        CleanUpDeadEnemies();
        List<GameObject> enemiesInRange = new List<GameObject>();
        foreach (GameObject enemy in _enemies)
        {
            Vector3 enemyPos = Vector3.Scale(enemy.transform.position, new Vector3(1, 0, 1));
            if ((center - enemyPos).sqrMagnitude <= radius * radius)
            {
                enemiesInRange.Add(enemy);
            }
        }
        return enemiesInRange;
    }

    void SpawnEnemy(EnemySO _enemyData)
    {
        GameObject newEnemyGO = GameObject.Instantiate(_enemyData.EnemyPrefab);
        newEnemyGO.GetComponent<EnemySOHolder>().SetEnemyData(_enemyData);
        newEnemyGO.GetComponentInChildren<EnemyMotor>().SetPath(_pathFindingService.FindBestPath(), TargetLocation);
        newEnemyGO.transform.position = SpawnLocation.position;
        _enemies.Add(newEnemyGO);
    }


    // Enemy Wave Loader
    [System.Serializable]
    private struct EnemySpawnInfoJSON
    {
        public string EnemyName;
        public int Quantity;
        public float TimeBetweenReleaseSeconds;
    }

    [System.Serializable]
    private struct WaveInfoJSON
    {
        public int WaveNumber;
        public float BuildDurationSeconds;
        public int WaveReward;
        public EnemySpawnInfoJSON[] Enemies;
    }

    [System.Serializable]
    private struct EnemyWaveJSON
    {
        public WaveInfoJSON[] Waves;
    }

    private EnemyWaveJSON LoadWavesFromJSON(string jsonPath="Data/Waves")
    {
        TextAsset jsonTextAsset = Resources.Load<TextAsset>(jsonPath);
        return JsonUtility.FromJson<EnemyWaveJSON>(jsonTextAsset.text);
    }
}