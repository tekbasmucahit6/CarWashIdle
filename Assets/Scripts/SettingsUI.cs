using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button privacyPolicyButton;
    [SerializeField] private Button privacyChoicesButton;
    [SerializeField] private Button reportAdButton;
    [SerializeField] private Button closeButton;

    private const string PrivacyPolicyUrl =
        "https://cooked-bellflower-069.notion.site/Car-Wash-Idle-Privacy-Policy-3ee5ffd8beb28090be9fecdb096f026d";

    private const string SupportUrl =
        "https://cooked-bellflower-069.notion.site/Car-Wash-Idle-Support-3ee5ffd8beb2805ca760e6a5e13884ad";

    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OpenSettings);
        }

        if (privacyPolicyButton != null)
        {
            privacyPolicyButton.onClick.RemoveAllListeners();
            privacyPolicyButton.onClick.AddListener(OpenPrivacyPolicy);
        }

        if (privacyChoicesButton != null)
        {
            privacyChoicesButton.onClick.RemoveAllListeners();
            privacyChoicesButton.onClick.AddListener(OpenPrivacyChoices);
        }

        if (reportAdButton != null)
        {
            reportAdButton.onClick.RemoveAllListeners();
            reportAdButton.onClick.AddListener(OpenSupport);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseSettings);
        }

        RefreshPrivacyChoicesButton();
    }

    private void Update()
    {
        RefreshPrivacyChoicesButton();
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        RefreshPrivacyChoicesButton();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL(PrivacyPolicyUrl);
    }

    public void OpenPrivacyChoices()
    {
        if (ConsentManager.Instance == null)
            return;

        ConsentManager.Instance.ShowPrivacyOptions();
    }

    public void OpenSupport()
    {
        Application.OpenURL(SupportUrl);
    }

    private void RefreshPrivacyChoicesButton()
    {
        if (privacyChoicesButton == null)
            return;

        if (ConsentManager.Instance == null)
        {
            privacyChoicesButton.interactable = false;
            return;
        }

        privacyChoicesButton.interactable =
            ConsentManager.Instance.IsPrivacyOptionsRequired;
    }
}