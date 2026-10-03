using UnityEngine;

public class AutoInterstitialManager : MonoBehaviour
{
    public static AutoInterstitialManager Instance { get; private set; }

    [Header("Interstitial Timing")]
    [SerializeField] private float firstAdDelaySeconds = 180f;
    [SerializeField] private float adIntervalSeconds = 120f;

    private float timer;
    private bool adDue;
    private bool firstInterstitialShown;

    private void Awake()
    {
        if (Instance != null && Instance != this)
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
            AdMobRewardedManager.Instance.IsFullScreenAdShowing
        )
        {
            return;
        }

        timer += Time.unscaledDeltaTime;

        float requiredTime =
            firstInterstitialShown
                ? adIntervalSeconds
                : firstAdDelaySeconds;

        if (timer >= requiredTime)
        {
            adDue = true;

            Debug.Log(
                "Interstitial ready. Waiting for wash completion."
            );
        }
    }

    public void NotifyWashCompleted()
    {
        if (!adDue)
            return;

        if (AdMobRewardedManager.Instance == null)
            return;

        if (
            AdMobRewardedManager.Instance.IsFullScreenAdShowing
        )
        {
            return;
        }

        bool shown =
            AdMobRewardedManager.Instance.ShowInterstitial();

        if (!shown)
            return;

        firstInterstitialShown = true;

        ResetCooldown();

        Debug.Log(
            "Interstitial shown. Cooldown restarted."
        );
    }

    public void NotifyFullScreenAdShown()
    {
        ResetCooldown();

        Debug.Log(
            "Fullscreen ad shown. Interstitial cooldown restarted."
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