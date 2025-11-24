using UnityEngine;

[CreateAssetMenu(menuName = "Machines/MachineData")]
public class MachineData : ScriptableObject
{
    public string machineName;
    [TextArea] public string description;
    public MachineType machineType; 
    public GameObject modelPrefab;
    [Header("Technical Specs")]
    public float defaultVoltage;
    public float defaultCurrent;
    public string[] components; 
}
public enum MachineType { AC, DC }

