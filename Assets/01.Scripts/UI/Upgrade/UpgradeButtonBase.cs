using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class UpgradeButtonBase : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] protected Button _button;
    [SerializeField] protected TextMeshProUGUI _nameText;
    [SerializeField] protected TextMeshProUGUI _descText;
    [SerializeField] protected TextMeshProUGUI _levelText;
    [SerializeField] protected TextMeshProUGUI _costText;

    protected virtual void Start()
    {
        _button.onClick.AddListener(OnClick);
        CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        UpgradeManager.OnDataChanged += OnDataChanged;
        Refresh();
    }

    protected virtual void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
        UpgradeManager.OnDataChanged -= OnDataChanged;
    }

    private void OnCurrencyChanged(ECurrencyType type, Currency amount) => Refresh();
    private void OnDataChanged() => Refresh();

    private void OnClick()
    {
        if (TryUpgrade())
            Refresh();
    }

    public void Refresh()
    {
        UpdateDisplay();
        UpdateInteractable();
    }

    protected abstract bool TryUpgrade();
    protected abstract void UpdateDisplay();
    protected abstract double GetCurrentCost();

    protected virtual void UpdateInteractable()
    {
        double cost = GetCurrentCost();
        _button.interactable = cost >= 0 && CurrencyManager.Instance.CanAfford(ECurrencyType.Gold, cost);
    }

    protected void SetDisplay(string name, string desc, string level, string cost)
    {
        _nameText.text = name;
        _descText.text = desc;
        _levelText.text = level;
        _costText.text = cost;
    }
}
