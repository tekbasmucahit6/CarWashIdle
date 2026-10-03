using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfflineEarningsUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject offlinePanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text offlineCoinsText;

    [Header("Buttons")]
    [SerializeField] private Button claimButton;
    [SerializeField] private Button doubleClaimButton;

    private IEnumerator Start()
    {
        if (offlinePanel != null)
        {
            offlinePanel.SetActive(false);
        }

        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();

            claimButton.onClick.AddListener(
                ClaimOfflineEarnings
            );
        }

        if (doubleClaimButton != null)
        {
            doubleClaimButton.onClick.RemoveAllListeners();

            doubleClaimButton.onClick.AddListener(
                ClaimDoubleWithAd
            );
        }

        yield return null;

        Refresh();
    }

    private void Update()
    {
        if (
            offlinePanel == null ||
            !offlinePanel.activeSelf
        )
        {
            return;
        }

        RefreshDoubleButton();
    }

    private void Refresh()
    {
        if (
            OfflineEarningsManager.Instance == null ||
            offlinePanel == null
        )
        {
            return;
        }

        int coins =
            OfflineEarningsManager.Instance.PendingCoins;

        if (coins <= 0)
        {
            offlinePanel.SetActive(false);
            return;
        }

        offlinePanel.SetActive(true);

        if (offlineCoinsText != null)
        {
            offlineCoinsText.text =
                $"YOU EARNED\n{FormatNumber(coins)} COINS";
        }

        RefreshDoubleButton();
    }

    private void RefreshDoubleButton()
    {
        if (doubleClaimButton == null)
            return;

        if (AdMobRewardedManager.Instance == null)
        {
            doubleClaimButton.interactable = false;
            return;
        }

        doubleClaimButton.interactable =
            AdMobRewardedManager.Instance.IsRewardedReady;
    }

    private void ClaimOfflineEarnings()
    {
        if (OfflineEarningsManager.Instance == null)
            return;

        OfflineEarningsManager.Instance.Claim();

        ClosePanel();
    }

    private void ClaimDoubleWithAd()
    {
        if (
            OfflineEarningsManager.Instance == null ||
            AdMobRewardedManager.Instance == null
        )
        {
            return;
        }

        doubleClaimButton.interactable = false;

        AdMobRewardedManager.Instance.ShowRewarded(
            () =>
            {
                if (OfflineEarningsManager.Instance == null)
                    return;

                OfflineEarningsManager.Instance.ClaimDouble();

                ClosePanel();
            }
        );
    }

    private void ClosePanel()
    {
        if (offlinePanel != null)
        {
            offlinePanel.SetActive(false);
        }
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
}