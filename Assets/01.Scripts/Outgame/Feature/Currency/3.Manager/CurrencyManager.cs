using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private double _startingMoney = 0;

    private Currency[] _currency = new Currency[(int)ECurrencyType.Count];

    public event Action<ECurrencyType, Currency> OnCurrencyChanged;

    private ICurrencyRepository _repository;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _currency[(int)ECurrencyType.Gold] = new Currency(_startingMoney);

        // HybridRepository 사용: 로컬 + Firebase 동기화
        _repository = new HybridCurrencyRepository(AccountManager.Instance.Email);    
    }

    private void Start()
    {
        Load();
    }

    public Currency Get(ECurrencyType type)
    {
        return _currency[(int)type];
    }

    public void Add(ECurrencyType type, Currency amount)
    {
        if (amount <= 0) return;

        _currency[(int)type] += amount;

        Save();

        OnCurrencyChanged?.Invoke(type, _currency[(int)type]);

    }

    public bool Spend(ECurrencyType type, Currency amount)
    {
        if (amount <= 0 || _currency[(int)type] < amount)
            return false;

        _currency[(int)type] -= amount;

        Save();

        OnCurrencyChanged?.Invoke(type, _currency[(int)type]);

        return true;
    }

    public bool CanAfford(ECurrencyType type, Currency amount)
    {
        return _currency[(int)type] >= amount;
    }

    private async void Save()
    {
        var saveData = new CurrencySaveData
        {
            Currencies = new double[(int)ECurrencyType.Count]
        };
        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            saveData.Currencies[i] = _currency[i].Value;
        }
        await _repository.Save(saveData);
    }

    private async void Load()
    {
        var saveData = await _repository.Load();
        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            if (saveData.Currencies[i] > 0)
            {
                _currency[i] = new Currency(saveData.Currencies[i]);
                OnCurrencyChanged?.Invoke((ECurrencyType)i, _currency[i]);
            }
        }
    }
}