using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.XR;
public static class InputStrategyResolver
{
    public static IInputStrategy GetStrategy(XRBaseController vrController = null)
    {
#if UNITY_XR_MANAGEMENT
        if (vrController != null)
        {
            return new VRInputStrategy(vrController);
        }
#endif
        return new PCInputStrategy();
    }
}
