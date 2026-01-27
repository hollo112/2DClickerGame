using UnityEngine;
using Lean.Pool;

public class DamageFloaterSpawner : MonoBehaviour
{
    public static DamageFloaterSpawner Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private LeanGameObjectPool _pool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public GameObject Spawn(Vector3 position)
    {
        if (_pool == null)
        {
            Debug.LogWarning("[DamageFloaterSpawner] Pool이 설정되지 않았습니다.");
            return null;
        }

        return _pool.Spawn(position, Quaternion.identity);
    }

    public void Despawn(GameObject floater, float delay = 0f)
    {
        if (_pool == null || floater == null) return;

        _pool.Despawn(floater, delay);
    }
}