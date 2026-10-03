using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text upgradeButtonText;

    [Header("Buttons")]
    [SerializeField] private Button upgradeButton;

    [Header("Floating Coin")]
    [SerializeField] private GameObject floatingCoinPrefab;
    [SerializeField] private RectTransform floatingCoinSpawnPoint;

    [Header("Level Up")]
    [SerializeField] private TMP_Text levelUpText;

    [Header("Animation")]
    [SerializeField] private float coinPopScale = 1.2f;
    [SerializeField] private float animationDuration = 0.15f;

    private Vector3 originalCoinScale;

    private Coroutine coinAnimationRoutine;
    private Coroutine levelUpRoutine;

    private void Start()
    {
        // Upgrade butonunun text referansı boşsa
        // otomatik olarak butonun içinden bul.
        if (
            upgradeButtonText == null &&
            upgradeButton != null
        )
        {
            upgradeButtonText =
                upgradeButton.GetComponentInChildren<TMP_Text>();
        }

        if (coinsText != null)
        {
            originalCoinScale =
                coinsText.transform.localScale;
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();

            upgradeButton.onClick.AddListener(
                OnUpgradeButtonPressed
            );
        }

        if (levelUpText != null)
        {
            levelUpText.gameObject.SetActive(false);
        }

        SubscribeToEvents();

        RefreshUI();
    }

    private void SubscribeToEvents()
    {
        if (GameEconomy.Instance != null)
        {
            GameEconomy.Instance.OnCoinsChanged +=
                OnCoinsChanged;
        }

        if (WashUpgradeManager.Instance != null)
        {
            WashUpgradeManager.Instance.OnUpgradeChanged +=
                OnUpgradeChanged;
        }
    }

    private void OnCoinsChanged(int coins)
    {
        RefreshUI();

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

    private void OnUpgradeChanged()
    {
        RefreshUI();

        if (levelUpRoutine != null)
        {
            StopCoroutine(
                levelUpRoutine
            );
        }

        levelUpRoutine =
            StartCoroutine(
                LevelUpAnimation()
            );
    }

    private void OnUpgradeButtonPressed()
    {
        if (WashUpgradeManager.Instance == null)
            return;

        WashUpgradeManager.Instance.TryUpgrade();

        // Upgrade sonrası fiyatı anında yenile.
        RefreshUI();
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

    private void RefreshUI()
    {
        RefreshCoins();
        RefreshUpgrade();
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

    private void RefreshUpgrade()
    {
        if (WashUpgradeManager.Instance == null)
            return;

        int level =
            WashUpgradeManager.Instance.Level;

        int nextPrice =
            WashUpgradeManager.Instance.CurrentUpgradeCost;

        bool isMax =
            WashUpgradeManager.Instance.IsMaxLevel;

        if (levelText != null)
        {
            levelText.text =
                $"WASH LV. {level}";
        }

        if (upgradeButtonText != null)
        {
            if (isMax)
            {
                upgradeButtonText.text =
                    "MAX LEVEL";
            }
            else
            {
                upgradeButtonText.text =
                    $"WASH SPEED\nUPGRADE • {FormatNumber(nextPrice)}";
            }
        }

        if (
            upgradeButton != null &&
            GameEconomy.Instance != null
        )
        {
            upgradeButton.interactable =
                !isMax &&
                GameEconomy.Instance.CanAfford(
                    nextPrice
                );
        }
    }

    private IEnumerator CoinPopAnimation()
    {
        if (coinsText == null)
            yield break;

        Vector3 targetScale =
            originalCoinScale * coinPopScale;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;

            coinsText.transform.localScale =
                Vector3.Lerp(
                    originalCoinScale,
                    targetScale,
                    timer / animationDuration
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
                    timer / animationDuration
                );

            yield return null;
        }

        coinsText.transform.localScale =
            originalCoinScale;
    }

    private IEnumerator LevelUpAnimation()
    {
        if (
            levelUpText == null ||
            WashUpgradeManager.Instance == null
        )
        {
            yield break;
        }

        levelUpText.gameObject.SetActive(true);

        levelUpText.text =
            $"LEVEL {WashUpgradeManager.Instance.Level}!";

        levelUpText.transform.localScale =
            Vector3.zero;

        float timer = 0f;

        while (timer < 0.2f)
        {
            timer += Time.deltaTime;

            levelUpText.transform.localScale =
                Vector3.Lerp(
                    Vector3.zero,
                    Vector3.one * 1.2f,
                    timer / 0.2f
                );

            yield return null;
        }

        yield return new WaitForSeconds(0.6f);

        timer = 0f;

        Vector3 startScale =
            levelUpText.transform.localScale;

        while (timer < 0.2f)
        {
            timer += Time.deltaTime;

            levelUpText.transform.localScale =
                Vector3.Lerp(
                    startScale,
                    Vector3.zero,
                    timer / 0.2f
                );

            yield return null;
        }

        levelUpText.gameObject.SetActive(false);
    }

    private string FormatNumber(int number)
    {
        if (number >= 1_000_000_000)
        {
            return
                $"{number / 1_000_000_000f:0.#}B";
        }

        if (number >= 1_000_000)
        {
            return
                $"{number / 1_000_000f:0.#}M";
        }

        if (number >= 1_000)
        {
            return
                $"{number / 1_000f:0.#}K";
        }

        return number.ToString();
    }

    private void OnDestroy()
    {
        if (GameEconomy.Instance != null)
        {
            GameEconomy.Instance.OnCoinsChanged -=
                OnCoinsChanged;
        }

        if (WashUpgradeManager.Instance != null)
        {
            WashUpgradeManager.Instance.OnUpgradeChanged -=
                OnUpgradeChanged;
        }
    }
}