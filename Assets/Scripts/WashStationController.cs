using System;
using System.Collections.Generic;
using UnityEngine;

public class WashStationController : MonoBehaviour
{
    private static readonly List<WashStationController> allStations =
        new List<WashStationController>();

    [Header("Identity")]
    [SerializeField] private string stationId = "station_1";
    [SerializeField] private string displayName = "BASIC WASH";

    [Header("Unlock")]
    [SerializeField] private bool startsUnlocked = true;
    [SerializeField] private WashStationController prerequisiteStation;
    [SerializeField] private int requiredPrerequisiteLevel = 10;
    [SerializeField] private int unlockCost = 50000;

    [Header("Level")]
    [SerializeField] private int maxLevel = 10;

    [Header("Upgrade Cost")]
    [SerializeField] private int baseUpgradeCost = 25;
    [SerializeField] private float upgradeCostMultiplier = 1.32f;

    [Header("Income")]
    [SerializeField] private int baseReward = 10;
    [SerializeField] private int rewardIncreasePerLevel = 5;

    [Header("Milestones")]
    [SerializeField] private int milestoneEveryLevels = 5;
    [SerializeField] private float milestoneIncomeMultiplier = 2f;

    [Header("Wash Speed")]
    [SerializeField] private float baseWashDuration = 2f;
    [SerializeField] private float washDurationDecreasePerLevel = 0.12f;
    [SerializeField] private float minimumWashDuration = 0.75f;

    [Header("Income / Second Estimate")]
    [SerializeField] private float estimatedTravelAndSpawnSeconds = 4f;

    private int level = 1;
    private bool unlocked;

    public string StationId => stationId;
    public string DisplayName => displayName;

    public int Level => level;
    public int MaxLevel => maxLevel;

    public bool IsUnlocked => unlocked;
    public bool IsMaxLevel => level >= maxLevel;

    public int UnlockCost => unlockCost;

    public WashStationController PrerequisiteStation =>
        prerequisiteStation;

    public int RequiredPrerequisiteLevel =>
        requiredPrerequisiteLevel;

    public bool PrerequisiteMet
    {
        get
        {
            if (prerequisiteStation == null)
                return true;

            return prerequisiteStation.Level >=
                   requiredPrerequisiteLevel;
        }
    }

    public int CurrentReward =>
        GetRewardForLevel(level);

    public int CurrentUpgradeCost
    {
        get
        {
            if (IsMaxLevel)
                return 0;

            return GetUpgradeCostForLevel(level);
        }
    }

    public float CurrentWashDuration =>
        GetWashDurationForLevel(level);

    public float CurrentIncomePerSecond =>
        GetIncomePerSecondForLevel(level);

    public event Action OnStationChanged;
    public event Action OnUnlocked;

    private string LevelKey =>
        $"wash_station_{stationId}_level";

    private string UnlockKey =>
        $"wash_station_{stationId}_unlocked";

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(stationId))
        {
            stationId = gameObject.name;
        }

        LoadState();
    }

    private void OnEnable()
    {
        if (!allStations.Contains(this))
        {
            allStations.Add(this);
        }
    }

    private void Start()
    {
        OnStationChanged?.Invoke();
    }

    private void OnDisable()
    {
        allStations.Remove(this);
    }

    private void LoadState()
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

        if (startsUnlocked)
        {
            unlocked = true;
        }
        else
        {
            unlocked =
                PlayerPrefs.GetInt(
                    UnlockKey,
                    0
                ) == 1;
        }
    }

    private void SaveLevel()
    {
        PlayerPrefs.SetInt(
            LevelKey,
            level
        );

        PlayerPrefs.Save();
    }

    private void SaveUnlocked()
    {
        PlayerPrefs.SetInt(
            UnlockKey,
            unlocked ? 1 : 0
        );

        PlayerPrefs.Save();
    }

    public bool TryUpgrade()
    {
        if (!unlocked)
            return false;

        if (IsMaxLevel)
            return false;

        if (GameEconomy.Instance == null)
            return false;

        int cost =
            CurrentUpgradeCost;

        if (!GameEconomy.Instance.SpendCoins(cost))
            return false;

        level++;

        SaveLevel();

        OnStationChanged?.Invoke();

        Debug.Log(
            $"{displayName} upgraded to Level {level}. " +
            $"Reward: {CurrentReward}, " +
            $"Income/sec: {CurrentIncomePerSecond:0.0}"
        );

        return true;
    }

    public bool TryUnlock()
    {
        if (unlocked)
            return false;

        if (!PrerequisiteMet)
            return false;

        if (GameEconomy.Instance == null)
            return false;

        if (
            !GameEconomy.Instance.SpendCoins(
                unlockCost
            )
        )
        {
            return false;
        }

        unlocked = true;

        SaveUnlocked();

        OnStationChanged?.Invoke();
        OnUnlocked?.Invoke();

        Debug.Log(
            $"{displayName} UNLOCKED!"
        );

        return true;
    }

    public int GetRewardForLevel(int targetLevel)
    {
        targetLevel = Mathf.Clamp(
            targetLevel,
            1,
            maxLevel
        );

        int normalReward =
            baseReward +
            ((targetLevel - 1) *
             rewardIncreasePerLevel);

        int milestoneCount = 0;

        if (milestoneEveryLevels > 0)
        {
            milestoneCount =
                targetLevel /
                milestoneEveryLevels;
        }

        float milestoneMultiplier =
            Mathf.Pow(
                milestoneIncomeMultiplier,
                milestoneCount
            );

        return Mathf.RoundToInt(
            normalReward *
            milestoneMultiplier
        );
    }

    public int GetUpgradeCostForLevel(
        int currentLevel
    )
    {
        return Mathf.RoundToInt(
            baseUpgradeCost *
            Mathf.Pow(
                upgradeCostMultiplier,
                currentLevel - 1
            )
        );
    }

    public float GetWashDurationForLevel(
        int targetLevel
    )
    {
        targetLevel = Mathf.Clamp(
            targetLevel,
            1,
            maxLevel
        );

        float duration =
            baseWashDuration -
            ((targetLevel - 1) *
             washDurationDecreasePerLevel);

        return Mathf.Max(
            minimumWashDuration,
            duration
        );
    }

    public float GetIncomePerSecondForLevel(
        int targetLevel
    )
    {
        float cycleTime =
            estimatedTravelAndSpawnSeconds +
            GetWashDurationForLevel(
                targetLevel
            );

        if (cycleTime <= 0f)
            return 0f;

        return
            GetRewardForLevel(targetLevel) /
            cycleTime;
    }

    public static float GetTotalUnlockedIncomePerSecond()
    {
        float total = 0f;

        foreach (
            WashStationController station
            in allStations
        )
        {
            if (
                station == null ||
                !station.IsUnlocked
            )
            {
                continue;
            }

            total +=
                station.CurrentIncomePerSecond;
        }

        return total;
    }
}