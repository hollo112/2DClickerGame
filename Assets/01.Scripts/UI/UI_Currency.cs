using UnityEngine;
using TMPro;

public class UI_Currency : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _moneyText;

    private void Start()
    {
        CurrencyManager.Instance.OnMoneyChanged += UpdateMoneyText;
        UpdateMoneyText(CurrencyManager.Instance.CurrentMoney);
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnMoneyChanged -= UpdateMoneyText;
    }

    private void UpdateMoneyText(double money)
    {
        _moneyText.text = $"{money.ToFormattedString()} G";
    }
}