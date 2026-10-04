using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationWorldUI : MonoBehaviour
{
    [Header("Station")]
    [SerializeField] private WashStationController station;

    [Header("Texts")]
    [SerializeField] private TMP_Text stationNameText;
    [SerializeField] private TMP_Text levelText;

    [Header("Button")]
    [SerializeField] private Button openButton;
    [SerializeField] private TMP_Text openButtonText;

    [Header("Camera Facing")]
    [SerializeField] private bool faceCamera = true;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();

            openButton.onClick.AddListener(
                OpenStationPanel
            );
        }

        if (station != null)
        {
            station.OnStationChanged += Refresh;
            station.OnUnlocked += Refresh;
        }

        Refresh();
    }

    private void LateUpdate()
    {
        if (!faceCamera)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        transform.rotation =
            mainCamera.transform.rotation;
    }

    private void Refresh()
    {
        if (station == null)
            return;

        if (stationNameText != null)
        {
            stationNameText.text =
                station.DisplayName;
        }

        if (levelText != null)
        {
            if (station.IsUnlocked)
            {
                levelText.text =
                    $"LV. {station.Level}";
            }
            else
            {
                levelText.text =
                    "LOCKED";
            }
        }

        if (openButtonText != null)
        {
            openButtonText.text =
                station.IsUnlocked
                    ? "UPGRADE"
                    : "VIEW";
        }
    }

    private void OpenStationPanel()
    {
        if (station == null)
            return;

        if (StationDetailPanel.Instance == null)
        {
            Debug.LogError(
                "StationDetailPanel missing."
            );

            return;
        }

        StationDetailPanel.Instance.Open(
            station
        );
    }

    private void OnDestroy()
    {
        if (station != null)
        {
            station.OnStationChanged -= Refresh;
            station.OnUnlocked -= Refresh;
        }
    }
}