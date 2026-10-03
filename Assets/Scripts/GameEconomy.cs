using System;
using UnityEngine;

public class GameEconomy : MonoBehaviour
{
    public static GameEconomy Instance { get; private set; }

    private const string CoinsKey = "player_coins";

    [Header("Economy")]
    [SerializeField] private int coins = 0;

    public int Coins => coins;

    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadCoins();
    }

    private void Start()
    {
        OnCoinsChanged?.Invoke(coins);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        coins += amount;

        SaveCoins();

        OnCoinsChanged?.Invoke(coins);
    }

    public bool CanAfford(int amount)
    {
        return coins >= amount;
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0)
            return false;

        if (!CanAfford(amount))
            return false;

        coins -= amount;

        SaveCoins();

        OnCoinsChanged?.Invoke(coins);

        return true;
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(
            CoinsKey,
            coins
        );

        PlayerPrefs.Save();
    }

    private void LoadCoins()
    {
        coins = PlayerPrefs.GetInt(
            CoinsKey,
            0
        );
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveCoins();
        }
    }

    private void OnApplicationQuit()
    {
        SaveCoins();
    }
}