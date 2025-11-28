using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.XR;
using UnityEngine;
using Input = UnityEngine.Input;

public class PCInputStrategy : IInputStrategy
{
    public bool IsWeldingPressed()
    {
        return Input.GetMouseButtonDown(0);
    }

    public bool IsWeldingReleased()
    {
        return Input.GetMouseButtonUp(0);
    }

    public Vector3 GetPointerPosition()
    {
        return Camera.main.ScreenToWorldPoint(
            Input.mousePosition + new Vector3(0, 0, 1)
        );
    }

    public Vector3 GetPointerDirection()
    {
        return Camera.main.transform.forward;
    }
}

