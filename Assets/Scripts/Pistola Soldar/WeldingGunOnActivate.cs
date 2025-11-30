using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

public class WeldingGunOnActivate : MonoBehaviour
{
    [Header("Efectos")]
    public ParticleSystem sparkEffect;
    public ParticleSystem fireEffect;
    public Transform spawnPoint;

    [Header("Control de Soldadura")]
    public AcidDecalSpawner acidDecalSpawner;
    public ObjectPoolManager poolManagerSparks;
    public ObjectPoolManager poolManagerFire;

    // Comentado: Se necesita una referencia a PistolaSphereCastMerge para obtener la pieza impactada si no se usa el raycast directamente
    // Usaremos el raycast directamente para obtener la pieza.

    private GameObject currentFireGO;
    private GameObject currentSparksGO;
    private ParticleSystem currentFire;
    private ParticleSystem currentSparks;

    private bool isTriggerPressed = false;
    private bool isNearMetal = false;
    private Vector3 metalContactPoint;

    private MachineData selectedMachineData;
    private IMachineBehavior machineBehavior;

    // Comentado: Nueva: Referencia a la pieza de metal que se está soldando actualmente
    private NewPart currentWeldedPart = null;

    void Start()
    {
        XRGrabInteractable grabbable = GetComponent<XRGrabInteractable>();
        grabbable.activated.AddListener(StartWelding);
        grabbable.deactivated.AddListener(StopWelding);

        if (MachineSelectionManager.Instance != null)
        {
            MachineSelectionManager.Instance.OnMachineSelected += OnMachineSelected;
            OnMachineSelected(MachineSelectionManager.Instance.SelectedMachine);
        }
    }

    private void OnMachineSelected(MachineData machine)
    {
        selectedMachineData = machine;
        machineBehavior = selectedMachineData?.behavior;
        if (machineBehavior == null && selectedMachineData != null)
        {
            Debug.LogError($"La máquina '{selectedMachineData.machineName}' no tiene un IMachineBehavior asignado.");
        }
        else if (machineBehavior != null)
        {
            Debug.Log($"Comportamiento de soldadura actualizado a: {selectedMachineData.machineType}");
        }
    }

    private void OnDestroy()
    {
        if (MachineSelectionManager.Instance != null)
        {
            MachineSelectionManager.Instance.OnMachineSelected -= OnMachineSelected;
        }
    }

    void Update()
    {
        if (selectedMachineData == null || machineBehavior == null) return;

        if (isTriggerPressed && currentFire != null)
        {
            currentFireGO.transform.position = spawnPoint.position;
            currentFireGO.transform.rotation = spawnPoint.rotation;

            CheckForMetal();
        }

        // Comentado: El decal ahora debe depender del comportamiento de la máquina
        if (isTriggerPressed && isNearMetal)
        {
            // Pasa el comportamiento de la máquina al decal spawner
            acidDecalSpawner.SpawnAcidDecal(metalContactPoint, machineBehavior);
        }
    }

    public void StartWelding(ActivateEventArgs arg)
    {
        if (selectedMachineData == null || machineBehavior == null) return;

        isTriggerPressed = true;

        if (poolManagerFire != null && fireEffect != null)
        {
            currentFireGO = poolManagerFire.GetObject(spawnPoint.position, spawnPoint.rotation, spawnPoint);
            currentFire = currentFireGO?.GetComponent<ParticleSystem>();

            if (currentFire != null)
            {
                currentFire.Play();
                machineBehavior.OnStart();
            }
        }
        else
        {
            Debug.LogError("¡Falta asignar poolManagerFire!");
        }

        CheckForMetal();
    }

    public void StopWelding(DeactivateEventArgs arg)
    {
        isTriggerPressed = false;
        isNearMetal = false;

        if (machineBehavior != null)
        {
            machineBehavior.OnStop();
        }

        // Comentado: Llama a StopWelding en la pieza ANTES de detener el arco
        if (currentWeldedPart != null)
        {
            currentWeldedPart.StopWelding();
            currentWeldedPart = null;
        }

        if (currentFireGO != null)
        {
            currentFire?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            currentFireGO.SetActive(false);
            currentFire = null;
            currentFireGO = null;
        }

        if (currentSparksGO != null)
        {
            currentSparks?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            currentSparksGO.SetActive(false);
            currentSparks = null;
            currentSparksGO = null;
        }
    }

