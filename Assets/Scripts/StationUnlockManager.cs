using UnityEngine;

public class StationUnlockManager : MonoBehaviour
{
    [Header("Station 2")]
    [SerializeField] private GameObject station2;

    [Header("Unlock")]
    [SerializeField] private int station2UnlockLevel = 10;

    private bool station2Unlocked;

    private void Start()
    {
        ApplyCurrentState();

        if (WashUpgradeManager.Instance != null)
        {
            WashUpgradeManager.Instance.OnUpgradeChanged +=
                OnUpgradeChanged;
        }
        else
        {
            Debug.LogError(
                "StationUnlockManager: WashUpgradeManager missing."
            );
        }
    }

    private void ApplyCurrentState()
    {
        if (station2 == null)
        {
            Debug.LogError(
                "StationUnlockManager: Station2 reference missing."
            );

            return;
        }

        if (WashUpgradeManager.Instance == null)
        {
            station2.SetActive(false);
            return;
        }

        int currentLevel =
            WashUpgradeManager.Instance.Level;

        bool shouldBeUnlocked =
            currentLevel >= station2UnlockLevel;

        station2Unlocked =
            shouldBeUnlocked;

        station2.SetActive(
            shouldBeUnlocked
        );

        Debug.Log(
            shouldBeUnlocked
                ? "Station 2 unlocked."
                : $"Station 2 locked until level {station2UnlockLevel}."
        );
    }

    private void OnUpgradeChanged()
    {
        if (station2Unlocked)
            return;

        if (WashUpgradeManager.Instance == null)
            return;

        if (
            WashUpgradeManager.Instance.Level <
            station2UnlockLevel
        )
        {
            return;
        }

        UnlockStation2();
    }

    private void UnlockStation2()
    {
        if (station2 == null)
            return;

        station2Unlocked = true;

        station2.SetActive(true);

        Debug.Log(
            "NEW STATION UNLOCKED!"
        );
    }

    private void OnDestroy()
    {
        if (WashUpgradeManager.Instance != null)
        {
            WashUpgradeManager.Instance.OnUpgradeChanged -=
                OnUpgradeChanged;
        }
    }
}