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
        CurrencyManager.Instance.OnMoneyChanged += _ => Refresh();
        UpgradeManager.Instance.OnUpgraded += (_, _) => Refresh();
        Refresh();
    }

    protected virtual void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnMoneyChanged -= _ => Refresh();
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgraded -= (_, _) => Refresh();
    }

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
    protected abstract int GetCurrentCost();

    protected virtual void UpdateInteractable()
    {
        int cost = GetCurrentCost();
        _button.interactable = cost >= 0 && CurrencyManager.Instance.CanAfford(cost);
    }

    protected void SetDisplay(string name, string desc, string level, string cost)
    {
        _nameText.text = name;
        _descText.text = desc;
        _levelText.text = level;
        _costText.text = cost;
    }
}