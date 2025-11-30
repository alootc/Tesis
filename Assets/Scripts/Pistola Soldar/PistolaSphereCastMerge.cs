using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Necesario para Text e Image
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Necesario para XRGrabInteractable

public class PistolaSphereCastMerge : MonoBehaviour
{
    public float sphereRadius = 1.0f; // Radio del SphereCast
    public float maxDistance = 5.0f;  // Distancia máxima del SphereCast
    public LayerMask layerMask; // Define qué capas detectar
    private List<GameObject> detectedObjects = new List<GameObject>();
    public Transform pivot;
    bool Press;
    public ParticleSystem Spark;

    [SerializeField] private InputActionReference triggerAction; // Referencia al botón del gatillo
    Vector3 normales = Vector3.zero;
    public bool IsGizmo = false;
    float FrameRate = 0;
    public float Rate;
    public ObjectPoolManager _ObjectPoolManager;


    private NewPart currentPart;
    // Comentado: Se eliminan las referencias de UI (voltageText, speedText, etc.) para simplificar por ahora la conexión
    // pero se pueden reintegrar fácilmente.
    /*
    public Text voltageText;
    public Text speedText;
    public Text timeText;
    public Text resultText;
    public Image panelImage;
    public Sprite defaultImage;  // Imagen por defecto
    public List<NewPartInfo> partInfos; // Lista de cubos con imágenes
    */

    // Comentado: Nuevo campo para la máquina seleccionada
    private MachineData selectedMachine;

    [System.Serializable]
    public class NewPartInfo
    {
        public string partName;
        public Sprite partImage;
    }


    /////
    // Variables de soldadura - Comentado: Se eliminan las variables de soldadura fijas.
    // public float voltage = 22.0f;
    // public float wireSpeed = 385f;
    public string weldingResult = "65% regular"; // Comentado: Se mantiene el resultado, aunque podría depender de la máquina

    public bool IsWelding() => Press;
    // Comentado: Los métodos ahora obtienen los valores de la máquina seleccionada
    public float GetVoltage() => selectedMachine != null ? selectedMachine.defaultVoltage : 0f;
    public float GetWireSpeed() => selectedMachine != null ? selectedMachine.defaultCurrent : 0f; // Comentado: Se usa defaultCurrent como proxy para "wireSpeed" en el contexto AC/DC.
    public string GetWeldingResult() => weldingResult;


    /////

    // Comentado: Agregado Awake y OnDestroy para suscribirse al MachineSelectionManager
    void Awake()
    {
        if (MachineSelectionManager.Instance != null)
        {
            MachineSelectionManager.Instance.OnMachineSelected += OnMachineSelected;
            // Si ya hay una máquina seleccionada al inicio, la asignamos
            OnMachineSelected(MachineSelectionManager.Instance.SelectedMachine);
        }
        else
        {
            Debug.LogError("MachineSelectionManager no está instanciado. Asegúrate de que esté en la escena y se inicialice primero.");
        }
    }

    void OnDestroy()
    {
        if (MachineSelectionManager.Instance != null)
        {
            MachineSelectionManager.Instance.OnMachineSelected -= OnMachineSelected;
        }
    }

    // Comentado: Método para manejar la selección de la máquina
    private void OnMachineSelected(MachineData machine)
    {
        selectedMachine = machine;
        if (selectedMachine != null)
        {
            Debug.Log($"Máquina seleccionada en PistolaSphereCastMerge: {selectedMachine.machineName} ({selectedMachine.machineType})");
            // Aquí podrías actualizar la UI de la pistola si tuvieras los Text asignados
            // if (voltageText != null) voltageText.text = selectedMachine.defaultVoltage.ToString("F1") + "V";
            // if (speedText != null) speedText.text = selectedMachine.defaultCurrent.ToString("F0") + "A"; // Usando Current como proxy
        }
        else
        {
            Debug.LogWarning("Máquina deseleccionada o nula.");
        }
    }


