using System;
using UnityEngine;

public class WashUpgradeManager : MonoBehaviour
{
    public static WashUpgradeManager Instance { get; private set; }

    private const string LevelKey = "wash_level";

    [Header("Upgrade")]
    [SerializeField] private int level = 1;
    [SerializeField] private int maxLevel = 20;

    [SerializeField] private int baseUpgradeCost = 25;
    [SerializeField] private float upgradeCostMultiplier = 1.6f;

    [Header("Income")]
    [SerializeField] private int baseReward = 10;
    [SerializeField] private int rewardIncreasePerLevel = 5;

    [Header("Wash Speed")]
    [SerializeField] private float baseWashDuration = 2f;
    [SerializeField] private float washDurationDecreasePerLevel = 0.15f;
    [SerializeField] private float minimumWashDuration = 0.75f;

    public int Level => level;

    public int MaxLevel => maxLevel;

    public bool IsMaxLevel =>
        level >= maxLevel;

    public int CurrentReward =>
        baseReward +
        ((level - 1) * rewardIncreasePerLevel);

    public int CurrentUpgradeCost
    {
        get
        {
            if (IsMaxLevel)
                return 0;

            return Mathf.RoundToInt(
                baseUpgradeCost *
                Mathf.Pow(
                    upgradeCostMultiplier,
                    level - 1
                )
            );
        }
    }

    public float CurrentWashDuration
    {
        get
        {
            float duration =
                baseWashDuration -
                ((level - 1) *
                 washDurationDecreasePerLevel);

            return Mathf.Max(
                minimumWashDuration,
                duration
            );
        }
    }

    public event Action OnUpgradeChanged;

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadLevel();
    }

    public void TryUpgrade()
    {
        if (IsMaxLevel)
        {
            Debug.Log(
                "Wash station already at MAX level."
            );

            return;
        }

        if (GameEconomy.Instance == null)
            return;

        int cost =
            CurrentUpgradeCost;

        if (
            !GameEconomy.Instance
                .SpendCoins(cost)
        )
        {
            Debug.Log(
                "Not enough coins for upgrade."
            );

            return;
        }

        level++;

        SaveLevel();

        Debug.Log(
            $"Car Wash Level {level} | " +
            $"Reward: {CurrentReward} | " +
            $"Wash Time: {CurrentWashDuration:0.00}s"
        );

        OnUpgradeChanged?.Invoke();
    }

    private void SaveLevel()
    {
        PlayerPrefs.SetInt(
            LevelKey,
            level
        );

        PlayerPrefs.Save();
    }

    private void LoadLevel()
    {
        level = PlayerPrefs.GetInt(
            LevelKey,
            1
        );

        level = Mathf.Clamp(
            level,
            1,
            maxLevel
        );
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveLevel();
        }
    }

    private void OnApplicationQuit()
    {
        SaveLevel();
    }
}