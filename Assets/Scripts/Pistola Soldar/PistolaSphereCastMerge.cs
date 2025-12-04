using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Necesario para Text e Image
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Necesario para XRGrabInteractable

public class PistolaSphereCastMerge : MonoBehaviour
{
    [Header("Configuración de Detección")]
    public float sphereRadius = 1.0f; // Radio del SphereCast
    public float maxDistance = 5.0f;  // Distancia máxima del SphereCast
    public LayerMask layerMask; // Define qué capas detectar
    public Transform pivot; // Punto de origen del SphereCast

    [Header("Componentes de Soldadura")]
    public ParticleSystem Spark;
    public ObjectPoolManager _ObjectPoolManager;
    [SerializeField] private InputActionReference triggerAction; // Referencia al botón del gatillo

    // --- Nuevas Variables para Ajuste Visual (Usa sparkRightOffset para corregir el desplazamiento) ---
    [Header("Ajuste Visual del Spark")]
    [Tooltip("Levanta ligeramente la chispa de la superficie, a lo largo de la normal de impacto. Útil para evitar que la chispa se corte con el metal.")]
    public float sparkNormalOffset = 0.01f;
    [Tooltip("Mueve la chispa lateralmente (derecha/izquierda) en relación con el pivote de la pistola. Usa un valor negativo para moverla a la izquierda.")]
    public float sparkRightOffset = 0.0f;   // Corregido a 0.0f por defecto
    [Tooltip("Mueve la chispa hacia adelante (positivo) o hacia atrás (negativo) a lo largo del eje de la pistola, partiendo del punto de impacto.")]
    public float sparkForwardOffset = 0.0f;
    // --- Fin Ajuste Visual ---

    [Header("Debugging")]
    public bool IsGizmo = false;
    public float Rate; // Tasa de generación de puntos de soldadura

    // --- VARIABLES DE CONTROL DE ERRORES CRÍTICOS ---
    private const float ERROR_COOLDOWN = 1.5f; // Tiempo de espera para registrar el mismo error (en segundos)
    private float _errorTimer = 0f;
    private string _lastErrorMessage = "";
    // --- FIN VARIABLES DE CONTROL DE ERRORES CRÍTICOS ---


    private List<GameObject> detectedObjects = new List<GameObject>();
    private MachineData selectedMachine;
    private bool _weldingAllowed = false; // Flag de seguridad externa (proporcionada por WeldingSafetyGuard)

    private bool Press;
    private Vector3 normales = Vector3.zero;
    private float FrameRate = 0;

    // Bandera crítica para el SafetyGuard
    private bool isTouchingMetal = false;

    [System.Serializable]
    public class NewPartInfo
    {
        public string partName;
        public Sprite partImage;
    }

    // Propiedad que el WeldingSafetyGuard utiliza para saber si hay metal cerca.
    public bool IsWeldingAllowed
    {
        get => _weldingAllowed;
        set
        {
            _weldingAllowed = value;
            // Si se desactiva, detenemos inmediatamente las chispas para asegurar el bloqueo visual.
            if (!_weldingAllowed)
            {
                if (Spark != null && Spark.isPlaying) Spark.Stop();
            }
            Debug.Log($"[PISTOLA] El estado de 'IsWeldingAllowed' ha sido establecido a: {_weldingAllowed}");
        }
    }

    // Propiedad para obtener el estado del gatillo
    public bool IsWelding() => Press;

    // Método que el SafetyGuard llama para verificar si hay contacto con el metal
    public bool IsReadyToWeld()
    {
        // Devolvemos el estado de contacto actualizado constantemente en FixedUpdate
        return isTouchingMetal;
    }

    // --- Métodos de la Máquina ---
    public float GetVoltage() => selectedMachine != null ? selectedMachine.defaultVoltage : 0f;
    public float GetWireSpeed() => selectedMachine != null ? selectedMachine.defaultCurrent : 0f;
    public string GetWeldingResult() => "65% regular";

    // --- Lógica de Inicialización ---

