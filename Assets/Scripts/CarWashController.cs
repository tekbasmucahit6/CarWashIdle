using System;
using System.Collections;
using UnityEngine;

public class CarWashController : MonoBehaviour
{
    [Header("Station")]
    [SerializeField] private WashStationController stationController;

    [Header("Brushes")]
    [SerializeField] private BrushSpinner brushLeft;
    [SerializeField] private BrushSpinner brushRight;
    [SerializeField] private BrushSpinner brushTop;

    [Header("Effects")]
    [SerializeField] private ParticleSystem waterSpray;
    [SerializeField] private ParticleSystem foamParticles;

    [Header("Warning Light")]
    [SerializeField] private WarningLightBlink warningLight;

    [Header("Fallback Settings")]
    [SerializeField] private float fallbackWashDuration = 2f;

    private bool washing;

    public bool IsWashing => washing;

    public WashStationController StationController =>
        stationController;

    private void Start()
    {
        SetBrushes(false);
        SetEffects(false);

        if (warningLight != null)
        {
            warningLight.SetBlinking(false);
        }
    }

    public void StartWash(
        Action onWashCompleted
    )
    {
        if (washing)
            return;

        StartCoroutine(
            WashRoutine(
                onWashCompleted
            )
        );
    }

    private IEnumerator WashRoutine(
        Action onWashCompleted
    )
    {
        washing = true;

        SetBrushes(true);
        SetEffects(true);

        if (warningLight != null)
        {
            warningLight.SetBlinking(true);
        }

        float washDuration =
            fallbackWashDuration;

        if (stationController != null)
        {
            washDuration =
                stationController
                    .CurrentWashDuration;
        }

        Debug.Log(
            $"Wash started. Duration: {washDuration:0.00}s"
        );

        yield return new WaitForSeconds(
            washDuration
        );

        SetBrushes(false);
        SetEffects(false);

        if (warningLight != null)
        {
            warningLight.SetBlinking(false);
        }

        washing = false;

        Debug.Log(
            "Wash completed."
        );

        onWashCompleted?.Invoke();
    }

    private void SetBrushes(
        bool active
    )
    {
        if (brushLeft != null)
        {
            brushLeft.SetSpinning(active);
        }

        if (brushRight != null)
        {
            brushRight.SetSpinning(active);
        }

        if (brushTop != null)
        {
            brushTop.SetSpinning(active);
        }
    }

    private void SetEffects(
        bool active
    )
    {
        SetParticleState(
            waterSpray,
            active
        );

        SetParticleState(
            foamParticles,
            active
        );
    }

    private void SetParticleState(
        ParticleSystem particles,
        bool active
    )
    {
        if (particles == null)
            return;

        if (active)
        {
            particles.Play();
        }
        else
        {
            particles.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );
        }
    }
}