using UnityEngine;

public class Resource : MonoBehaviour, IClickable
{
    [Header("Resource Settings")]
    [SerializeField] private string _name;
    [SerializeField] private int _requiredToolLevel = 0;
    [SerializeField] private int _baseReward = 10;

    [Header("HP Settings")]
    [SerializeField] private int _maxHp = 5;

    [Header("Effects")]
    [SerializeField] private GameObject _destroyEffectPrefab;

    private int _currentHp;
    private ResourceSpawner _spawner;
    private Vector2 _spawnedPosition;
    private int _spawnedLevel;

    public int RequiredToolLevel => _requiredToolLevel;
    public int CurrentHp => _currentHp;
    public int MaxHp => _maxHp;

    public void Initialize(ResourceSpawner spawner, int spawnedLevel)
    {
        _spawner = spawner;
        _spawnedPosition = transform.position;
        _spawnedLevel = spawnedLevel;
        _currentHp = _maxHp;
    }

    private void Awake()
    {
        _currentHp = _maxHp;
        _spawnedPosition = transform.position;
    }

    public bool OnClick(ClickInfo clickInfo)
    {
        if (clickInfo.ToolLevel < _requiredToolLevel)
        {
            Debug.Log($"도구 레벨 부족! 필요: {_requiredToolLevel}, 현재: {clickInfo.ToolLevel}");
            return false;
        }

        // 데미지 적용
        TakeDamage(clickInfo.Damage);

        // 보상 지급
        int reward = _baseReward + clickInfo.Damage;
        CurrencyManager.Instance.AddMoney(reward);

        // 피드백 재생
        var feedbacks = GetComponentsInChildren<IFeedback>();
        foreach (var feedback in feedbacks)
        {
            feedback.Play(clickInfo);
        }

        return true;
    }

    private void TakeDamage(int damage)
    {
        _currentHp -= damage;

        Debug.Log($"[{_name}] HP: {_currentHp}/{_maxHp}");

        if (_currentHp <= 0)
        {
            Despawn(notifySpawner: true);
        }
    }

    // 외부에서 강제 파괴 시 호출 (업그레이드로 인한 제거 등)
    public void ForceDestroy()
    {
        Despawn(notifySpawner: false);
    }

    private void Despawn(bool notifySpawner)
    {
        SpawnDestroyEffect();

        if (notifySpawner)
            _spawner?.OnResourceDestroyed(gameObject, _spawnedPosition, _spawnedLevel);

        Destroy(gameObject);
    }

    private void SpawnDestroyEffect()
    {
        if (_destroyEffectPrefab == null) return;

        Instantiate(_destroyEffectPrefab, transform.position, Quaternion.identity);
    }
}