using UnityEngine;

[CreateAssetMenu(menuName = "Machines/Behaviors/AC Behavior")]
public class ACMachineBehavior : ScriptableObject, IMachineBehavior
{
    [Header("AC Properties")]
    public float baseIntensity = 0.8f; // menor que DC
    public float instabilityFactor = 0.4f; // más inestable
    public float penetrationMultiplier = 0.7f; // menor penetración

    public void OnStart() { }
    public void OnStop() { }

    public float GetArcIntensity(float distance, float speed, float voltage)
    {
        float intensity = baseIntensity
                        * (voltage / 25f)
                        * Mathf.Clamp01(1f - distance * 0.2f);

        // Variación AC ? simula la alternancia 60 Hz
        intensity *= 1f + Mathf.Sin(Time.time * 120f) * 0.15f;

        return intensity * penetrationMultiplier;
    }

    public float GetStabilityModifier(float distance, float speed)
    {
        return Mathf.Clamp01(1f - (distance * instabilityFactor));
    }
}

