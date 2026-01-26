using UnityEngine;

public class Resource : MonoBehaviour, IClickable
{
    [Header("Resource Settings")]
    [SerializeField] private string _name;
    [SerializeField] private int _requiredToolLevel = 0;
    [SerializeField] private int _baseReward = 10;

    [Header("HP Settings")]
    [SerializeField] private int _maxHp = 5;

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
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"[{_name}] 파괴됨!");

        // 스포너에 알림
        _spawner?.OnResourceDestroyed(gameObject, _spawnedPosition, _spawnedLevel);

        // TODO: 파괴 이펙트/사운드

        Destroy(gameObject);
    }
}