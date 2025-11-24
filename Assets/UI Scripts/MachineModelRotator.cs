using UnityEngine;

public class MachineModelRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 80f;
    [SerializeField] private float inertia = 0.95f;

    private float currentSpeed;
    private Vector2 input; // x = horizontal drag, y ignored

    private void Update()
    {
        // simple mouse/drag rotation for editor; replace with VR input as needed
        if (Input.GetMouseButton(0))
        {
            input.x = Input.GetAxis("Mouse X");
            currentSpeed = input.x * rotationSpeed;
        }
        else
        {
            currentSpeed *= inertia;
        }

        transform.Rotate(Vector3.up, -currentSpeed * Time.deltaTime, Space.World);
    }

    // Expose method for VR thumbstick or drag input
    public void RotateByDelta(float delta)
    {
        transform.Rotate(Vector3.up, delta * Time.deltaTime, Space.World);
    }
}


