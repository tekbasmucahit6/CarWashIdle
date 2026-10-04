using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationDetailPanel : MonoBehaviour
{
    public static StationDetailPanel Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text incomeText;
    [SerializeField] private TMP_Text perCarText;
    [SerializeField] private TMP_Text washTimeText;
    [SerializeField] private TMP_Text nextLevelText;
    [SerializeField] private TMP_Text requirementText;

    [Header("Buttons")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button closeButton;

    private WashStationController selectedStation;

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
        if (panel != null)
            panel.SetActive(false);

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionButtonPressed);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        if (GameEconomy.Instance != null)
        {
            GameEconomy.Instance.OnCoinsChanged +=
                OnCoinsChanged;
        }
    }

    public void Open(WashStationController station)
    {
        if (station == null)
            return;

        UnsubscribeFromSelectedStation();

        selectedStation = station;

        selectedStation.OnStationChanged += Refresh;
        selectedStation.OnUnlocked += Refresh;

        if (panel != null)
            panel.SetActive(true);

        Refresh();
    }

    public void Close()
    {
        UnsubscribeFromSelectedStation();

        selectedStation = null;

        if (panel != null)
            panel.SetActive(false);
    }

    private void Refresh()
    {
        if (selectedStation == null)
            return;

        if (titleText != null)
            titleText.text = selectedStation.DisplayName;

        if (selectedStation.IsUnlocked)
            ShowUnlockedStation();
        else
            ShowLockedStation();
    }

    private void ShowUnlockedStation()
    {
        if (selectedStation == null)
            return;

        if (levelText != null)
            levelText.text =
                $"LEVEL {selectedStation.Level}";

        if (incomeText != null)
        {
            incomeText.text =
                $"INCOME / SEC\n" +
                $"{FormatNumber(selectedStation.CurrentIncomePerSecond)}";
        }

        if (perCarText != null)
        {
            perCarText.text =
                $"PER CAR\n" +
                $"{FormatNumber(selectedStation.CurrentReward)}";
        }

        if (washTimeText != null)
        {
            washTimeText.text =
                $"WASH TIME\n" +
                $"{selectedStation.CurrentWashDuration:0.00}s";
        }

        if (requirementText != null)
            requirementText.gameObject.SetActive(false);

        if (selectedStation.IsMaxLevel)
        {
            if (nextLevelText != null)
                nextLevelText.text = "MAXIMUM LEVEL";

            if (actionButtonText != null)
                actionButtonText.text = "MAX LEVEL";

            if (actionButton != null)
                actionButton.interactable = false;

            return;
        }

        int nextLevel =
            selectedStation.Level + 1;

        float nextIncome =
            selectedStation.GetIncomePerSecondForLevel(
                nextLevel
            );

        float incomeIncrease =
            nextIncome -
            selectedStation.CurrentIncomePerSecond;

        if (nextLevelText != null)
        {
            nextLevelText.text =
                $"NEXT LEVEL\n" +
                $"+{FormatNumber(incomeIncrease)} / SEC";
        }

        int cost =
            selectedStation.CurrentUpgradeCost;

        if (actionButtonText != null)
        {
            actionButtonText.text =
                $"UPGRADE • {FormatNumber(cost)}";
        }

        if (
            actionButton != null &&
            GameEconomy.Instance != null
        )
        {
            actionButton.interactable =
                GameEconomy.Instance.CanAfford(cost);
        }
    }

    private void ShowLockedStation()
    {
        if (selectedStation == null)
            return;

        if (levelText != null)
            levelText.text = "LOCKED";

        if (incomeText != null)
        {
            incomeText.text =
                $"STARTING INCOME / SEC\n" +
                $"{FormatNumber(selectedStation.CurrentIncomePerSecond)}";
        }

        if (perCarText != null)
        {
            perCarText.text =
                $"STARTING PER CAR\n" +
                $"{FormatNumber(selectedStation.CurrentReward)}";
        }

        if (washTimeText != null)
        {
            washTimeText.text =
                $"WASH TIME\n" +
                $"{selectedStation.CurrentWashDuration:0.00}s";
        }

        if (nextLevelText != null)
            nextLevelText.text = "";

        bool prerequisiteMet =
            selectedStation.PrerequisiteMet;

        if (requirementText != null)
        {
            requirementText.gameObject.SetActive(true);

            if (
                selectedStation.PrerequisiteStation != null &&
                !prerequisiteMet
            )
            {
                requirementText.text =
                    $"REQUIRES\n" +
                    $"{selectedStation.PrerequisiteStation.DisplayName} " +
                    $"LV. {selectedStation.RequiredPrerequisiteLevel}";
            }
            else
            {
                requirementText.text =
                    "READY TO UNLOCK";
            }
        }

        int unlockCost =
            selectedStation.UnlockCost;

        if (actionButtonText != null)
        {
            actionButtonText.text =
                $"UNLOCK • {FormatNumber(unlockCost)}";
        }

        if (
            actionButton != null &&
            GameEconomy.Instance != null
        )
        {
            actionButton.interactable =
                prerequisiteMet &&
                GameEconomy.Instance.CanAfford(
                    unlockCost
                );
        }
    }

    private void OnActionButtonPressed()
    {
        if (selectedStation == null)
            return;

        if (selectedStation.IsUnlocked)
            selectedStation.TryUpgrade();
        else
            selectedStation.TryUnlock();

        Refresh();
    }

    private void OnCoinsChanged(int coins)
    {
        if (
            panel == null ||
            !panel.activeSelf
        )
        {
            return;
        }

        Refresh();
    }

    private void UnsubscribeFromSelectedStation()
    {
        if (selectedStation == null)
            return;

        selectedStation.OnStationChanged -= Refresh;
        selectedStation.OnUnlocked -= Refresh;
    }

    private string FormatNumber(int number)
    {
        if (number >= 1_000_000_000)
            return $"{number / 1_000_000_000f:0.#}B";

        if (number >= 1_000_000)
            return $"{number / 1_000_000f:0.#}M";

        if (number >= 1_000)
            return $"{number / 1_000f:0.#}K";

        return number.ToString();
    }

    private string FormatNumber(float number)
    {
        if (number >= 1_000_000_000f)
            return $"{number / 1_000_000_000f:0.#}B";

        if (number >= 1_000_000f)
            return $"{number / 1_000_000f:0.#}M";

        if (number >= 1_000f)
            return $"{number / 1_000f:0.#}K";

        return number.ToString("0.0");
    }

    private void OnDestroy()
    {
        UnsubscribeFromSelectedStation();

        if (GameEconomy.Instance != null)
        {
            GameEconomy.Instance.OnCoinsChanged -=
                OnCoinsChanged;
        }

        if (Instance == this)
            Instance = null;
    }
}