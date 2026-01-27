using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Game/MonsterData")]
public class MonsterData : ScriptableObject
{
    [System.Serializable]
    public class TierInfo
    {
        public string Name;
        public Sprite Sprite;
        public double AttackDamage;      // 공격력
        public float AttackInterval;     // 공격 간격 (초)
        public float AttackRange = 1f;   // 공격 범위
    }

    [Header("Tier Settings")]
    public TierInfo[] Tiers = new TierInfo[5];  // 5단계

    [Header("Cost Settings")]
    public double SpawnCost = 50;               // 소환 비용
    public double MergeCost = 100;              // 머지 비용

    [Header("Movement Settings")]
    public float MoveSpeed = 2f;                // 이동 속도
    public float MinSpacing = 1.5f;             // 최소 간격

    [Header("Prefab")]
    public GameObject MonsterPrefab;            // 몬스터 프리팹

    public int MaxTier => Tiers.Length - 1;     // 최대 티어 (0-indexed)
}
