using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Game/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    [Header("Tool Upgrade (도구)")]
    public int MaxToolLevel = 10;
    public double[] ToolUpgradeCosts;  // 레벨별 비용

    [Header("Damage Upgrade (돈 업그레이드)")]
    public int MaxDamageLevel = 20;
    public double[] DamageUpgradeCosts;
    public double[] DamagePerLevels;  // 레벨별 추가 데미지 (누적)

    [Header("Auto Click Unlock (오토클릭 해금)")]
    public double AutoClickUnlockCost = 100;

    [Header("Auto Click Speed (오토클릭 속도)")]
    public int MaxAutoClickLevel = 10;
    public double[] AutoClickSpeedCosts;
    public float BaseAutoClickInterval = 1f;
    public float IntervalReductionPerLevel = 0.08f;  // 레벨당 감소량
    public float MinAutoClickInterval = 0.2f;        // 최소 간격
}