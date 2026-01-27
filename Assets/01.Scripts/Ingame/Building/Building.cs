using UnityEngine;
using DG.Tweening;

public class Building : MonoBehaviour
{
    [Header("Building Settings")]
    [SerializeField] private string _name = "Building";
    [SerializeField] private double _baseIncome = 5;

    [Header("Effects")]
    [SerializeField] private GameObject _incomeEffectPrefab;

    [Header("Spawn Animation")]
    [SerializeField] private float _spawnOffsetY = -0.5f;
    [SerializeField] private float _spawnDuration = 0.3f;

    private BuildingSpawner _spawner;
    private Vector2 _spawnedPosition;
    private float _incomeTimer = 0f;

    public Vector2 SpawnedPosition => _spawnedPosition;

    public void Initialize(BuildingSpawner spawner)
    {
        _spawner = spawner;
        _spawnedPosition = transform.position;

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

    private void Update()
    {
        if (!UpgradeManager.Instance.IsBuildingUnlocked) return;

        _incomeTimer += Time.deltaTime;

        float interval = UpgradeManager.Instance.BuildingInterval;
        if (_incomeTimer >= interval)
        {
            GenerateIncome();
            _incomeTimer = 0f;
        }
    }

    private void GenerateIncome()
    {
        double income = UpgradeManager.Instance.BuildingIncome;
        CurrencyManager.Instance.AddMoney(income);

        Debug.Log($"[{_name}] 수입 발생: +{income} G");

        // 이펙트 재생
        if (_incomeEffectPrefab != null)
        {
            Instantiate(_incomeEffectPrefab, transform.position, Quaternion.identity);
        }

        // 수입 피드백 (있다면)
        var feedbacks = GetComponentsInChildren<IFeedback>();
        foreach (var feedback in feedbacks)
        {
            ClickInfo info = new ClickInfo
            {
                Reward = income,
                WorldPosition = transform.position
            };
            feedback.Play(info);
        }
    }

    public void ForceDestroy()
    {
        transform.DOKill();
        _spawner?.OnBuildingDestroyed(this, _spawnedPosition);
        Destroy(gameObject);
    }
}
