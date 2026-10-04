using System;
using System.Collections;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

public class AdMobRewardedManager : MonoBehaviour
{
    public static AdMobRewardedManager Instance { get; private set; }

    [Header("Testing")]
    [SerializeField] private bool simulateRewardInEditor = true;

    [Tooltip(
        "Keep this ON for TestFlight testing. " +
        "Turn it OFF before the final App Store release build."
    )]
    [SerializeField] private bool useTestAds = true;

    [Header("Retry Settings")]
    [SerializeField] private float rewardedRetryDelaySeconds = 10f;
    [SerializeField] private float interstitialRetryDelaySeconds = 15f;

    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;

    private Coroutine rewardedRetryRoutine;
    private Coroutine interstitialRetryRoutine;

    private bool rewardedLoading;
    private bool interstitialLoading;

    private bool fullScreenAdShowing;
    private bool adsInitialized;
    private bool initializationStarted;

    // ---------------------------------------------------------
    // GOOGLE OFFICIAL TEST AD UNIT IDS
    // ---------------------------------------------------------

    private const string AndroidTestRewardedId =
        "ca-app-pub-3940256099942544/5224354917";

    private const string IosTestRewardedId =
        "ca-app-pub-3940256099942544/1712485313";

    private const string AndroidTestInterstitialId =
        "ca-app-pub-3940256099942544/1033173712";

    private const string IosTestInterstitialId =
        "ca-app-pub-3940256099942544/4411468910";

    // ---------------------------------------------------------
    // WASHLINE IDLE PRODUCTION IOS IDS
    // ---------------------------------------------------------

    private const string IosProductionRewardedId =
        "ca-app-pub-8239080327367611/3531958929";

    private const string IosProductionInterstitialId =
        "ca-app-pub-8239080327367611/1532481547";

    // ---------------------------------------------------------
    // PUBLIC STATE
    // ---------------------------------------------------------

    public bool IsFullScreenAdShowing =>
        fullScreenAdShowing;

    public bool AdsInitialized =>
        adsInitialized;

    public bool IsRewardedReady
    {
        get
        {
#if UNITY_EDITOR
            if (simulateRewardInEditor)
            {
                return adsInitialized;
            }
#endif

            return adsInitialized &&
                   rewardedAd != null &&
                   rewardedAd.CanShowAd();
        }
    }

    public bool IsInterstitialReady
    {
        get
        {
#if UNITY_EDITOR
            if (simulateRewardInEditor)
            {
                return adsInitialized;
            }
#endif

            return adsInitialized &&
                   interstitialAd != null &&
                   interstitialAd.CanShowAd();
        }
    }

