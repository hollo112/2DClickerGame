using System;

public class MonsterCollection
{
    private int[] _tierCounts;
    private readonly int _totalTiers;
    private readonly int _maxPerTier;
    private readonly int _mergeRequirement;

    public int TotalTiers => _totalTiers;

    public MonsterCollection(int totalTiers, int maxPerTier, int mergeRequirement)
    {
        _totalTiers = totalTiers;
        _maxPerTier = maxPerTier;
        _mergeRequirement = mergeRequirement;
        _tierCounts = new int[totalTiers];
    }

    public int GetTierCount(int tier)
    {
        if (tier < 0 || tier >= _totalTiers) return 0;
        return _tierCounts[tier];
    }

    public bool CanAddMonster(int tier)
    {
        if (tier < 0 || tier >= _totalTiers) return false;
        return _tierCounts[tier] < _maxPerTier;
    }

    public void AddMonster(int tier)
    {
        if (tier < 0 || tier >= _totalTiers) return;
        _tierCounts[tier]++;
    }

    public void RemoveMonster(int tier)
    {
        if (tier < 0 || tier >= _totalTiers) return;
        if (_tierCounts[tier] > 0) _tierCounts[tier]--;
    }

    public int FindMergeableTier()
    {
        for (int i = 0; i < _totalTiers - 1; i++)
        {
            if (_tierCounts[i] >= _mergeRequirement && _tierCounts[i + 1] < _maxPerTier)
                return i;
        }
        return -1;
    }

    public bool CanMerge() => FindMergeableTier() >= 0;

    public MonsterSaveData ToSaveData()
    {
        var data = new MonsterSaveData
        {
            TierCounts = new int[_totalTiers]
        };
        Array.Copy(_tierCounts, data.TierCounts, _totalTiers);
        return data;
    }

    public void LoadFrom(MonsterSaveData data)
    {
        if (data?.TierCounts == null || data.TierCounts.Length == 0) return;

        int count = Math.Min(data.TierCounts.Length, _totalTiers);
        for (int i = 0; i < count; i++)
        {
            _tierCounts[i] = data.TierCounts[i];
        }
    }
}
