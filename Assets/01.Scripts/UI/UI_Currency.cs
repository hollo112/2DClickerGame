using UnityEngine;
using TMPro;

public class UI_Currency : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _moneyText;

    private void Start()
    {
        CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        UpdateMoneyText(CurrencyManager.Instance.Get(ECurrencyType.Gold));
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
    }

    private void OnCurrencyChanged(ECurrencyType type, double amount)
    {
        if (type == ECurrencyType.Gold)
            UpdateMoneyText(amount);
    }

    private void UpdateMoneyText(double money)
    {
        _moneyText.text = $"{money.ToFormattedString()}";
    }
}