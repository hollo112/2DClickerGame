public interface IClickable
{
    bool OnClick(ClickInfo clickInfo);
    int RequiredToolLevel { get; }  // 이 자원을 캐기 위한 최소 도구 레벨
}
