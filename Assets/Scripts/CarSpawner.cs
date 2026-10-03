using System.Collections;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("Cars")]
    [SerializeField] private GameObject[] carPrefabs;

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
        SpawnCar();
    }

    private void SpawnCar()
    {
        if (carActive)
            return;

        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogError(
                "CarSpawner: No car prefabs assigned."
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
                "CarSpawner: One or more path points are missing."
            );

            return;
        }

        int randomIndex =
            Random.Range(0, carPrefabs.Length);

        GameObject selectedPrefab =
            carPrefabs[randomIndex];

        if (selectedPrefab == null)
        {
            Debug.LogError(
                "CarSpawner: A car prefab slot is empty."
            );

            return;
        }

        GameObject newCar = Instantiate(
            selectedPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CarMover carMover =
            newCar.GetComponent<CarMover>();

        if (carMover == null)
        {
            Debug.LogError(
                "CarSpawner: Spawned car does not contain CarMover."
            );

            Destroy(newCar);

            return;
        }

        carActive = true;

        carMover.Initialize(
            entryPoint,
            washPoint,
            exitPoint,
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

        SpawnCar();
    }
}