using System;
using UnityEngine;

public class OfflineEarningsManager : MonoBehaviour
{
    public static OfflineEarningsManager Instance { get; private set; }

    private const string LastSeenKey = "last_seen_utc";
    private const string PendingCoinsKey = "pending_offline_coins";

    [Header("Offline Earnings")]
    [SerializeField] private float maxOfflineHours = 4f;

    [SerializeField] private float estimatedCycleSeconds = 10f;

    [Range(0f, 1f)]
    [SerializeField] private float offlineEfficiency = 0.6f;

    public int PendingCoins { get; private set; }

    public bool HasPendingCoins => PendingCoins > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        PendingCoins = PlayerPrefs.GetInt(
            PendingCoinsKey,
            0
        );
    }

    private void Start()
    {
        CalculateOfflineEarnings();

        SaveCurrentTime();
    }

    private void CalculateOfflineEarnings()
    {
        if (PendingCoins > 0)
            return;

        if (!PlayerPrefs.HasKey(LastSeenKey))
            return;

        string savedTime =
            PlayerPrefs.GetString(
                LastSeenKey,
                string.Empty
            );

        if (!long.TryParse(
                savedTime,
                out long lastSeenUnix
            ))
        {
            return;
        }

        long currentUnix =
            DateTimeOffset.UtcNow
                .ToUnixTimeSeconds();

        long secondsAway =
            currentUnix - lastSeenUnix;

        if (secondsAway <= 10)
            return;

        float maxSeconds =
            maxOfflineHours * 3600f;

        float countedSeconds =
            Mathf.Min(
                secondsAway,
                maxSeconds
            );

        if (WashUpgradeManager.Instance == null)
            return;

        int rewardPerWash =
            WashUpgradeManager.Instance
                .CurrentReward;

        float completedCycles =
            countedSeconds /
            estimatedCycleSeconds;

        float calculatedCoins =
            completedCycles *
            rewardPerWash *
            offlineEfficiency;

        PendingCoins =
            Mathf.Max(
                0,
                Mathf.FloorToInt(
                    calculatedCoins
                )
            );

        PlayerPrefs.SetInt(
            PendingCoinsKey,
            PendingCoins
        );

        PlayerPrefs.Save();
    }

    public void Claim()
    {
        if (PendingCoins <= 0)
            return;

        if (GameEconomy.Instance == null)
            return;

        GameEconomy.Instance.AddCoins(
            PendingCoins
        );

        ClearPendingCoins();
    }

    public void ClaimDouble()
    {
        if (PendingCoins <= 0)
            return;

        if (GameEconomy.Instance == null)
            return;

        GameEconomy.Instance.AddCoins(
            PendingCoins * 2
        );

        ClearPendingCoins();
    }

    private void ClearPendingCoins()
    {
        PendingCoins = 0;

        PlayerPrefs.SetInt(
            PendingCoinsKey,
            0
        );

        PlayerPrefs.Save();
    }

    private void SaveCurrentTime()
    {
        long currentUnix =
            DateTimeOffset.UtcNow
                .ToUnixTimeSeconds();

        PlayerPrefs.SetString(
            LastSeenKey,
            currentUnix.ToString()
        );

        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveCurrentTime();
        }
    }

    private void OnApplicationQuit()
    {
        SaveCurrentTime();
    }
}