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

    private void OnCurrencyChanged(ECurrencyType type, Currency amount)
    {
        if (type == ECurrencyType.Gold)
            UpdateMoneyText(amount);
    }

    private void UpdateMoneyText(Currency money)
    {
        _moneyText.text = money.ToString();
    }
}