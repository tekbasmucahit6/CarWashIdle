using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animation")]
    [SerializeField] private float pressedScale = 0.92f;
    [SerializeField] private float animationSpeed = 12f;

    private Vector3 originalScale;
    private Coroutine scaleRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AnimateTo(originalScale * pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AnimateTo(originalScale);
    }

    private void AnimateTo(Vector3 targetScale)
    {
        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
        }

        scaleRoutine = StartCoroutine(
            ScaleRoutine(targetScale)
        );
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        while (
            Vector3.Distance(
                transform.localScale,
                targetScale
            ) > 0.01f
        )
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                animationSpeed * Time.unscaledDeltaTime
            );

            yield return null;
        }

        transform.localScale = targetScale;
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
    }
}