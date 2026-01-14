#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BalanceInspectorWindow : EditorWindow
{
    // ---------- JSON DTOs ----------
    [Serializable] private class TowersRoot { public List<TowerDef> Towers; }
    [Serializable] private class EnemiesRoot { public List<EnemyDef> Enemies; }
    [Serializable] private class WavesRoot { public List<WaveDef> Waves; }

    [Serializable]
    private class TowerDef
    {
        public string TowerName;
        public string TowerShape;
        public string TowerDescription;
        public int Price;
        public float Range;
        public float FireTimer;
        public float BulletDamage;
    }

    [Serializable]
    private class EnemyDef
    {
        public string EnemyName;
        public int Health;
        public float Speed;
        public int Reward;
        public bool IsBoss;
    }

    [Serializable]
    private class WaveDef
    {
        public int WaveNumber;
        public float BuildDurationSeconds;
        public int WaveReward;
        public List<WaveEnemyEntry> Enemies;
    }

    [Serializable]
    private class WaveEnemyEntry
    {
        public string EnemyName;
        public int Quantity;
        public float TimeBetweenReleaseSeconds;
    }

    // ---------- Computed rows ----------
    private class TowerRow
    {
        public string Name;
        public int Price;
        public float Range;
        public float FireTimer;
        public float Damage;
        public float DPS;
        public float DPSPerPrice;
        public float DPSRangePerPrice;
    }

    private class WaveRow
    {
        public int WaveNumber;
        public float BuildDuration;
        public int Reward;

        public int TotalHealth;
        public float ReleaseDuration;
        public float AvgHealthPerSecond;
        public float PeakHealthPerSecond; // max segment rate
        public int LargestSpikeHealth;     // for interval == 0 cases
    }

    // ---------- Data ----------
    private const string ResourcesBasePath = "Data/"; // Resources/Data/*.json (loaded without extension)

    private TowersRoot _towersRoot;
    private EnemiesRoot _enemiesRoot;
    private WavesRoot _wavesRoot;

    private Dictionary<string, EnemyDef> _enemyByName;

    private List<TowerRow> _towerRows = new();
    private List<WaveRow> _waveRows = new();

    // ---------- UI state ----------
    private Vector2 _scroll;
    private bool _showTowers = true;
    private bool _showWaves = true;

    private enum TowerSort { DPSRangePerPrice, DPSPerPrice, DPS, Price, Range, Name }
    private enum WaveSort { WaveNumber, AvgHps, PeakHps, TotalHealth, ReleaseDuration, LargestSpike }

    private TowerSort _towerSort = TowerSort.DPSRangePerPrice;
    private WaveSort _waveSort = WaveSort.WaveNumber;
    private bool _towerSortDescending = true;
    private bool _waveSortDescending = false;

    [MenuItem("Custom Window/Balance Inspector")]
    public static void Open()
    {
        var w = GetWindow<BalanceInspectorWindow>("Balance Inspector");
        w.minSize = new Vector2(820, 450);
        w.Show();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        _towersRoot = LoadJson<TowersRoot>(ResourcesBasePath + "Towers");
        _enemiesRoot = LoadJson<EnemiesRoot>(ResourcesBasePath + "Enemies");
        _wavesRoot = LoadJson<WavesRoot>(ResourcesBasePath + "Waves");

        _enemyByName = new Dictionary<string, EnemyDef>(StringComparer.OrdinalIgnoreCase);
        if (_enemiesRoot?.Enemies != null)
        {
            foreach (var e in _enemiesRoot.Enemies)
            {
                if (!string.IsNullOrWhiteSpace(e.EnemyName))
                    _enemyByName[e.EnemyName] = e;
            }
        }

        BuildTowerRows();
        BuildWaveRows();

        Repaint();
    }

    private static T LoadJson<T>(string resourcesPath) where T : class
    {
        var ta = Resources.Load<TextAsset>(resourcesPath);
        if (!ta)
            return null;

        try
        {
            return JsonUtility.FromJson<T>(ta.text);
        }
        catch
        {
            return null;
        }
    }

    private void BuildTowerRows()
    {
        _towerRows.Clear();
        if (_towersRoot?.Towers == null) return;

        foreach (var t in _towersRoot.Towers)
        {
            if (t == null) continue;

            // DPS rule:
            // - If FireTimer <= 0, treat BulletDamage as DPS (e.g. Laser)
            // - Else DPS = BulletDamage / FireTimer
            float dps = (t.FireTimer <= 0.0001f) ? t.BulletDamage : (t.BulletDamage / t.FireTimer);

            float dpsPerPrice = (t.Price > 0) ? (dps / t.Price) : 0f;
            float dpsRangePerPrice = (t.Price > 0) ? (dps * t.Range / t.Price) : 0f;

            _towerRows.Add(new TowerRow
            {
                Name = t.TowerName,
                Price = t.Price,
                Range = t.Range,
                FireTimer = t.FireTimer,
                Damage = t.BulletDamage,
                DPS = dps,
                DPSPerPrice = dpsPerPrice,
                DPSRangePerPrice = dpsRangePerPrice
            });
        }

        ApplyTowerSort();
    }

    private void BuildWaveRows()
    {
        _waveRows.Clear();
        if (_wavesRoot?.Waves == null) return;

        foreach (var w in _wavesRoot.Waves)
        {
            if (w == null) continue;

            int totalHealth = 0;
            float releaseDuration = 0f;

            float peakHps = 0f;
            int largestSpike = 0;

            if (w.Enemies != null)
            {
                foreach (var entry in w.Enemies)
                {
                    if (entry == null || entry.Quantity <= 0) continue;

                    if (!_enemyByName.TryGetValue(entry.EnemyName ?? "", out var enemy))
                        continue; // unknown enemy name -> ignore for metrics

                    int groupHealth = enemy.Health * entry.Quantity;
                    totalHealth += groupHealth;

                    float interval = Mathf.Max(0f, entry.TimeBetweenReleaseSeconds);

                    // Duration model:
                    // For a group released sequentially, first spawns at t=0, last at t=(qty-1)*interval
                    // Groups are assumed sequential in the JSON order.
                    float groupDuration = (entry.Quantity <= 1) ? 0f : (entry.Quantity - 1) * interval;
                    releaseDuration += groupDuration;

                    // Pressure model (HP per second):
                    // If interval > 0: during this group, you're effectively receiving enemy.Health each interval,
                    // so HP/s ~ enemy.Health / interval (independent of quantity, while the group lasts).
                    // If interval == 0: it's a spike (all at once).
                    if (interval > 0.0001f)
                    {
                        float hps = enemy.Health / interval;
                        if (hps > peakHps) peakHps = hps;
                    }
                    else
                    {
                        if (groupHealth > largestSpike) largestSpike = groupHealth;
                    }
                }
            }

            float avgHps = 0f;
            if (releaseDuration > 0.0001f)
                avgHps = totalHealth / releaseDuration;
            else
                avgHps = (totalHealth > 0) ? float.PositiveInfinity : 0f;

            _waveRows.Add(new WaveRow
            {
                WaveNumber = w.WaveNumber,
                BuildDuration = w.BuildDurationSeconds,
                Reward = w.WaveReward,
                TotalHealth = totalHealth,
                ReleaseDuration = releaseDuration,
                AvgHealthPerSecond = avgHps,
                PeakHealthPerSecond = peakHps,
                LargestSpikeHealth = largestSpike
            });
        }

        ApplyWaveSort();
    }

    private void ApplyTowerSort()
    {
        IOrderedEnumerable<TowerRow> ordered = _towerRows.OrderBy(r => 0);
        switch (_towerSort)
        {
            case TowerSort.DPSRangePerPrice: ordered = _towerRows.OrderBy(r => r.DPSRangePerPrice); break;
            case TowerSort.DPSPerPrice:      ordered = _towerRows.OrderBy(r => r.DPSPerPrice); break;
            case TowerSort.DPS:              ordered = _towerRows.OrderBy(r => r.DPS); break;
            case TowerSort.Price:            ordered = _towerRows.OrderBy(r => r.Price); break;
            case TowerSort.Range:            ordered = _towerRows.OrderBy(r => r.Range); break;
            case TowerSort.Name:             ordered = _towerRows.OrderBy(r => r.Name); break;
        }
        _towerRows = (_towerSortDescending ? ordered.Reverse() : ordered).ToList();
    }

    private void ApplyWaveSort()
    {
        IOrderedEnumerable<WaveRow> ordered = _waveRows.OrderBy(r => 0);
        switch (_waveSort)
        {
            case WaveSort.WaveNumber:      ordered = _waveRows.OrderBy(r => r.WaveNumber); break;
            case WaveSort.AvgHps:          ordered = _waveRows.OrderBy(r => r.AvgHealthPerSecond); break;
            case WaveSort.PeakHps:         ordered = _waveRows.OrderBy(r => r.PeakHealthPerSecond); break;
            case WaveSort.TotalHealth:     ordered = _waveRows.OrderBy(r => r.TotalHealth); break;
            case WaveSort.ReleaseDuration: ordered = _waveRows.OrderBy(r => r.ReleaseDuration); break;
            case WaveSort.LargestSpike:    ordered = _waveRows.OrderBy(r => r.LargestSpikeHealth); break;
        }
        _waveRows = (_waveSortDescending ? ordered.Reverse() : ordered).ToList();
    }

    private void OnGUI()
    {
        DrawTopBar();

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        DrawLoadStatus();

        EditorGUILayout.Space(8);

        _showTowers = EditorGUILayout.Foldout(_showTowers, "Towers (DPS / Price, DPS*Range / Price)", true);
        if (_showTowers) DrawTowersTable();

        EditorGUILayout.Space(10);

        _showWaves = EditorGUILayout.Foldout(_showWaves, "Waves (HP per second pressure)", true);
        if (_showWaves) DrawWavesTable();

        EditorGUILayout.EndScrollView();
    }

    private void DrawTopBar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                Refresh();

            GUILayout.Space(10);
            GUILayout.Label("Loads Resources/Data/{Towers,Enemies,Waves}.json", EditorStyles.miniLabel);

            GUILayout.FlexibleSpace();
        }
    }

    private void DrawLoadStatus()
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            bool okT = _towersRoot?.Towers != null;
            bool okE = _enemiesRoot?.Enemies != null;
            bool okW = _wavesRoot?.Waves != null;

            EditorGUILayout.LabelField("Load Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Towers.json", okT ? $"OK ({_towersRoot.Towers.Count})" : "Missing/Invalid");
            EditorGUILayout.LabelField("Enemies.json", okE ? $"OK ({_enemiesRoot.Enemies.Count})" : "Missing/Invalid");
            EditorGUILayout.LabelField("Waves.json", okW ? $"OK ({_wavesRoot.Waves.Count})" : "Missing/Invalid");

            if (okW && okE)
            {
                // quick warning if waves reference unknown enemies
                var unknown = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var w in _wavesRoot.Waves)
                {
                    if (w?.Enemies == null) continue;
                    foreach (var e in w.Enemies)
                    {
                        var name = e?.EnemyName ?? "";
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        if (!_enemyByName.ContainsKey(name)) unknown.Add(name);
                    }
                }

                if (unknown.Count > 0)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField("Unknown enemy references in Waves.json:", EditorStyles.miniBoldLabel);
                    EditorGUILayout.LabelField(string.Join(", ", unknown), EditorStyles.wordWrappedMiniLabel);
                }
            }
        }
    }

    private void DrawTowersTable()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Sort", GUILayout.Width(30));
            var newSort = (TowerSort)EditorGUILayout.EnumPopup(_towerSort, GUILayout.Width(170));
            if (newSort != _towerSort) { _towerSort = newSort; ApplyTowerSort(); }

            _towerSortDescending = EditorGUILayout.ToggleLeft("Descending", _towerSortDescending, GUILayout.Width(100));
            if (GUILayout.Button("Apply", GUILayout.Width(60))) ApplyTowerSort();

            GUILayout.FlexibleSpace();
        }

        EditorGUILayout.Space(4);

        DrawTableHeader(new[]
        {
            ("Tower", 160.0f),
            ("Price", 60.0f),
            ("Range", 60.0f),
            ("FireTimer", 70.0f),
            ("Dmg", 60.0f),
            ("DPS", 70.0f),
            ("DPS/Price", 90.0f),
            ("(DPS*Range)/Price", 130.0f),
        });

        foreach (var r in _towerRows)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawCell(r.Name, 160);
                DrawCell(r.Price.ToString(), 60);
                DrawCell(r.Range.ToString("0.00"), 60);
                DrawCell(r.FireTimer.ToString("0.00"), 70);
                DrawCell(r.Damage.ToString("0.00"), 60);
                DrawCell(r.DPS.ToString("0.00"), 70);
                DrawCell(r.DPSPerPrice.ToString("0.000"), 90);
                DrawCell(r.DPSRangePerPrice.ToString("0.000"), 130);
            }
        }
    }

    private void DrawWavesTable()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Sort", GUILayout.Width(30));
            var newSort = (WaveSort)EditorGUILayout.EnumPopup(_waveSort, GUILayout.Width(170));
            if (newSort != _waveSort) { _waveSort = newSort; ApplyWaveSort(); }

            _waveSortDescending = EditorGUILayout.ToggleLeft("Descending", _waveSortDescending, GUILayout.Width(100));
            if (GUILayout.Button("Apply", GUILayout.Width(60))) ApplyWaveSort();

            GUILayout.FlexibleSpace();
        }

        EditorGUILayout.Space(4);

        DrawTableHeader(new[]
        {
            ("Wave", 50.0f),
            ("Build (s)", 70.0f),
            ("Reward", 60.0f),
            ("Total HP", 70.0f),
            ("Release (s)", 80.0f),
            ("Avg HP/s", 80.0f),
            ("Peak HP/s", 80.0f),
            ("Largest Spike", 90.0f),
        });

        foreach (var r in _waveRows)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawCell(r.WaveNumber.ToString(), 50);
                DrawCell(r.BuildDuration.ToString("0.0"), 70);
                DrawCell(r.Reward.ToString(), 60);
                DrawCell(r.TotalHealth.ToString(), 70);
                DrawCell(r.ReleaseDuration.ToString("0.0"), 80);

                string avg = float.IsPositiveInfinity(r.AvgHealthPerSecond) ? "∞" : r.AvgHealthPerSecond.ToString("0.0");
                DrawCell(avg, 80);

                DrawCell(r.PeakHealthPerSecond.ToString("0.0"), 80);
                DrawCell(r.LargestSpikeHealth.ToString(), 90);
            }
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox(
            "Notes:\n" +
            "- Release(s) assumes each group releases sequentially; duration per group is (Quantity-1)*TimeBetweenReleaseSeconds.\n" +
            "- Peak HP/s is max(EnemyHealth / interval) over groups with interval > 0.\n" +
            "- interval == 0 contributes to 'Largest Spike' (instant health added).",
            MessageType.Info);
    }

    // ---------- small UI helpers ----------
    private static void DrawTableHeader((string label, float width)[] cols)
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            foreach (var c in cols)
                GUILayout.Label(c.label, EditorStyles.miniBoldLabel, GUILayout.Width(c.width));
        }
    }

    private static void DrawCell(string text, float width)
    {
        GUILayout.Label(text ?? "", EditorStyles.label, GUILayout.Width(width));
    }
}

#endif