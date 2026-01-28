using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private double _startingMoney = 0;

    private double[] _currency = new double[(int)ECurrencyType.Count];

    public event Action<ECurrencyType, double> OnCurrencyChanged;

    private ICurrencyRepository _repository;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _currency[(int)ECurrencyType.Gold] = _startingMoney;
        _repository = new LocalCurrencyRepository();
    }

    private void Start()
    {
        Load();
    }

    public double Get(ECurrencyType type)
    {
        return _currency[(int)type];
    }

    public void Add(ECurrencyType type, double amount)
    {
        if (amount <= 0) return;

        _currency[(int)type] += amount;

        Save();

        OnCurrencyChanged?.Invoke(type, _currency[(int)type]);

        Debug.Log($"[Currency] {type} +{amount} | 현재: {_currency[(int)type]}");
    }

    public bool Spend(ECurrencyType type, double amount)
    {
        if (amount <= 0 || _currency[(int)type] < amount)
            return false;

        _currency[(int)type] -= amount;

        Save();

        OnCurrencyChanged?.Invoke(type, _currency[(int)type]);

        Debug.Log($"[Currency] {type} -{amount} | 현재: {_currency[(int)type]}");
        return true;
    }

    public bool CanAfford(ECurrencyType type, double amount)
    {
        return _currency[(int)type] >= amount;
    }

    private void Save()
    {
        var saveData = new CurrencySaveData { Currencies = _currency };
        _repository.Save(saveData);
    }

    private void Load()
    {
        var saveData = _repository.Load();
        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            if (saveData.Currencies[i] > 0)
            {
                _currency[i] = saveData.Currencies[i];
                OnCurrencyChanged?.Invoke((ECurrencyType)i, _currency[i]);
            }
        }
    }
}