    void Awake()
    {
        // Suscribirse a la selección de máquina
        if (MachineSelectionManager.Instance != null)
        {
            MachineSelectionManager.Instance.OnMachineSelected += OnMachineSelected;
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

    private void OnMachineSelected(MachineData machine)
    {
        selectedMachine = machine;
        if (selectedMachine != null)
        {
            Debug.Log($"Máquina seleccionada en PistolaSphereCastMerge: {selectedMachine.machineName} ({selectedMachine.machineType})");
        }
        else
        {
            Debug.LogWarning("Máquina deseleccionada o nula.");
        }
    }

    // --- Bucle de Actualización Física ---

    void FixedUpdate()
    {
        // Actualizar el temporizador de error
        if (_errorTimer > 0)
        {
            _errorTimer -= Time.deltaTime;
        }

        // --- 1. DETECCIÓN DE CONTACTO ---
        RaycastHit hitInfo;
        Vector3 direction = pivot.forward;

        if (Physics.SphereCast(pivot.position, sphereRadius, direction, out hitInfo, maxDistance, layerMask))
        {
            isTouchingMetal = true;
        }
        else
        {
            isTouchingMetal = false;
        }

        // --- 2. VERIFICACIÓN DE ESTADO ---
        if (selectedMachine == null)
        {
            if (Spark.isPlaying) Spark.Stop();
            return;
        }

        Press = (triggerAction.action.ReadValue<float>() > 0.2f);

        // --- 3. LÓGICA DE SOLDADURA FINAL ---
        if (IsWeldingAllowed && Press && isTouchingMetal)
        {
            // Soldadura exitosa
            // Resetear el último error registrado para permitir que un nuevo error se registre inmediatamente después.
            _lastErrorMessage = "";

            normales = Vector3.zero;
            RaycastHit[] hits = Physics.SphereCastAll(pivot.position, sphereRadius, direction, maxDistance, layerMask);
            detectedObjects.Clear();

            // Debug.Log($"[WELD] SphereCastAll detectó {hits.Length} objetos."); 

            foreach (RaycastHit hit in hits)
            {
                if (!detectedObjects.Contains(hit.collider.gameObject))
                {
                    detectedObjects.Add(hit.collider.gameObject);
                }
                normales += hit.normal;
            }

            // Lógica para MERGE
            if (detectedObjects.Count >= 2)
            {
                MergeObjects(detectedObjects[0], detectedObjects[1]);
            }

            // Lógica de Partículas y Spawning
            if (detectedObjects.Count > 0)
            {
                RaycastHit hit = hits[0];
                // Orientar la rotación del Spark para que mire hacia la normal promedio
                Quaternion rotation = Quaternion.LookRotation(normales.normalized, Vector3.up);

                // --- APLICACIÓN DE OFFSET DE POSICIÓN ---
                Vector3 sparkPosition = hit.point;

                // 1. Offset Normal (Levantar ligeramente de la superficie)
                sparkPosition += hit.normal * sparkNormalOffset;

                // 2. Offset Lateral (Corregir el desplazamiento 'a la derecha' o 'izquierda')
                // Usa pivot.right para la corrección lateral. Si la chispa está a la derecha, usa un valor negativo.
                sparkPosition += pivot.right * sparkRightOffset;

                // 3. Offset Longitudinal (Corregir el desplazamiento 'adelante' o 'atrás' a lo largo del eje de la pistola)
                sparkPosition += pivot.forward * sparkForwardOffset;
                // ----------------------------------------

                Spark.transform.position = sparkPosition;
                Spark.transform.rotation = rotation;

                // Spawning del cordón de soldadura
                if (FrameRate > Rate)
                {
                    // Nota: Mantenemos el cordón en hit.point para que se pegue a la superficie (sin offsets)
                    _ObjectPoolManager?.GetObject(hit.point, Quaternion.identity, detectedObjects[0].transform);
                    FrameRate = 0;
                }
                FrameRate += Time.deltaTime;

                // Iniciar partículas
                if (!Spark.isPlaying)
                {
                    Spark.Play();
                }
            }
            else
            {
                if (Spark.isPlaying) Spark.Stop();
            }
        }
        else // Bloqueado por seguridad
        {
            if (Spark != null && Spark.isPlaying)
            {
                Spark.Stop();
            }

            // --- LÓGICA DE REGISTRO DE ERRORES CRÍTICOS (Advertencias) ---
            if (Press) // Solo si el gatillo está activado, registramos el error
            {
                string currentErrorMessage = string.Empty;

                if (!IsWeldingAllowed)
                {
                    // Error de EPI (SafetyGuard)
                    currentErrorMessage = "Error Crítico: Soldadura intentada sin equipo de seguridad (EPI) completo o en zona no autorizada.";
                }
                else if (IsWeldingAllowed && !isTouchingMetal)
                {
                    // Error de Contacto (Pistola)
                    currentErrorMessage = "Error Crítico: Soldadura intentada sin contacto o muy lejos de la pieza de metal.";
                }

                // Solo si encontramos un nuevo error Y el temporizador ha expirado, registramos.
                if (!string.IsNullOrEmpty(currentErrorMessage))
                {
                    // Comprobar si es un error nuevo o si el cooldown ha terminado
                    if (currentErrorMessage != _lastErrorMessage || _errorTimer <= 0f)
                    {
                        if (FeedbackReportManager.Instance != null)
                        {
                            // Registra el error en el manager para la penalización en la puntuación
                            FeedbackReportManager.Instance.RecordCriticalWeldError(currentErrorMessage);
                        }

                        // Restablecer temporizador y guardar el mensaje del último error
                        _errorTimer = ERROR_COOLDOWN;
                        _lastErrorMessage = currentErrorMessage;
                    }
                }
                // Si Press es True, pero no hay error, significa que está en un estado intermedio seguro (raro)
                // En este caso, no registramos nada.
            }
            else // Si el gatillo NO está presionado, no hay necesidad de registrar errores, y reseteamos el error temporal.
            {
                _lastErrorMessage = "";
            }
            // --- FIN LÓGICA DE REGISTRO DE ERRORES CRÍTICOS ---
        }
    }

    // --- Métodos de Utilidad ---

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
            part2.AbsorbPiece(part1);
        }
    }

    // --- Gizmos para Visualización en el Editor ---

    private void OnDrawGizmos()
    {
        if (!IsGizmo) return;
        Gizmos.color = Color.red;

        if (pivot != null)
        {
            Gizmos.DrawLine(pivot.position, pivot.position + pivot.forward * maxDistance);
            Gizmos.DrawWireSphere(pivot.position + pivot.forward * maxDistance, sphereRadius);

            Vector3 vizNormales = Vector3.zero;
            RaycastHit[] hits;
            Vector3 direction = pivot.forward;

            hits = Physics.SphereCastAll(pivot.position, sphereRadius, direction, maxDistance, layerMask);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Metal"))
                {
                    vizNormales += hit.normal;
                }
            }

            if (hits.Length > 0)
            {
                // Dibuja la normal promedio
                Gizmos.DrawLine(hits[0].point, hits[0].point + vizNormales.normalized * 0.5f);

                // Dibuja el punto donde se colocaría la chispa sin offsets
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(hits[0].point, 0.05f);
            }
        }
    }
}