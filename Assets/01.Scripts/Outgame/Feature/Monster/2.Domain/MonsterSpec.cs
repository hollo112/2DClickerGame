using System;
using UnityEngine;

public class MonsterSpec
    {
        private readonly MonsterData _data;
        public MonsterData Data => _data;

        public int MaxTier => _data?.Tiers?.Length > 0 ? _data.Tiers.Length - 1 : 0;
        public int TierCount => _data?.Tiers?.Length ?? 0;
        public double SpawnCost => GetValidSpawnCost();
        public int MaxMonstersPerTier => GetValidMaxMonstersPerTier();
        public float MoveSpeed => GetValidMoveSpeed();
        public float MinSpacing => GetValidMinSpacing();

        public GameObject MonsterPrefab => _data?.MonsterPrefab;

        public MonsterSpec(MonsterData data)
        {
            _data = data;
            ValidateData();
        }

        private void ValidateData()
        {
            if (_data == null)
            {
                Debug.LogWarning("[MonsterSpec] MonsterData is null. Using default values.");
                return;
            }

            if (_data.Tiers == null || _data.Tiers.Length == 0)
            {
                Debug.LogWarning("[MonsterSpec] Tiers is null or empty. At least one tier is required.");
            }

            if (_data.SpawnCost < 0)
            {
                Debug.LogError($"[MonsterSpec] SpawnCost must be >= 0, but got {_data.SpawnCost}. Using 0.");
            }

            if (_data.MaxMonstersPerTier <= 0)
            {
                Debug.LogError($"[MonsterSpec] MaxMonstersPerTier must be > 0, but got {_data.MaxMonstersPerTier}. Using 10.");
            }

            if (_data.MoveSpeed <= 0)
            {
                Debug.LogError($"[MonsterSpec] MoveSpeed must be > 0, but got {_data.MoveSpeed}. Using 2f.");
            }

            if (_data.MinSpacing <= 0)
            {
                Debug.LogError($"[MonsterSpec] MinSpacing must be > 0, but got {_data.MinSpacing}. Using 1.5f.");
            }

            ValidateMergeCosts();
        }

        private void ValidateMergeCosts()
        {
            if (_data.MergeCosts == null || _data.MergeCosts.Length == 0)
            {
                Debug.LogWarning("[MonsterSpec] MergeCosts is null or empty.");
                return;
            }

            for (int i = 0; i < _data.MergeCosts.Length; i++)
            {
                if (_data.MergeCosts[i] < 0)
                {
                    Debug.LogError($"[MonsterSpec] MergeCosts[{i}] must be >= 0, but got {_data.MergeCosts[i]}. Using 0.");
                }
            }
        }

        private double GetValidSpawnCost()
        {
            if (_data == null) return double.MaxValue;
            return _data.SpawnCost >= 0 ? _data.SpawnCost : 0;
        }

        private int GetValidMaxMonstersPerTier()
        {
            if (_data == null) return 10;
            return _data.MaxMonstersPerTier > 0 ? _data.MaxMonstersPerTier : 10;
        }

        private float GetValidMoveSpeed()
        {
            if (_data == null) return 2f;
            return _data.MoveSpeed > 0 ? _data.MoveSpeed : 2f;
        }

        private float GetValidMinSpacing()
        {
            if (_data == null) return 1.5f;
            return _data.MinSpacing > 0 ? _data.MinSpacing : 1.5f;
        }

        public MonsterData.TierInfo GetTierInfo(int tier)
        {
            if (!IsValidTier(tier))
            {
                Debug.LogError($"[MonsterSpec] Invalid tier: {tier}. Valid range is 0~{MaxTier}.");
                return null;
            }
            return _data.Tiers[tier];
        }

        public double GetMergeCost(int tier)
        {
            if (!CanMerge(tier))
            {
                Debug.LogError($"[MonsterSpec] Cannot merge from tier {tier}. Valid range is 0~{MaxTier - 1}.");
                return double.MaxValue;
            }

            if (_data.MergeCosts == null || tier >= _data.MergeCosts.Length)
                return 0;

            return _data.MergeCosts[tier] >= 0 ? _data.MergeCosts[tier] : 0;
        }

        public bool IsValidTier(int tier)
        {
            return tier >= 0 && tier < TierCount;
        }

        public bool CanMerge(int tier)
        {
            return tier >= 0 && tier < MaxTier; // 마지막 티어는 머지 불가
        }

        public bool CanSpawn()
        {
            return _data != null
                && _data.Tiers != null
                && _data.Tiers.Length > 0
                && SpawnCost < double.MaxValue;
        }

        public int MergeCostsLength => _data?.MergeCosts?.Length ?? 0;
    }