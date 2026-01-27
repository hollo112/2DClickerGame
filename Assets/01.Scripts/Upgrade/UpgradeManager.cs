using System;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private UpgradeData _data;

    // 현재 레벨들
    private int _toolLevel = 0;
    private int _damageLevel = 0;
    private bool _isAutoClickUnlocked = false;
    private int _autoClickSpeedLevel = 0;

    // 프로퍼티
    public int ToolLevel => _toolLevel;
    public double BonusDamage => CalculateBonusDamage(_damageLevel);
    public bool IsAutoClickUnlocked => _isAutoClickUnlocked;

    private double CalculateBonusDamage(int level)
    {
        if (_data.DamagePerLevels == null || level <= 0) return 0;
        double total = 0;
        int maxIndex = Mathf.Min(level, _data.DamagePerLevels.Length);
        for (int i = 0; i < maxIndex; i++)
            total += _data.DamagePerLevels[i];
        return total;
    }

    public double GetNextBonusDamage()
    {
        if (_data.DamagePerLevels == null || _damageLevel >= _data.DamagePerLevels.Length)
            return 0;
        return _data.DamagePerLevels[_damageLevel];
    }
    public float AutoClickInterval => Mathf.Max(
        _data.BaseAutoClickInterval - (_autoClickSpeedLevel * _data.IntervalReductionPerLevel),
        _data.MinAutoClickInterval
    );

    // 업그레이드 이벤트
    public event Action<UpgradeType, int> OnUpgraded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 업그레이드 시도. 성공 시 true 반환
    public bool TryUpgrade(UpgradeType type)
    {
        double cost = GetUpgradeCost(type);
        if (cost < 0 || !CurrencyManager.Instance.CanAfford(cost))
        {
            Debug.Log($"[Upgrade] {type} 실패 - 비용 부족 또는 최대 레벨");
            return false;
        }

        CurrencyManager.Instance.SpendMoney(cost);
        ApplyUpgrade(type);

        // GameManager에 알려서 Clicker 갱신
        GameManager.Instance?.RefreshClickerStats();

        return true;
    }

    // 업그레이드 비용 조회. 최대 레벨이면 -1 반환
    public double GetUpgradeCost(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Tool:
                if (_toolLevel >= _data.MaxToolLevel) return -1;
                return _data.ToolUpgradeCosts[_toolLevel];

            case UpgradeType.Damage:
                if (_damageLevel >= _data.MaxDamageLevel) return -1;
                return _data.DamageUpgradeCosts[_damageLevel];

            case UpgradeType.AutoClickUnlock:
                if (_isAutoClickUnlocked) return -1;
                return _data.AutoClickUnlockCost;

            case UpgradeType.AutoClickSpeed:
                if (!_isAutoClickUnlocked) return -1;  // 해금 필요
                if (_autoClickSpeedLevel >= _data.MaxAutoClickLevel) return -1;
                return _data.AutoClickSpeedCosts[_autoClickSpeedLevel];

            default:
                return -1;
        }
    }

    // 현재 레벨 조회
    public int GetCurrentLevel(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Tool => _toolLevel,
            UpgradeType.Damage => _damageLevel,
            UpgradeType.AutoClickUnlock => _isAutoClickUnlocked ? 1 : 0,
            UpgradeType.AutoClickSpeed => _autoClickSpeedLevel,
            _ => 0
        };
    }

    // 최대 레벨 여부
    public bool IsMaxLevel(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Tool => _toolLevel >= _data.MaxToolLevel,
            UpgradeType.Damage => _damageLevel >= _data.MaxDamageLevel,
            UpgradeType.AutoClickUnlock => _isAutoClickUnlocked,
            UpgradeType.AutoClickSpeed => _autoClickSpeedLevel >= _data.MaxAutoClickLevel,
            _ => true
        };
    }

    private void ApplyUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Tool:
                _toolLevel++;
                Debug.Log($"[Upgrade] 도구 레벨 UP! Lv.{_toolLevel}");
                break;

            case UpgradeType.Damage:
                _damageLevel++;
                Debug.Log($"[Upgrade] 데미지 레벨 UP! Lv.{_damageLevel} (보너스: +{BonusDamage})");
                break;

            case UpgradeType.AutoClickUnlock:
                _isAutoClickUnlocked = true;
                Debug.Log("[Upgrade] 오토클릭 해금!");
                break;

            case UpgradeType.AutoClickSpeed:
                _autoClickSpeedLevel++;
                Debug.Log($"[Upgrade] 오토클릭 속도 UP! Lv.{_autoClickSpeedLevel} (간격: {AutoClickInterval:F2}s)");
                break;
        }

        OnUpgraded?.Invoke(type, GetCurrentLevel(type));
    }
}