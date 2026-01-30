using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Clicker _clicker;

    [Header("Settings")]
    [SerializeField] private double _baseClickDamage = 1;  // 기본 클릭 데미지

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
        UpgradeManager.OnDataChanged += RefreshClickerStats;
        RefreshClickerStats();
    }

    private void OnDestroy()
    {
        UpgradeManager.OnDataChanged -= RefreshClickerStats;
    }

    /// 업그레이드 변경 시 Clicker에 반영
    public void RefreshClickerStats()
    {
        if (_clicker == null) return;

        var upgrade = UpgradeManager.Instance;

        // 도구 레벨
        var toolUpgrade = upgrade.Get(EUpgradeType.ToolLevel);
        _clicker.SetToolLevel(toolUpgrade?.Level ?? 0);

        // 클릭 데미지
        double clickDamageValue = upgrade.Get(EUpgradeType.ClickDamage)?.Value ?? 0;
        double totalDamage = _baseClickDamage + clickDamageValue;
        _clicker.SetDamage(totalDamage);
        _clicker.SetAutoDamage(totalDamage);

        // 오토클릭
        var autoClickUpgrade = upgrade.Get(EUpgradeType.AutoClick);
        int autoClickLevel = autoClickUpgrade?.Level ?? 0;
        _clicker.SetAutoClickEnabled(autoClickLevel >= 1);
        if (autoClickLevel >= 1)
        {
            float interval = (float)(autoClickUpgrade.Value);
            _clicker.SetAutoClickInterval(Mathf.Max(interval, 0.1f));
        }
    }
}
