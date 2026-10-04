using System.Collections;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [Header("Coins")]
    [SerializeField] private TMP_Text coinsText;

    [Header("Floating Coin")]
    [SerializeField] private GameObject floatingCoinPrefab;
    [SerializeField] private RectTransform floatingCoinSpawnPoint;

    [Header("Animation")]
    [SerializeField] private float coinPopScale = 1.2f;
    [SerializeField] private float animationDuration = 0.15f;

    private Vector3 originalCoinScale;

    private Coroutine coinAnimationRoutine;

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

    private void Start()
    {
        if (coinsText != null)
        {
            originalCoinScale =
                coinsText.transform.localScale;
        }

        if (GameEconomy.Instance != null)
        {
            GameEconomy.Instance.OnCoinsChanged +=
                OnCoinsChanged;
        }

        RefreshCoins();
    }

    private void OnCoinsChanged(int coins)
    {
        RefreshCoins();

        if (coinAnimationRoutine != null)
        {
            StopCoroutine(
                coinAnimationRoutine
            );
        }

        coinAnimationRoutine =
            StartCoroutine(
                CoinPopAnimation()
            );
    }

    public void ShowFloatingCoins(int amount)
    {
        if (
            floatingCoinPrefab == null ||
            floatingCoinSpawnPoint == null
        )
        {
            return;
        }

        GameObject instance =
            Instantiate(
                floatingCoinPrefab,
                floatingCoinSpawnPoint.position,
                Quaternion.identity,
                floatingCoinSpawnPoint.parent
            );

        FloatingText floatingText =
            instance.GetComponent<FloatingText>();

        if (floatingText != null)
        {
            floatingText.SetText(
                $"+{amount}"
            );
        }
    }

    private void RefreshCoins()
    {
        if (
            coinsText == null ||
            GameEconomy.Instance == null
        )
        {
            return;
        }

        coinsText.text =
            $"COINS {FormatNumber(GameEconomy.Instance.Coins)}";
    }

    private IEnumerator CoinPopAnimation()
    {
        if (coinsText == null)
            yield break;

        Vector3 targetScale =
            originalCoinScale *
            coinPopScale;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;

            coinsText.transform.localScale =
                Vector3.Lerp(
                    originalCoinScale,
                    targetScale,
                    timer /
                    animationDuration
                );

            yield return null;
        }

        timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;

            coinsText.transform.localScale =
                Vector3.Lerp(
                    targetScale,
                    originalCoinScale,
                    timer /
                    animationDuration
                );

            yield return null;
        }

        coinsText.transform.localScale =
            originalCoinScale;
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

    private void OnDestroy()
    {
        if (GameEconomy.Instance != null)
        {
            GameEconomy.Instance.OnCoinsChanged -=
                OnCoinsChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}