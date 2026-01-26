using UnityEngine;

public class ClickTarget : MonoBehaviour, IClickable
{
    [SerializeField] private string _name;
    [SerializeField] private int _requiredToolLevel = 0;  // 필요 도구 레벨
    [SerializeField] private int _baseReward = 10;        // 기본 보상

    public int RequiredToolLevel => _requiredToolLevel;

    public bool OnClick(ClickInfo clickInfo)
    {
        // 도구 레벨 부족 시 채집 불가
        if (clickInfo.ToolLevel < _requiredToolLevel)
        {
            Debug.Log($"도구 레벨 부족! 필요: {_requiredToolLevel}, 현재: {clickInfo.ToolLevel}");
            return false;
        }

        int reward = _baseReward + clickInfo.Damage;
        CurrencyManager.Instance.AddMoney(reward);

        // TODO: 이펙트, 사운드 추가

        return true;
    }
}
