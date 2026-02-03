using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterInGameManager : MonoBehaviour
{
    public static MonsterInGameManager Instance { get; private set; }

    [SerializeField] private MonsterData _data;
    [SerializeField] private ResourceSpawner _resourceSpawner;

    private List<Monster> _monsters = new List<Monster>();
    public MonsterData Data => _data;
    public IReadOnlyList<Monster> Monsters => _monsters;

    public event Action OnMonsterChanged;
    private const int MonstersRequiredForMerge = 3;

    private int _currentToolLevel;
    public int CurrentToolLevel => _currentToolLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _currentToolLevel = UpgradeManager.Instance?.Get(EUpgradeType.ToolLevel)?.Level ?? 0;
        UpgradeManager.OnDataChanged += OnUpgradeChanged;

        LoadMonsters();
    }

    private void OnDestroy()
    {
        UpgradeManager.OnDataChanged -= OnUpgradeChanged;
    }

    private void OnUpgradeChanged()
    {
        int newLevel = UpgradeManager.Instance?.Get(EUpgradeType.ToolLevel)?.Level ?? 0;
        if (_currentToolLevel == newLevel) return;
        _currentToolLevel = newLevel;
    }

    #region Spawn & Merge
    public bool TrySpawnMonster()
    {
        if (MonsterOutgameManager.Instance == null) return false;
        if (!MonsterOutgameManager.Instance.TrySpawn()) return false;

        Vector2? spawnPos = FindValidSpawnPosition();
        if (spawnPos == null) return false;

        CreateMonster(0, spawnPos.Value);
        return true;
    }

    public bool TryMerge()
    {
        if (MonsterOutgameManager.Instance == null) return false;
        if (!MonsterOutgameManager.Instance.TryMerge(out int sourceTier)) return false;

        var targets = _monsters.Where(m => m != null && m.Tier == sourceTier).Take(MonstersRequiredForMerge).ToList();
        if (targets.Count < MonstersRequiredForMerge) return false;

        Vector2 mergePos = Vector2.zero;
        foreach (var m in targets) mergePos += (Vector2)m.transform.position;
        mergePos /= targets.Count;

        foreach (var m in targets) { _monsters.Remove(m); m.OnMerged(); }

        CreateMonster(sourceTier + 1, mergePos);
        OnMonsterChanged?.Invoke();
        return true;
    }
    #endregion

    #region Load
    private void LoadMonsters()
    {
        if (MonsterOutgameManager.Instance == null || _data == null) return;

        for (int tier = 0; tier < _data.Tiers.Length; tier++)
        {
            int count = MonsterOutgameManager.Instance.GetTierCount(tier);
            for (int i = 0; i < count; i++)
            {
                Vector2? pos = FindValidSpawnPosition();
                if (pos != null)
                {
                    CreateMonster(tier, pos.Value);
                }
            }
        }
    }
    #endregion

    #region Monster Management
    private bool CreateMonster(int tier, Vector2 position)
    {
        GameObject obj = Instantiate(_data.MonsterPrefab, position, Quaternion.identity, transform);
        if (obj.TryGetComponent(out Monster monster))
        {
            if (!_monsters.Contains(monster))
            {
                _monsters.Add(monster);
            }

            monster.Initialize(this, tier, _data.Tiers[tier]);
            OnMonsterChanged?.Invoke();
            return true;
        }
        Destroy(obj);
        return false;
    }
    #endregion

    #region Position Validation
    private Vector2? FindValidSpawnPosition()
    {
        Vector2 min = _resourceSpawner.AreaCenter - _resourceSpawner.AreaSize / 2f;
        Vector2 max = _resourceSpawner.AreaCenter + _resourceSpawner.AreaSize / 2f;

        for (int i = 0; i < 50; i++)
        {
            Vector2 candidate = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            if (IsValidPosition(candidate)) return candidate;
        }
        return null;
    }

    private bool IsValidPosition(Vector2 pos)
    {
        float spacing = _data.MinSpacing;
        if (_monsters.Any(m => m != null && Vector2.Distance(pos, m.transform.position) < spacing)) return false;
        if (_resourceSpawner.ActiveResources.Any(r => r != null && Vector2.Distance(pos, r.transform.position) < spacing)) return false;
        return true;
    }
    #endregion

    public Resource FindRandomResource(int maxLevel)
    {
        var resources = FindObjectsOfType<Resource>().Where(r => r.RequiredToolLevel <= maxLevel).ToList();
        if (resources.Count == 0) return null;
        var targeted = new HashSet<Resource>(_monsters.Where(m => m != null && m.TargetResource != null).Select(m => m.TargetResource));
        var untargeted = resources.Where(r => !targeted.Contains(r)).ToList();
        return (untargeted.Count > 0 ? untargeted : resources)[Random.Range(0, (untargeted.Count > 0 ? untargeted : resources).Count)];
    }
}