    // ---------------------------------------------------------
    // UNITY
    // ---------------------------------------------------------

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
    }

    // ConsentManager bunu çağıracak.
    public void InitializeAds()
    {
        if (adsInitialized)
            return;

        if (initializationStarted)
            return;

        initializationStarted = true;

#if UNITY_EDITOR
        if (simulateRewardInEditor)
        {
            adsInitialized = true;
            initializationStarted = false;

            Debug.Log(
                "AdMob editor simulation ready."
            );

            return;
        }
#endif

        MobileAds.Initialize(
            (InitializationStatus initStatus) =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(
                    () =>
                    {
                        initializationStarted = false;

                        if (initStatus == null)
                        {
                            Debug.LogError(
                                "AdMob initialization failed."
                            );

                            return;
                        }

                        adsInitialized = true;

                        Debug.Log(
                            "AdMob initialized after consent."
                        );

                        Debug.Log(
                            useTestAds
                                ? "AdMob mode: TEST ADS"
                                : "AdMob mode: PRODUCTION ADS"
                        );

                        // Rewarded reklam hemen hazırlanır.
                        LoadRewardedAd();

                        // Interstitial da arkada hazırlanır.
                        LoadInterstitialAd();
                    }
                );
            }
        );
    }

    // ---------------------------------------------------------
    // AD UNIT IDS
    // ---------------------------------------------------------

    private string GetRewardedAdUnitId()
    {
#if UNITY_IOS

        if (useTestAds)
        {
            return IosTestRewardedId;
        }

        return IosProductionRewardedId;

#elif UNITY_ANDROID

        return AndroidTestRewardedId;

#else

        return string.Empty;

#endif
    }

    private string GetInterstitialAdUnitId()
    {
#if UNITY_IOS

        if (useTestAds)
        {
            return IosTestInterstitialId;
        }

        return IosProductionInterstitialId;

#elif UNITY_ANDROID

        return AndroidTestInterstitialId;

#else

        return string.Empty;

#endif
    }

    // ---------------------------------------------------------
    // REWARDED LOAD
    // ---------------------------------------------------------

    public void LoadRewardedAd()
    {
        if (!adsInitialized)
            return;

        if (rewardedLoading)
            return;

        if (
            rewardedAd != null &&
            rewardedAd.CanShowAd()
        )
        {
            return;
        }

        string adUnitId =
            GetRewardedAdUnitId();

        if (string.IsNullOrEmpty(adUnitId))
            return;

        rewardedLoading = true;

        DestroyRewardedAd();

        Debug.Log(
            "Loading rewarded ad..."
        );

        AdRequest request =
            new AdRequest();

        RewardedAd.Load(
            adUnitId,
            request,
            (RewardedAd ad, LoadAdError error) =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(
                    () =>
                    {
                        rewardedLoading = false;

                        if (
                            error != null ||
                            ad == null
                        )
                        {
                            Debug.LogWarning(
                                "Rewarded ad load failed: " +
                                error
                            );

                            ScheduleRewardedRetry();

                            return;
                        }

                        CancelRewardedRetry();

                        rewardedAd = ad;

                        RegisterRewardedEvents(
                            ad
                        );

                        Debug.Log(
                            "Rewarded ad READY."
                        );
                    }
                );
            }
        );
    }

    private void ScheduleRewardedRetry()
    {
        if (
            rewardedRetryRoutine != null
        )
        {
            return;
        }

        rewardedRetryRoutine =
            StartCoroutine(
                RewardedRetryRoutine()
            );
    }

    private IEnumerator RewardedRetryRoutine()
    {
        Debug.Log(
            $"Rewarded retry in " +
            $"{rewardedRetryDelaySeconds:0}s."
        );

        yield return new WaitForSecondsRealtime(
            rewardedRetryDelaySeconds
        );

        rewardedRetryRoutine = null;

        LoadRewardedAd();
    }

    private void CancelRewardedRetry()
    {
        if (rewardedRetryRoutine == null)
            return;

        StopCoroutine(
            rewardedRetryRoutine
        );

        rewardedRetryRoutine = null;
    }

    // ---------------------------------------------------------
    // INTERSTITIAL LOAD
    // ---------------------------------------------------------

    public void LoadInterstitialAd()
    {
        if (!adsInitialized)
            return;

        if (interstitialLoading)
            return;

        if (
            interstitialAd != null &&
            interstitialAd.CanShowAd()
        )
        {
            return;
        }

        string adUnitId =
            GetInterstitialAdUnitId();

        if (string.IsNullOrEmpty(adUnitId))
            return;

        interstitialLoading = true;

        DestroyInterstitialAd();

        Debug.Log(
            "Loading interstitial ad..."
        );

        AdRequest request =
            new AdRequest();

        InterstitialAd.Load(
            adUnitId,
            request,
            (InterstitialAd ad, LoadAdError error) =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(
                    () =>
                    {
                        interstitialLoading = false;

                        if (
                            error != null ||
                            ad == null
                        )
                        {
                            Debug.LogWarning(
                                "Interstitial ad load failed: " +
                                error
                            );

                            ScheduleInterstitialRetry();

                            return;
                        }

                        CancelInterstitialRetry();

                        interstitialAd = ad;

                        RegisterInterstitialEvents(
                            ad
                        );

                        Debug.Log(
                            "Interstitial ad READY."
                        );
                    }
                );
            }
        );
    }

    private void ScheduleInterstitialRetry()
    {
        if (
            interstitialRetryRoutine != null
        )
        {
            return;
        }

        interstitialRetryRoutine =
            StartCoroutine(
                InterstitialRetryRoutine()
            );
    }

    private IEnumerator InterstitialRetryRoutine()
    {
        Debug.Log(
            $"Interstitial retry in " +
            $"{interstitialRetryDelaySeconds:0}s."
        );

        yield return new WaitForSecondsRealtime(
            interstitialRetryDelaySeconds
        );

        interstitialRetryRoutine = null;

        LoadInterstitialAd();
    }

    private void CancelInterstitialRetry()
    {
        if (
            interstitialRetryRoutine == null
        )
        {
            return;
        }

        StopCoroutine(
            interstitialRetryRoutine
        );

        interstitialRetryRoutine = null;
    }

    // ---------------------------------------------------------
    // SHOW REWARDED
    // ---------------------------------------------------------

    public void ShowRewarded(
        Action onRewardEarned
    )
    {
#if UNITY_EDITOR
        if (
            simulateRewardInEditor &&
            adsInitialized
        )
        {
            NotifyFullScreenAdStarted();

            Debug.Log(
                "Fake rewarded ad completed."
            );

            onRewardEarned?.Invoke();

            return;
        }
#endif

        if (fullScreenAdShowing)
            return;

        if (
            rewardedAd == null ||
            !rewardedAd.CanShowAd()
        )
        {
            Debug.LogWarning(
                "Rewarded ad is not ready yet."
            );

            LoadRewardedAd();

            return;
        }

        fullScreenAdShowing = true;

        NotifyFullScreenAdStarted();

        RewardedAd adToShow =
            rewardedAd;

        rewardedAd = null;

        adToShow.Show(
            (Reward reward) =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(
                    () =>
                    {
                        Debug.Log(
                            "Reward earned."
                        );

                        onRewardEarned?.Invoke();
                    }
                );
            }
        );
    }

    // ---------------------------------------------------------
    // SHOW INTERSTITIAL
    // ---------------------------------------------------------

    public bool ShowInterstitial()
    {
#if UNITY_EDITOR
        if (
            simulateRewardInEditor &&
            adsInitialized
        )
        {
            NotifyFullScreenAdStarted();

            Debug.Log(
                "Fake interstitial ad shown."
            );

            return true;
        }
#endif

        if (fullScreenAdShowing)
            return false;

        if (
            interstitialAd == null ||
            !interstitialAd.CanShowAd()
        )
        {
            Debug.Log(
                "Interstitial ad is not ready."
            );

            LoadInterstitialAd();

            return false;
        }

        fullScreenAdShowing = true;

        NotifyFullScreenAdStarted();

        InterstitialAd adToShow =
            interstitialAd;

        interstitialAd = null;

        adToShow.Show();

        return true;
    }

    private void NotifyFullScreenAdStarted()
    {
        if (
            AutoInterstitialManager.Instance != null
        )
        {
            AutoInterstitialManager.Instance
                .NotifyFullScreenAdShown();
        }
    }

    // ---------------------------------------------------------
    // REWARDED EVENTS
    // ---------------------------------------------------------

    private void RegisterRewardedEvents(
        RewardedAd ad
    )
    {
        ad.OnAdFullScreenContentOpened +=
            () =>
            {
                fullScreenAdShowing = true;
            };

        ad.OnAdFullScreenContentClosed +=
            () =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(
                    () =>
                    {
                        fullScreenAdShowing = false;

                        ad.Destroy();

                        Debug.Log(
                            "Rewarded ad closed."
                        );

                        LoadRewardedAd();
                    }
                );
            };

        ad.OnAdFullScreenContentFailed +=
            (AdError error) =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(
                    () =>
                    {
                        fullScreenAdShowing = false;

                        Debug.LogWarning(
                            "Rewarded ad failed to show: " +
                            error
                        );

                        ad.Destroy();

                        ScheduleRewardedRetry();
                    }
                );
            };
    }

    // ---------------------------------------------------------
    // INTERSTITIAL EVENTS
    // ---------------------------------------------------------

    private void RegisterInterstitialEvents(
        InterstitialAd ad
    )
    {
        ad.OnAdFullScreenContentOpened +=
            () =>
            {
                fullScreenAdShowing = true;
            };

        ad.OnAdFullScreenContentClosed +=
            () =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(
                    () =>
                    {
                        fullScreenAdShowing = false;

                        ad.Destroy();

                        Debug.Log(
                            "Interstitial ad closed."
                        );

                        LoadInterstitialAd();
                    }
                );
            };

        ad.OnAdFullScreenContentFailed +=
            (AdError error) =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(
                    () =>
                    {
                        fullScreenAdShowing = false;

                        Debug.LogWarning(
                            "Interstitial failed to show: " +
                            error
                        );

                        ad.Destroy();

                        ScheduleInterstitialRetry();
                    }
                );
            };
    }

    // ---------------------------------------------------------
    // CLEANUP
    // ---------------------------------------------------------

    private void DestroyRewardedAd()
    {
        if (rewardedAd == null)
            return;

        rewardedAd.Destroy();

        rewardedAd = null;
    }

    private void DestroyInterstitialAd()
    {
        if (interstitialAd == null)
            return;

        interstitialAd.Destroy();

        interstitialAd = null;
    }

    private void OnDestroy()
    {
        CancelRewardedRetry();
        CancelInterstitialRetry();

        DestroyRewardedAd();
        DestroyInterstitialAd();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}