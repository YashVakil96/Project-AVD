using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using YashVakil96.UnityTools.Patterns;

public class WaveManager : Singleton<WaveManager>
{
    // ----------------- DATA -----------------
    [System.Serializable]
    public struct WaveEntry
    {
        public GameObject enemyPrefab;   // Chaser / Diver prefab
        public int count;                // how many to spawn
        public float spawnInterval;      // seconds between spawns for this entry
    }

    [System.Serializable]
    public class Wave
    {
        public string name = "Wave";
        public List<WaveEntry> entries = new List<WaveEntry>();
        public float restAfterWave = 4f; // intermission after this wave clears
    }

    // ----------------- INSPECTOR -----------------
    [Header("Spawn Points (edge empties around arena)")]
    public Transform[] spawnPoints;

    [Header("Waves")]
    public List<Wave> waves = new List<Wave>();
    public bool loopWaves = true;          // cycle after last wave
    public float loopScaleFactor = 1.25f;  // scale counts each loop

    [Header("Rules")]
    public float startDelay = 2f;
    public float minSpawnDistanceFromPlayer = 4f;
    public int   maxAliveCap = 30;

    [Header("References")]
    public Transform player;               // will auto-bind by Player tag if empty
    public TMPro.TextMeshProUGUI waveText; // optional HUD label (Wave N / name)

    // ----------------- STATE -----------------
    private int _currentWaveIndex = -1;
    private int _alive = 0;
    private bool _running = false;
    private Coroutine _runner;

    // This manager is scene-dependent: we place it in-scene, no auto-create
    protected override bool AutoCreate => false;

    protected override void OnSingletonReady()
    {
        TryBindSceneRefs();
    }

    protected override void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        // If this persists, rebind scene refs when the active scene changes
        TryBindSceneRefs();
    }

    private void TryBindSceneRefs()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // If you prefer automatic spawn point discovery:
        // Look for a GameObject named "SpawnPoints" and use its children.
        if ((spawnPoints == null || spawnPoints.Length == 0))
        {
            var container = GameObject.Find("SpawnPoints");
            if (container)
            {
                var list = new List<Transform>();
                foreach (Transform t in container.transform)
                    if (t != container.transform) list.Add(t);
                spawnPoints = list.ToArray();
            }
        }
    }

    // ----------------- PUBLIC API -----------------
    [Button("Start Wave")]
    public void StartWaves()
    {
        if (_runner != null) StopCoroutine(_runner);
        _running = true;
        _currentWaveIndex = -1;
        _alive = 0;
        _runner = StartCoroutine(RunAllWaves());
    }

    public void StopWaves(bool killRemaining = false)
    {
        _running = false;
        if (_runner != null) StopCoroutine(_runner);
        _runner = null;

        if (killRemaining)
        {
            foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
                Destroy(e);
            _alive = 0;
        }
    }

    /// <summary>Call from EnemyHealth.Die()</summary>
    public void ReportEnemyDeath()
    {
        _alive = Mathf.Max(0, _alive - 1);
    }

    /// <summary>Optional: if you spawn enemies elsewhere and want the cap to work.</summary>
    public void ReportEnemySpawn()
    {
        _alive++;
    }

    // ----------------- CORE LOOP -----------------

    private IEnumerator RunAllWaves()
    {
        yield return new WaitForSeconds(startDelay);

        while (_running)
        {
            // Advance wave index, handle looping
            _currentWaveIndex++;
            if (_currentWaveIndex >= waves.Count)
            {
                if (!loopWaves) yield break;
                _currentWaveIndex = 0;
                ScaleWaves(loopScaleFactor);
            }

            var w = waves[_currentWaveIndex];
            UpdateWaveHud(w.name, _currentWaveIndex + 1);

            // Spawn each entry
            foreach (var entry in w.entries)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    // Alive cap throttle
                    while (_alive >= maxAliveCap && _running)
                        yield return null;

                    var sp = PickSpawnPoint();
                    if (sp != null)
                        Spawn(entry.enemyPrefab, sp.position);

                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }

            // Wait for wave clear
            while (_alive > 0 && _running)
                yield return null;

            if (!_running) yield break;

            // Intermission
            if (w.restAfterWave > 0f)
                yield return new WaitForSeconds(w.restAfterWave);
        }
    }

    private void Spawn(GameObject prefab, Vector3 pos)
    {
        var go = Instantiate(prefab, pos, Quaternion.identity);
        // Ensure tag & layer are correct for collisions/damage
        if (go.tag != "Enemy") go.tag = "Enemy";
        _alive++;
    }

    private Transform PickSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;

        // Try to respect min distance from player
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (!player) return sp;
            if (Vector2.Distance(sp.position, player.position) >= minSpawnDistanceFromPlayer)
                return sp;
        }

        // Fallback: just pick one
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    private void UpdateWaveHud(string waveName, int index)
    {
        if (!waveText) return;
        waveText.text = $"Wave {index}\n{waveName}";
    }

    private void ScaleWaves(float factor)
    {
        // Simple loop scaling: increase counts; optionally tighten intervals
        foreach (var w in waves)
        {
            for (int i = 0; i < w.entries.Count; i++)
            {
                var e = w.entries[i];
                e.count = Mathf.RoundToInt(e.count * factor);
                e.spawnInterval = Mathf.Max(0.05f, e.spawnInterval * (1f / (0.9f * factor)));
                w.entries[i] = e;
            }
        }
    }

    // ----------------- EDITOR GIZMOS -----------------
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = new Color(0f, 1f, 0.6f, 0.8f);
        foreach (var sp in spawnPoints)
        {
            if (!sp) continue;
            Gizmos.DrawWireSphere(sp.position, 0.25f);
            if (player)
            {
                var d = Vector2.Distance(sp.position, player.position);
                if (d < minSpawnDistanceFromPlayer)
                {
                    // visualize too-close spawns in red
                    Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
                    Gizmos.DrawLine(sp.position, player.position);
                    Gizmos.color = new Color(0f, 1f, 0.6f, 0.8f);
                }
            }
        }
    }
#endif
}
