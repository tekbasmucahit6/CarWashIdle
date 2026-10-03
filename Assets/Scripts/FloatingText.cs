using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 100f;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 1.1f;

    [Header("Scale")]
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float maxScale = 1.25f;

    private TMP_Text textComponent;
    private CanvasGroup canvasGroup;

    private float timer;

    private void Awake()
    {
        textComponent =
            GetComponent<TMP_Text>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

        transform.localScale =
            Vector3.one * startScale;
    }

    public void SetText(string value)
    {
        if (textComponent != null)
        {
            textComponent.text = value;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.Translate(
            Vector3.up *
            moveSpeed *
            Time.deltaTime
        );

        float progress =
            Mathf.Clamp01(
                timer / lifeTime
            );

        UpdateScale(progress);
        UpdateFade(progress);

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateScale(float progress)
    {
        float scale;

        if (progress < 0.25f)
        {
            scale = Mathf.Lerp(
                startScale,
                maxScale,
                progress / 0.25f
            );
        }
        else
        {
            scale = Mathf.Lerp(
                maxScale,
                1f,
                (progress - 0.25f) / 0.75f
            );
        }

        transform.localScale =
            Vector3.one * scale;
    }

    private void UpdateFade(float progress)
    {
        if (canvasGroup == null)
            return;

        if (progress < 0.55f)
        {
            canvasGroup.alpha = 1f;
            return;
        }

        canvasGroup.alpha =
            Mathf.Lerp(
                1f,
                0f,
                (progress - 0.55f) / 0.45f
            );
    }
}