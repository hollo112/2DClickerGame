using UnityEngine;

public struct ClickInfo
{
    public Vector2 WorldPosition;   // 클릭한 월드 좌표
    public int Damage;              // 클릭 데미지 (도구 레벨 + 돈 업그레이드 반영)
    public int ToolLevel;           // 현재 도구 레벨 (자원 티어 비교용)
    public bool IsAutoClick;        // 오토클릭 여부

    public ClickInfo(Vector2 worldPosition, int damage, int toolLevel, bool isAutoClick)
    {
        WorldPosition = worldPosition;
        Damage = damage;
        ToolLevel = toolLevel;
        IsAutoClick = isAutoClick;
    }
}