using System;
using UnityEngine;

public class CarMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float normalSpeed = 3f;
    [SerializeField] private float rotationSpeed = 6f;

    [Header("Fallback Reward")]
    [SerializeField] private int fallbackRewardCoins = 10;

    private Transform entryPoint;
    private Transform washPoint;
    private Transform exitPoint;

    private int currentStage;
    private Action onFinished;

    private bool initialized;
    private bool finished;
    private bool waitingForWash;
    private bool rewardGiven;

    public void Initialize(
        Transform newEntryPoint,
        Transform newWashPoint,
        Transform newExitPoint,
        Action finishedCallback
    )
    {
        entryPoint = newEntryPoint;
        washPoint = newWashPoint;
        exitPoint = newExitPoint;
        onFinished = finishedCallback;

        currentStage = 0;

        initialized = true;
        finished = false;
        waitingForWash = false;
        rewardGiven = false;

        enabled = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (finished)
            return;

        if (waitingForWash)
            return;

        Transform target =
            GetCurrentTarget();

        if (target == null)
            return;

        MoveTowardsTarget(
            target,
            normalSpeed
        );

        CheckTargetReached(
            target
        );
    }

    private void MoveTowardsTarget(
        Transform target,
        float speed
    )
    {
        Vector3 targetPosition =
            target.position;

        targetPosition.y =
            transform.position.y;

        Vector3 direction =
            targetPosition -
            transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed *
                Time.deltaTime
            );
    }

    private void CheckTargetReached(
        Transform target
    )
    {
        Vector3 targetPosition =
            target.position;

        targetPosition.y =
            transform.position.y;

        float distance =
            Vector3.Distance(
                transform.position,
                targetPosition
            );

        if (distance > 0.08f)
            return;

        if (currentStage == 0)
        {
            currentStage = 1;
            return;
        }

        if (currentStage == 1)
        {
            StartWashing();
            return;
        }

        if (currentStage == 2)
        {
            FinishCar();
        }
    }

    private void StartWashing()
    {
        if (waitingForWash)
            return;

        waitingForWash = true;

        if (CarWashController.Instance == null)
        {
            Debug.LogWarning(
                "CarWashController missing. Wash skipped."
            );

            OnWashCompleted();

            return;
        }

        CarWashController.Instance.StartWash(
            OnWashCompleted
        );
    }

    private void OnWashCompleted()
    {
        waitingForWash = false;

        GiveWashReward();

        if (AutoInterstitialManager.Instance != null)
        {
            AutoInterstitialManager.Instance
                .NotifyWashCompleted();
        }

        currentStage = 2;
    }

    private void GiveWashReward()
    {
        if (rewardGiven)
            return;

        rewardGiven = true;

        int reward =
            fallbackRewardCoins;

        if (WashUpgradeManager.Instance != null)
        {
            reward =
                WashUpgradeManager.Instance
                    .CurrentReward;
        }

        int multiplier = 1;

        if (IncomeBoostManager.Instance != null)
        {
            multiplier =
                IncomeBoostManager.Instance
                    .CurrentMultiplier;
        }

        int finalReward =
            reward * multiplier;

        if (GameEconomy.Instance != null)
        {
            GameEconomy.Instance.AddCoins(
                finalReward
            );
        }

        GameUI gameUI =
            FindFirstObjectByType<GameUI>();

        if (gameUI != null)
        {
            gameUI.ShowFloatingCoins(
                finalReward
            );
        }

        Debug.Log(
            $"Wash completed. +{finalReward} coins. " +
            $"Multiplier: x{multiplier}"
        );
    }

    private Transform GetCurrentTarget()
    {
        switch (currentStage)
        {
            case 0:
                return entryPoint;

            case 1:
                return washPoint;

            case 2:
                return exitPoint;

            default:
                return null;
        }
    }

    private void FinishCar()
    {
        if (finished)
            return;

        finished = true;

        onFinished?.Invoke();

        Destroy(gameObject);
    }
}