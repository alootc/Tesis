public interface IMachineBehavior
{
    float GetArcIntensity(float distance, float speed, float voltage);
    float GetStabilityModifier(float distance, float speed);
    void OnStart();
    void OnStop();
}
