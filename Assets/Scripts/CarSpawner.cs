using System.Collections;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("Cars")]
    [SerializeField] private GameObject[] carPrefabs;

    [Header("Station")]
    [SerializeField] private WashStationController stationController;

    [Header("Car Wash")]
    [SerializeField] private CarWashController carWashController;

    [Header("Path Points")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform washPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDelay = 2f;

    private bool carActive;

    private void Start()
    {
        if (stationController == null)
        {
            Debug.LogError(
                "CarSpawner: StationController missing."
            );

            return;
        }

        stationController.OnUnlocked +=
            OnStationUnlocked;

        if (stationController.IsUnlocked)
        {
            SpawnCar();
        }
    }

    private void OnStationUnlocked()
    {
        if (!carActive)
        {
            SpawnCar();
        }
    }

    private void SpawnCar()
    {
        if (carActive)
            return;

        if (
            stationController == null ||
            !stationController.IsUnlocked
        )
        {
            return;
        }

        if (
            carPrefabs == null ||
            carPrefabs.Length == 0
        )
        {
            Debug.LogError(
                "CarSpawner: No car prefabs assigned."
            );

            return;
        }

        if (carWashController == null)
        {
            Debug.LogError(
                "CarSpawner: CarWashController missing."
            );

            return;
        }

        if (
            spawnPoint == null ||
            entryPoint == null ||
            washPoint == null ||
            exitPoint == null
        )
        {
            Debug.LogError(
                "CarSpawner: Path point missing."
            );

            return;
        }

        int randomIndex =
            Random.Range(
                0,
                carPrefabs.Length
            );

        GameObject selectedPrefab =
            carPrefabs[randomIndex];

        if (selectedPrefab == null)
            return;

        GameObject newCar =
            Instantiate(
                selectedPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        CarMover carMover =
            newCar.GetComponent<CarMover>();

        if (carMover == null)
        {
            Destroy(newCar);
            return;
        }

        carActive = true;

        carMover.Initialize(
            entryPoint,
            washPoint,
            exitPoint,
            carWashController,
            stationController,
            OnCarFinished
        );
    }

    private void OnCarFinished()
    {
        StartCoroutine(
            SpawnNextCarAfterDelay()
        );
    }

    private IEnumerator SpawnNextCarAfterDelay()
    {
        yield return new WaitForSeconds(
            spawnDelay
        );

        carActive = false;

        if (
            stationController != null &&
            stationController.IsUnlocked
        )
        {
            SpawnCar();
        }
    }

    private void OnDestroy()
    {
        if (stationController != null)
        {
            stationController.OnUnlocked -=
                OnStationUnlocked;
        }
    }
}