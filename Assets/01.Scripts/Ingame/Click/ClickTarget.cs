using UnityEngine;

public class ClickTarget : MonoBehaviour, IClickable
{
    [SerializeField] private string _name;
    [SerializeField] private int _requiredToolLevel = 0;
    [SerializeField] private int _baseReward = 10;

    private ScaleTweeningFeedback _scaleFeedback;
    private ColorFlashFeedback _flashFeedback;

    public int RequiredToolLevel => _requiredToolLevel;

    private void Awake()
    {
        _scaleFeedback = GetComponent<ScaleTweeningFeedback>();
        _flashFeedback = GetComponent<ColorFlashFeedback>();
    }

    public bool OnClick(ClickInfo clickInfo)
    {
        if (clickInfo.ToolLevel < _requiredToolLevel)
        {
            Debug.Log($"도구 레벨 부족! 필요: {_requiredToolLevel}, 현재: {clickInfo.ToolLevel}");
            return false;
        }

        int reward = _baseReward + clickInfo.Damage;
        CurrencyManager.Instance.AddMoney(reward);

        var feedbacks = GetComponentsInChildren<IFeedback>();
        foreach (var feedback in feedbacks)
        {
            feedback.Play(clickInfo);
        }

        return true;
    }
}
