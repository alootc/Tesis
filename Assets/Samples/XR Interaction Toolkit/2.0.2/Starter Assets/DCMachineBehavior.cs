using UnityEngine;

[CreateAssetMenu(menuName = "Machines/Behaviors/DC Behavior")]
public class DCMachineBehavior : ScriptableObject, IMachineBehavior
{
    [Header("DC Properties")]
    public float baseIntensity = 1f;
    public float stabilityBonus = 0.25f;
    public float penetrationMultiplier = 1.2f;

    public void OnStart() { }
    public void OnStop() { }

    public float GetArcIntensity(float distance, float speed, float voltage)
    {
        float intensity = baseIntensity
                        * (voltage / 25f)
                        * Mathf.Clamp01(1f - distance * 0.15f);

        return intensity * penetrationMultiplier;
    }

    public float GetStabilityModifier(float distance, float speed)
    {
        return Mathf.Clamp01(1f - distance * 0.1f) + stabilityBonus;
    }
}

