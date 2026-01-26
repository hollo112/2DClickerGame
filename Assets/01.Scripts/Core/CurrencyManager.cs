using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int _startingMoney = 0;

    private int _currentMoney;

    public int CurrentMoney => _currentMoney;

    // 재화 변경 시 이벤트
    public event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _currentMoney = _startingMoney;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        _currentMoney += amount;
        OnMoneyChanged?.Invoke(_currentMoney);

        Debug.Log($"[Currency] +{amount} | 현재: {_currentMoney}");
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0 || _currentMoney < amount)
            return false;

        _currentMoney -= amount;
        OnMoneyChanged?.Invoke(_currentMoney);

        Debug.Log($"[Currency] -{amount} | 현재: {_currentMoney}");
        return true;
    }

    public bool CanAfford(int amount) => _currentMoney >= amount;
}