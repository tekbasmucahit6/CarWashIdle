using System;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

public class AdMobRewardedManager : MonoBehaviour
{
    public static AdMobRewardedManager Instance { get; private set; }

    [Header("Editor Test")]
    [SerializeField] private bool simulateRewardInEditor = true;

    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;

    private bool fullScreenAdShowing;
    private bool adsInitialized;

    private const string AndroidTestRewardedId =
        "ca-app-pub-3940256099942544/5224354917";

    private const string IosTestRewardedId =
        "ca-app-pub-3940256099942544/1712485313";

    private const string AndroidTestInterstitialId =
        "ca-app-pub-3940256099942544/1033173712";

    private const string IosTestInterstitialId =
        "ca-app-pub-3940256099942544/4411468910";

    private const string IosProductionRewardedId =
        "ca-app-pub-8239080327367611/3531958929";

    private const string IosProductionInterstitialId =
        "ca-app-pub-8239080327367611/1532481547";

    public bool IsFullScreenAdShowing =>
        fullScreenAdShowing;

    public bool IsRewardedReady
    {
        get
        {
#if UNITY_EDITOR
            if (simulateRewardInEditor)
                return adsInitialized;
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
                return adsInitialized;
#endif

            return adsInitialized &&
                   interstitialAd != null &&
                   interstitialAd.CanShowAd();
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

    public void InitializeAds()
    {
        if (adsInitialized)
            return;

#if UNITY_EDITOR
        if (simulateRewardInEditor)
        {
            adsInitialized = true;

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
                            Debug.isDebugBuild
                                ? "AdMob mode: TEST ADS"
                                : "AdMob mode: PRODUCTION ADS"
                        );

                        LoadRewardedAd();
                        LoadInterstitialAd();
                    }
                );
            }
        );
    }

    private string GetRewardedAdUnitId()
    {
#if UNITY_IOS
        if (Debug.isDebugBuild)
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
        if (Debug.isDebugBuild)
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

    public void LoadRewardedAd()
    {
        if (!adsInitialized)
            return;

        string adUnitId =
            GetRewardedAdUnitId();

        if (string.IsNullOrEmpty(adUnitId))
            return;

        DestroyRewardedAd();

        AdRequest request =
            new AdRequest();

        RewardedAd.Load(
            adUnitId,
            request,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning(
                        "Rewarded ad load failed: " +
                        error
                    );

                    return;
                }

                rewardedAd = ad;

                RegisterRewardedEvents(ad);

                Debug.Log(
                    "Rewarded ad loaded."
                );
            }
        );
    }

    public void LoadInterstitialAd()
    {
        if (!adsInitialized)
            return;

        string adUnitId =
            GetInterstitialAdUnitId();

        if (string.IsNullOrEmpty(adUnitId))
            return;

        DestroyInterstitialAd();

        AdRequest request =
            new AdRequest();

        InterstitialAd.Load(
            adUnitId,
            request,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning(
                        "Interstitial ad load failed: " +
                        error
                    );

                    return;
                }

                interstitialAd = ad;

                RegisterInterstitialEvents(ad);

                Debug.Log(
                    "Interstitial ad loaded."
                );
            }
        );
    }

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
                "Rewarded ad is not ready."
            );

            LoadRewardedAd();

            return;
        }

        fullScreenAdShowing = true;

        NotifyFullScreenAdStarted();

        rewardedAd.Show(
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

        interstitialAd.Show();

        return true;
    }

    private void NotifyFullScreenAdStarted()
    {
        if (AutoInterstitialManager.Instance != null)
        {
            AutoInterstitialManager.Instance
                .NotifyFullScreenAdShown();
        }
    }

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

                        if (rewardedAd == ad)
                        {
                            rewardedAd = null;
                        }

                        ad.Destroy();

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
                            "Rewarded ad failed: " +
                            error
                        );

                        if (rewardedAd == ad)
                        {
                            rewardedAd = null;
                        }

                        ad.Destroy();

                        LoadRewardedAd();
                    }
                );
            };
    }

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

                        if (interstitialAd == ad)
                        {
                            interstitialAd = null;
                        }

                        ad.Destroy();

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
                            "Interstitial ad failed: " +
                            error
                        );

                        if (interstitialAd == ad)
                        {
                            interstitialAd = null;
                        }

                        ad.Destroy();

                        LoadInterstitialAd();
                    }
                );
            };
    }

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
        DestroyRewardedAd();
        DestroyInterstitialAd();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}