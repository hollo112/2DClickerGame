using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] _resourcePrefabs;
    [SerializeField] private int _maxResourceCount = 10;
    [SerializeField] private float _minSpacing = 1.5f;
    [SerializeField] private float _respawnDelay = 3f;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 _areaCenter;
    [SerializeField] private Vector2 _areaSize = new Vector2(10f, 10f);

    [Header("Debug")]
    [SerializeField] private bool _showGizmos = true;

    private List<GameObject> _activeResources = new List<GameObject>();
    private List<Vector2> _occupiedPositions = new List<Vector2>();

    private void Start()
    {
        SpawnInitialResources();
    }

    private void SpawnInitialResources()
    {
        for (int i = 0; i < _maxResourceCount; i++)
        {
            SpawnResource();
        }
    }

    public void SpawnResource()
    {
        if (_activeResources.Count >= _maxResourceCount) return;

        Vector2? spawnPos = FindValidSpawnPosition();
        if (spawnPos == null)
        {
            Debug.LogWarning("[ResourceSpawner] 유효한 스폰 위치를 찾지 못했습니다.");
            return;
        }

        GameObject prefab = _resourcePrefabs[Random.Range(0, _resourcePrefabs.Length)];
        GameObject resource = Instantiate(prefab, spawnPos.Value, Quaternion.identity, transform);

        _activeResources.Add(resource);
        _occupiedPositions.Add(spawnPos.Value);

        // Resource에 스포너 참조 전달
        if (resource.TryGetComponent(out Resource res))
        {
            res.Initialize(this);
        }
    }

    private Vector2? FindValidSpawnPosition(int maxAttempts = 30)
    {
        Vector2 min = _areaCenter - _areaSize / 2f;
        Vector2 max = _areaCenter + _areaSize / 2f;

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
        foreach (Vector2 occupied in _occupiedPositions)
        {
            if (Vector2.Distance(position, occupied) < _minSpacing)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 자원이 파괴될 때 호출
    /// </summary>
    public void OnResourceDestroyed(GameObject resource, Vector2 position)
    {
        _activeResources.Remove(resource);
        _occupiedPositions.Remove(position);

        // 일정 시간 후 리스폰
        StartCoroutine(RespawnAfterDelay());
    }

    private System.Collections.IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnResource();
    }

    private void OnDrawGizmos()
    {
        if (!_showGizmos) return;

        // 스폰 영역 표시
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(_areaCenter, _areaSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_areaCenter, _areaSize);
    }
}