    private void CheckForMetal()
    {
        RaycastHit hit;
        float maxRayDistance = 1.0f;

        NewPart previousWeldedPart = currentWeldedPart;
        currentWeldedPart = null; // Reiniciar la pieza soldada en cada frame

        if (selectedMachineData != null && machineBehavior != null)
        {
            // Usa la estabilidad de la máquina para modular la distancia del arco
            float stability = machineBehavior.GetStabilityModifier(0, selectedMachineData.defaultCurrent);
            maxRayDistance *= stability;
            maxRayDistance = Mathf.Clamp(maxRayDistance, 0.2f, 1.5f); // Distancia máxima efectiva del arco
        }

        if (Physics.Raycast(spawnPoint.position, spawnPoint.forward, out hit, maxRayDistance))
        {
            if (hit.collider.CompareTag("Metal"))
            {
                isNearMetal = true;
                metalContactPoint = hit.point;

                NewPart hitPart = hit.collider.GetComponent<NewPart>();

                // 1. Iniciar Soldadura en la pieza si no estaba soldando o si es una pieza nueva
                if (hitPart != null)
                {
                    currentWeldedPart = hitPart;
                    if (previousWeldedPart != currentWeldedPart || !previousWeldedPart.isBeingWelded)
                    {
                        currentWeldedPart.StartWelding();
                        Debug.Log($"Iniciando soldadura en pieza: {currentWeldedPart.gameObject.name}");
                    }
                }

                // 2. Aplica el ácido (Decal)
                CubeWeldingController cubeController = hit.collider.GetComponent<CubeWeldingController>();
                if (cubeController != null)
                {
                    cubeController.ApplyAcid();
                }

                // 3. Modifica la Intensidad Visual del Arco (Fuego)
                if (machineBehavior != null && currentFire != null)
                {
                    float arcIntensity = machineBehavior.GetArcIntensity(hit.distance, selectedMachineData.defaultCurrent, selectedMachineData.defaultVoltage);
                    var main = currentFire.main;

                    // Comentado: La intensidad del arco modifica el tamaño del efecto
                    main.startSizeMultiplier = 1.0f + (arcIntensity * 0.5f);
                }

                // 4. Activa las Chispas (Sparks)
                if (currentSparksGO == null)
                {
                    if (poolManagerSparks != null && sparkEffect != null)
                    {
                        currentSparksGO = poolManagerSparks.GetObject(hit.point, Quaternion.identity, hit.transform);
                        currentSparks = currentSparksGO?.GetComponent<ParticleSystem>();

                        if (currentSparks != null)
                        {
                            currentSparks.Play();
                        }
                    }
                    else
                    {
                        Debug.LogError("¡Falta asignar poolManagerSparks!");
                    }
                }
                else
                {
                    currentSparksGO.transform.position = hit.point;
                    currentSparksGO.transform.parent = hit.transform;
                }
            }
            else
            {
                // Si el objeto NO es de metal (o el rayo lo pasa de largo)
                HandleStopWeldingOnMetalMiss(previousWeldedPart);
            }
        }
        else
        {
            // Si no hay colisión (arco muy largo)
            HandleStopWeldingOnMetalMiss(previousWeldedPart);
        }
    }

    // Comentado: Nuevo método para manejar la detención de la soldadura en la pieza al perder el contacto.
    private void HandleStopWeldingOnMetalMiss(NewPart previousPart)
    {
        isNearMetal = false;

        // 1. Detener soldadura en la pieza previa
        if (previousPart != null && previousPart.isBeingWelded)
        {
            previousPart.StopWelding();
        }

        // 2. Detener las chispas
        if (currentSparksGO != null)
        {
            currentSparks?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            currentSparksGO.SetActive(false);
            currentSparks = null;
            currentSparksGO = null;
        }
    }
}
