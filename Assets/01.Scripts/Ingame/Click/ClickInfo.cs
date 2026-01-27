using UnityEngine;

public enum ClickerType
{
    Player,
    Monster,
}

public struct ClickInfo
{
    public Vector2 WorldPosition;
    public double Damage;
    public int ToolLevel;
    public bool IsAutoClick;
    public double Reward;  // 획득 재화
    public ClickerType Type;
    
    public ClickInfo(Vector2 worldPosition, double damage, int toolLevel, bool isAutoClick, ClickerType type = ClickerType.Player)
    {
        WorldPosition = worldPosition;
        Damage = damage;
        ToolLevel = toolLevel;
        IsAutoClick = isAutoClick;
        Reward = 0;
        Type = type;
    }
}