using UnityEngine;
using DG.Tweening;

public class Resource : MonoBehaviour, IClickable
{
    [Header("Resource Settings")]
    [SerializeField] private string _name;
    [SerializeField] private int _requiredToolLevel = 0;
    [SerializeField] private double _baseReward = 10;

    [Header("HP Settings")]
    [SerializeField] private double _maxHp = 5;

    [Header("Effects")]
    [SerializeField] private GameObject _destroyEffectPrefab;

    private WarningFloaterFeedback _warningFeedback;

    [Header("Spawn Animation")]
    [SerializeField] private float _spawnOffsetY = -0.5f;
    [SerializeField] private float _spawnDuration = 0.3f;

    private double _currentHp;
    private ResourceSpawner _spawner;
    private Vector2 _spawnedPosition;
    private int _spawnedLevel;

    public int RequiredToolLevel => _requiredToolLevel;
    public double CurrentHp => _currentHp;
    public double MaxHp => _maxHp;

    public void Initialize(ResourceSpawner spawner, int spawnedLevel)
    {
        _spawner = spawner;
        _spawnedPosition = transform.position;
        _spawnedLevel = spawnedLevel;
        _currentHp = _maxHp;

        if (TryGetComponent(out SpriteRenderer sr))
        {
            sr.flipX = Random.value > 0.5f;
        }

        PlaySpawnAnimation();
    }

    private void PlaySpawnAnimation()
    {
        Vector3 targetPos = transform.position;
        Vector3 targetScale = transform.localScale;

        transform.position = targetPos + Vector3.up * _spawnOffsetY;
        transform.localScale = Vector3.zero;

        transform.DOMove(targetPos, _spawnDuration).SetEase(Ease.OutBack);
        transform.DOScale(targetScale, _spawnDuration).SetEase(Ease.OutBack)
            .OnComplete(() => StartIdleAnimation(targetScale));
    }

    private void StartIdleAnimation(Vector3 scale)
    {
        if (TryGetComponent(out IdleAnimation idle))
        {
            idle.Initialize(scale);
        }
    }

    private void Awake()
    {
        _currentHp = _maxHp;
        _spawnedPosition = transform.position;
        _warningFeedback = GetComponentInChildren<WarningFloaterFeedback>();
    }

    public bool OnClick(ClickInfo clickInfo)
    {
        if (clickInfo.ToolLevel < _requiredToolLevel)
        {
            _warningFeedback?.Play();
            return false;
        }

        // 데미지 적용
        TakeDamage(clickInfo.Damage);

        // 보상 지급
        double reward = _baseReward + clickInfo.Damage;
        CurrencyManager.Instance.Add(ECurrencyType.Gold, reward);

        // 피드백 재생 (Reward 설정)
        clickInfo.Reward = reward;
        var feedbacks = GetComponentsInChildren<IFeedback>();
        foreach (var feedback in feedbacks)
        {
            feedback.Play(clickInfo);
        }

        return true;
    }

    private void TakeDamage(double damage)
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
        transform.DOKill();
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