using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRInputStrategy : IInputStrategy
{
    private XRBaseController controller;

    public VRInputStrategy(XRBaseController controller)
    {
        this.controller = controller;
    }

    public bool IsWeldingPressed()
    {
        return controller.activateInteractionState.activatedThisFrame;
    }

    public bool IsWeldingReleased()
    {
        return controller.activateInteractionState.deactivatedThisFrame;
    }

    public Vector3 GetPointerPosition()
    {
        return controller.transform.position;
    }

    public Vector3 GetPointerDirection()
    {
        return controller.transform.forward;
    }
}
