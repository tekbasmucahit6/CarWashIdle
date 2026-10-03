using UnityEngine;

public class BrushSpinner : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 180f;

    [Header("State")]
    [SerializeField] private bool spinning = false;

    public bool IsSpinning => spinning;

    private void Update()
    {
        if (!spinning)
            return;

        transform.Rotate(
            rotationAxis,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }

    public void SetSpinning(bool value)
    {
        spinning = value;
    }
}