using UnityEngine;
public interface IInputStrategy
{
    bool IsWeldingPressed();
    bool IsWeldingReleased();
    Vector3 GetPointerPosition();
    Vector3 GetPointerDirection();
}
