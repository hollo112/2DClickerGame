using System.Collections.Generic;
using UnityEngine;

public class BuildingSpawner : MonoBehaviour
{
    public static BuildingSpawner Instance { get; private set; }

    [Header("Building Prefab")]
    [SerializeField] private GameObject _buildingPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private ResourceSpawner _resourceSpawner;  // 스폰 영역 참조
    [SerializeField] private float _minSpacing = 1.5f;  // 최소 간격

    [Header("Debug")]
    [SerializeField] private bool _showGizmos = true;

    private List<Building> _activeBuildings = new List<Building>();
    private List<Vector2> _occupiedPositions = new List<Vector2>();

    public bool HasBuilding => _activeBuildings.Count > 0;
    public IReadOnlyList<Vector2> OccupiedPositions => _occupiedPositions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpgradeManager.Instance.OnUpgraded += OnUpgraded;
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgraded -= OnUpgraded;
    }

    private void OnUpgraded(UpgradeType type, int level)
    {
        if (type == UpgradeType.BuildingUnlock)
        {
            SpawnBuilding();
        }
    }

    public bool SpawnBuilding()
    {
        if (_resourceSpawner == null)
        {
            Debug.LogWarning("[BuildingSpawner] ResourceSpawner가 설정되지 않았습니다.");
            return false;
        }

        Vector2? spawnPos = FindValidSpawnPosition();
        if (spawnPos == null)
        {
            Debug.LogWarning("[BuildingSpawner] 유효한 스폰 위치를 찾지 못했습니다.");
            return false;
        }

        GameObject buildingObj = Instantiate(_buildingPrefab, spawnPos.Value, Quaternion.identity, transform);

        if (buildingObj.TryGetComponent(out Building building))
        {
            building.Initialize(this);
            _activeBuildings.Add(building);
            _occupiedPositions.Add(spawnPos.Value);

            Debug.Log($"[BuildingSpawner] 건축물 생성! 위치: {spawnPos.Value}");
            return true;
        }

        return false;
    }

    private Vector2? FindValidSpawnPosition(int maxAttempts = 30)
    {
        Vector2 areaCenter = _resourceSpawner.AreaCenter;
        Vector2 areaSize = _resourceSpawner.AreaSize;

        Vector2 min = areaCenter - areaSize / 2f;
        Vector2 max = areaCenter + areaSize / 2f;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y)
            );

            if (IsValidPosition(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private bool IsValidPosition(Vector2 position)
    {
        // 다른 Building과의 거리 검사
        foreach (Vector2 occupied in _occupiedPositions)
        {
            if (Vector2.Distance(position, occupied) < _minSpacing)
            {
                return false;
            }
        }

        // Resource와의 거리 검사
        if (_resourceSpawner != null)
        {
            float spacing = Mathf.Max(_minSpacing, _resourceSpawner.MinSpacing);
            foreach (Vector2 resourcePos in _resourceSpawner.OccupiedPositions)
            {
                if (Vector2.Distance(position, resourcePos) < spacing)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void OnBuildingDestroyed(Building building, Vector2 position)
    {
        _activeBuildings.Remove(building);
        _occupiedPositions.Remove(position);
    }

    private void OnDrawGizmos()
    {
        if (!_showGizmos) return;

        // 스폰 영역 표시 (ResourceSpawner와 동일)
        if (_resourceSpawner != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            Gizmos.DrawCube(_resourceSpawner.AreaCenter, _resourceSpawner.AreaSize);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireCube(_resourceSpawner.AreaCenter, _resourceSpawner.AreaSize);
        }

        // 활성 건축물 위치 표시
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        foreach (Vector2 pos in _occupiedPositions)
        {
            Gizmos.DrawWireSphere(pos, _minSpacing / 2f);
        }
    }
}
