using UnityEngine;

public class AutoInterstitialManager : MonoBehaviour
{
    public static AutoInterstitialManager Instance { get; private set; }

    [Header("Interstitial Timing")]
    [SerializeField] private float adIntervalSeconds = 120f;

    private float timer;
    private bool adDue;

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

    private void Update()
    {
        if (adDue)
            return;

        if (
            AdMobRewardedManager.Instance != null &&
            AdMobRewardedManager.Instance
                .IsFullScreenAdShowing
        )
        {
            return;
        }

        timer += Time.unscaledDeltaTime;

        if (timer < adIntervalSeconds)
            return;

        adDue = true;

        Debug.Log(
            "Interstitial due. " +
            "Waiting for next wash completion."
        );
    }

    public void NotifyWashCompleted()
    {
        if (!adDue)
            return;

        if (
            AdMobRewardedManager.Instance == null
        )
        {
            return;
        }

        if (
            AdMobRewardedManager.Instance
                .IsFullScreenAdShowing
        )
        {
            return;
        }

        bool shown =
            AdMobRewardedManager.Instance
                .ShowInterstitial();

        if (!shown)
        {
            Debug.Log(
                "Interstitial due but not ready. " +
                "Will try again after next wash."
            );

            return;
        }

        Debug.Log(
            "Interstitial shown."
        );
    }

    public void NotifyFullScreenAdShown()
    {
        ResetCooldown();

        Debug.Log(
            "Fullscreen ad shown. " +
            "Interstitial cooldown restarted."
        );
    }

    private void ResetCooldown()
    {
        timer = 0f;
        adDue = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}