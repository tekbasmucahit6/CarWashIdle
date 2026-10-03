using GoogleMobileAds.Ump.Api;
using UnityEngine;

public class ConsentManager : MonoBehaviour
{
    public static ConsentManager Instance { get; private set; }

    private bool adsStarted;

    public bool IsPrivacyOptionsRequired =>
        ConsentInformation.PrivacyOptionsRequirementStatus ==
        PrivacyOptionsRequirementStatus.Required;

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
#if UNITY_EDITOR
        Debug.Log(
            "UMP: Editor mode - consent simulation skipped."
        );

        StartAds();

        return;
#else
        RequestConsent();
#endif
    }

    private void RequestConsent()
    {
        ConsentRequestParameters request =
            new ConsentRequestParameters();

        ConsentInformation.Update(
            request,
            OnConsentInfoUpdated
        );
    }

    private void OnConsentInfoUpdated(
        FormError updateError
    )
    {
        if (updateError != null)
        {
            Debug.LogWarning(
                "UMP update error: " +
                updateError.Message
            );

            TryStartAds();

            return;
        }

        Debug.Log(
            "UMP consent information updated."
        );

        ConsentForm.LoadAndShowConsentFormIfRequired(
            OnConsentFormCompleted
        );
    }

    private void OnConsentFormCompleted(
        FormError formError
    )
    {
        if (formError != null)
        {
            Debug.LogWarning(
                "UMP form error: " +
                formError.Message
            );
        }
        else
        {
            Debug.Log(
                "UMP consent process completed."
            );
        }

        TryStartAds();
    }

    private void TryStartAds()
    {
        if (!ConsentInformation.CanRequestAds())
        {
            Debug.Log(
                "UMP: Ads cannot be requested yet."
            );

            return;
        }

        StartAds();
    }

    private void StartAds()
    {
        if (adsStarted)
            return;

        adsStarted = true;

        if (AdMobRewardedManager.Instance == null)
        {
            Debug.LogError(
                "AdMobRewardedManager is missing."
            );

            return;
        }

        AdMobRewardedManager.Instance
            .InitializeAds();
    }

    public void ShowPrivacyOptions()
    {
        if (!IsPrivacyOptionsRequired)
            return;

        ConsentForm.ShowPrivacyOptionsForm(
            (FormError error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning(
                        "Privacy options error: " +
                        error.Message
                    );

                    return;
                }

                Debug.Log(
                    "Privacy options closed."
                );
            }
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}