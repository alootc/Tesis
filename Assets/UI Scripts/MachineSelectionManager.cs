using UnityEngine;
using System;

public enum PlayMode { Practice, Evaluation }

public class MachineSelectionManager : MonoBehaviour
{
    public static MachineSelectionManager Instance { get; private set; }

    public event Action<PlayMode> OnModeSelected;
    public event Action<MachineData> OnMachineSelected;

    public PlayMode SelectedMode { get; private set; }
    public MachineData SelectedMachine { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMode(PlayMode mode)
    {
        SelectedMode = mode;
        OnModeSelected?.Invoke(mode);
    }

    public void SetMachine(MachineData machine)
    {
        SelectedMachine = machine;
        OnMachineSelected?.Invoke(machine);
    }

    // Optional validation helper
    public bool HasSelection() => SelectedMachine != null;
}




