using UnityEngine;

public class MachineModelRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 80f;
    [SerializeField] private float inertia = 0.95f;

    private float currentSpeed;
    private Vector2 input; 

    private void Update()
    {
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
    //VR
    public void RotateByDelta(float delta)
    {
        transform.Rotate(Vector3.up, delta * Time.deltaTime, Space.World);
    }
}