    void FixedUpdate()
    {
        // Comentado: Verificar si se ha seleccionado una máquina antes de simular la soldadura
        if (selectedMachine == null)
        {
            if (Spark.isPlaying) Spark.Stop();
            //Debug.Log("Esperando selección de máquina...");
            return;
        }

        // Detectar si el gatillo del controlador está presionado
        Press = (triggerAction.action.ReadValue<float>() > 0.2f);
        // Debug.Log("Press: " + Press); // Depuración del gatillo

        if (Press)
        {
            normales = Vector3.zero;
            RaycastHit[] hits;
            Vector3 direction = pivot.forward;

            hits = Physics.SphereCastAll(pivot.position, sphereRadius, direction, maxDistance, layerMask);
            detectedObjects.Clear();

            foreach (RaycastHit hit in hits)
            {
                if (!detectedObjects.Contains(hit.collider.gameObject))
                {
                    detectedObjects.Add(hit.collider.gameObject);
                }
                normales += hit.normal;
            }

            if (detectedObjects.Count == 2)
            {
                MergeObjects(detectedObjects[0], detectedObjects[1]);
            }

            if (detectedObjects.Count > 0)
            {
                RaycastHit hit = hits[0]; // Toma el primer impacto
                Quaternion rotation = Quaternion.LookRotation(normales, Vector3.up);

                // Comentado: Aquí podrías usar GetArcIntensity del behavior
                // float arcIntensity = selectedMachine.behavior.GetArcIntensity(hit.distance, GetWireSpeed(), GetVoltage());
                // Debug.Log($"Intensidad del Arco: {arcIntensity}");

                Spark.transform.position = hit.point;
                Spark.transform.rotation = rotation;

                if (FrameRate > Rate)
                {
                    // Comentado: Asegurar que el objeto padre sea el primer objeto detectado o el objeto de soldadura
                    _ObjectPoolManager?.GetObject(hit.point, Quaternion.identity, detectedObjects[0].transform);
                    FrameRate = 0;
                }
                FrameRate += Time.deltaTime;

                // Debug.Log("Activando partículas en: " + Spark.transform.position);
                if (!Spark.isPlaying)
                {
                    Spark.Play();
                }
            }
            else
            {
                // Si el gatillo está presionado pero no hay colisión, detener partículas
                if (Spark.isPlaying)
                {
                    Spark.Stop();
                }
            }
        }
        else
        {
            if (Spark.isPlaying)
            {
                Spark.Stop();
                // Debug.Log("Deteniendo partículas");
            }
        }
    }

    void MergeObjects(GameObject obj1, GameObject obj2)
    {
        NewPart part1 = obj1.GetComponent<NewPart>();
        NewPart part2 = obj2.GetComponent<NewPart>();
        if (part1 == null || part2 == null) return;


        if (part1.weight > part2.weight)
        {
            part1.AbsorbPiece(part2);
        }
        else
        {
            //Debug.Log("part2: "+part2.name + " AbsorbPiece-> part1: " + part1.name);
            part2.AbsorbPiece(part1);
        }

    }

    private void OnDrawGizmos()
    {
        if (!IsGizmo) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pivot.position, pivot.position + pivot.forward * maxDistance);
        Gizmos.DrawWireSphere(pivot.position + pivot.forward * maxDistance, sphereRadius);

        normales = Vector3.zero;

        RaycastHit[] hits;
        Vector3 direction = pivot.forward; // Dirección del SphereCast

        hits = Physics.SphereCastAll(pivot.position, sphereRadius, direction, maxDistance, layerMask);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Metal")) // Asegurar que sean cubos
            {
                normales += hit.normal;
            }
        }

        if (hits.Length > 0) // Si detecta al menos un objeto
        {
            // Calcular la rotación para que Spark mire en la dirección de la normal
            Quaternion rotation = Quaternion.LookRotation(normales);

            // Ajustar la posición y rotación de las partículas
            Spark.transform.position = hits[0].point; // Coloca las partículas en el punto de impacto
            Spark.transform.rotation = rotation;      // Orienta las partículas según la normal

        }
        if (hits.Length > 0)
        {
            Gizmos.DrawLine(hits[0].point, hits[0].point + normales * 12);
        }
    }
}