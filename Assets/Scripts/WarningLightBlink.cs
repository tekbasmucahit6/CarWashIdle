using UnityEngine;

public class WarningLightBlink : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 0.3f;

    [Header("Lamp Colors")]
    [SerializeField] private Color normalColor =
        new Color(1f, 0.55f, 0.05f, 1f);

    [SerializeField] private Color brightColor =
        new Color(1f, 0.85f, 0.1f, 1f);

    [Header("Emission")]
    [SerializeField] private float normalEmission = 0.4f;
    [SerializeField] private float brightEmission = 4f;

    [Header("Optional Real Light")]
    [SerializeField] private Light warningLight;

    private Renderer[] lampRenderers;
    private Material[][] lampMaterials;

    private bool blinking;
    private bool brightState;
    private float timer;

    private void Awake()
    {
        lampRenderers =
            GetComponentsInChildren<Renderer>();

        lampMaterials =
            new Material[lampRenderers.Length][];

        for (int i = 0; i < lampRenderers.Length; i++)
        {
            lampMaterials[i] =
                lampRenderers[i].materials;

            foreach (Material material in lampMaterials[i])
            {
                if (material == null)
                    continue;

                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                }
            }
        }

        SetNormalState();
    }

    private void Update()
    {
        if (!blinking)
            return;

        timer += Time.deltaTime;

        if (timer < blinkInterval)
            return;

        timer = 0f;
        brightState = !brightState;

        if (brightState)
        {
            SetBrightState();
        }
        else
        {
            SetNormalState();
        }
    }

    public void SetBlinking(bool value)
    {
        blinking = value;
        timer = 0f;
        brightState = false;

        if (blinking)
        {
            SetBrightState();
            brightState = true;
        }
        else
        {
            SetNormalState();
        }
    }

    private void SetNormalState()
    {
        ApplyLampVisual(
            normalColor,
            normalEmission
        );

        if (warningLight != null)
        {
            warningLight.enabled = false;
        }
    }

    private void SetBrightState()
    {
        ApplyLampVisual(
            brightColor,
            brightEmission
        );

        if (warningLight != null)
        {
            warningLight.enabled = true;
        }
    }

    private void ApplyLampVisual(
        Color color,
        float emissionIntensity
    )
    {
        if (lampMaterials == null)
            return;

        Color emissionColor =
            color * emissionIntensity;

        foreach (Material[] materials in lampMaterials)
        {
            foreach (Material material in materials)
            {
                if (material == null)
                    continue;

                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor(
                        "_BaseColor",
                        color
                    );
                }
                else if (material.HasProperty("_Color"))
                {
                    material.SetColor(
                        "_Color",
                        color
                    );
                }

                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor(
                        "_EmissionColor",
                        emissionColor
                    );
                }
            }
        }
    }
}