using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IncomeBoostManager : MonoBehaviour
{
    public static IncomeBoostManager Instance { get; private set; }

    private const string BoostEndKey = "income_boost_end_utc";

    [Header("Boost")]
    [SerializeField] private int boostDurationSeconds = 300;
    [SerializeField] private int boostMultiplier = 2;

    [Header("UI")]
    [SerializeField] private Button boostButton;
    [SerializeField] private TMP_Text boostButtonText;

    public int CurrentMultiplier
    {
        get
        {
            return IsBoostActive
                ? boostMultiplier
                : 1;
        }
    }

    public bool IsBoostActive
    {
        get
        {
            return GetRemainingSeconds() > 0;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (
            boostButtonText == null &&
            boostButton != null
        )
        {
            boostButtonText =
                boostButton.GetComponentInChildren<TMP_Text>();
        }

        if (boostButton != null)
        {
            boostButton.onClick.RemoveAllListeners();

            boostButton.onClick.AddListener(
                WatchAdForBoost
            );
        }

        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
    }

    private void WatchAdForBoost()
    {
        if (IsBoostActive)
            return;

        if (AdMobRewardedManager.Instance == null)
            return;

        AdMobRewardedManager.Instance.ShowRewarded(
            ActivateBoost
        );
    }

    private void ActivateBoost()
    {
        long currentTime =
            DateTimeOffset.UtcNow
                .ToUnixTimeSeconds();

        long boostEndTime =
            currentTime +
            boostDurationSeconds;

        PlayerPrefs.SetString(
            BoostEndKey,
            boostEndTime.ToString()
        );

        PlayerPrefs.Save();

        RefreshUI();

        Debug.Log(
            "2X income activated for 5 minutes."
        );
    }

    private int GetRemainingSeconds()
    {
        if (!PlayerPrefs.HasKey(BoostEndKey))
            return 0;

        string savedValue =
            PlayerPrefs.GetString(
                BoostEndKey,
                "0"
            );

        if (!long.TryParse(
                savedValue,
                out long boostEndTime
            ))
        {
            return 0;
        }

        long currentTime =
            DateTimeOffset.UtcNow
                .ToUnixTimeSeconds();

        long remaining =
            boostEndTime - currentTime;

        return Mathf.Max(
            0,
            (int)remaining
        );
    }

    private void RefreshUI()
    {
        int remainingSeconds =
            GetRemainingSeconds();

        bool active =
            remainingSeconds > 0;

        if (boostButtonText != null)
        {
            if (active)
            {
                int minutes =
                    remainingSeconds / 60;

                int seconds =
                    remainingSeconds % 60;

                boostButtonText.text =
                    $"2X ACTIVE\n" +
                    $"{minutes:00}:{seconds:00}";
            }
            else
            {
                boostButtonText.text =
                    "2X INCOME\nWATCH AD";
            }
        }

        if (boostButton != null)
        {
            if (active)
            {
                boostButton.interactable =
                    false;
            }
            else
            {
                boostButton.interactable =
                    AdMobRewardedManager.Instance != null &&
                    AdMobRewardedManager.Instance.IsRewardedReady;
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}