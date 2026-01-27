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
        // 매니저들 초기화 후 Clicker에 초기값 반영
        RefreshClickerStats();
    }

    /// 업그레이드 변경 시 Clicker에 반영
    public void RefreshClickerStats()
    {
        if (_clicker == null) return;

        var upgrade = UpgradeManager.Instance;

        _clicker.SetToolLevel(upgrade.ToolLevel);
        _clicker.SetDamage(_baseClickDamage + upgrade.BonusDamage);
        _clicker.SetAutoClickEnabled(upgrade.IsAutoClickUnlocked);
        _clicker.SetAutoClickInterval(upgrade.AutoClickInterval);
    }
}