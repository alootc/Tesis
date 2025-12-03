using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
[DefaultExecutionOrder(-100)]
public class WeldingSafetyGuard : MonoBehaviour
{
    [Header("Configuración de Seguridad")]
    [Tooltip("Lista de FeedbackData para los ítems de seguridad (Ej: NO_CARETA, NO_GUANTES, NO_CASCO_OSCURO).")]
    public List<FeedbackData> safetyViolationFeedback;

    [Tooltip("IDs de los ítems de seguridad CRÍTICOS (Ej: Careta, Guantes).")]
    public List<string> criticalItems;

    [Header("Componentes Específicos")]
    // **NUEVO CAMPO:** Referencia al script de la Careta/Casco de Soldadura. ¡Debes asignarlo en el Inspector!
    [Tooltip("Referencia al componente que maneja el oscurecimiento del casco de soldar.")]
    public WeldingHelmet weldingHelmet;

    private XRGrabInteractable grabbable;
    private WeldingGunOnActivate weldingGun;
    private PistolaSphereCastMerge sphereCastMerge;

    void Start()
    {
        grabbable = GetComponent<XRGrabInteractable>();
        weldingGun = GetComponent<WeldingGunOnActivate>();
        sphereCastMerge = GetComponent<PistolaSphereCastMerge>();

        if (grabbable != null && weldingGun != null)
        {
            // --- BLOQUE CRÍTICO DE INTERCEPTACIÓN ---

            // 1. Intentamos remover el listener original de la pistola. 
            for (int i = 0; i < 5; i++)
            {
                grabbable.activated.RemoveListener(weldingGun.StartWelding);
            }

            // 2. Conectamos el gatillo SOLO a nuestro método de verificación.
            grabbable.activated.AddListener(AttemptStartWelding);

            Debug.Log("[SAFETY GUARD] Inicialización OK. Conexión de activación interceptada exitosamente.");

            // INICIALIZACIÓN CRÍTICA: Bloquear la soldadura por defecto hasta que se compruebe la seguridad.
            if (sphereCastMerge != null)
            {
                sphereCastMerge.IsWeldingAllowed = false;
            }
        }
        else
        {
            Debug.LogError("[SAFETY GUARD ERROR] Faltan componentes (WeldingGunOnActivate o XRGrabInteractable). El sistema de seguridad NO funcionará correctamente.");
        }
    }

    // Método que WeldingGunOnActivate debería llamar para detener la soldadura (si se implementara la detención)
    public void StopWelding(DeactivateEventArgs arg)
    {
        // Al detener la soldadura, siempre la desactivamos en la pistola
        if (sphereCastMerge != null)
        {
            sphereCastMerge.IsWeldingAllowed = false;
        }
        weldingGun?.StopWelding(arg);
    }

    private bool IsMetalReadyForWeld()
    {
        if (sphereCastMerge == null)
        {
            Debug.LogWarning("[SAFETY GUARD] PistolaSphereCastMerge es NULL. Asumiendo contacto con metal OK.");
            return true;
        }
        return sphereCastMerge.IsReadyToWeld();
    }


    public void AttemptStartWelding(ActivateEventArgs arg)
    {
        bool canWeldEPI = true;
        string missingItemId = null;

        Debug.Log("=================================================");
        Debug.Log("[SAFETY GUARD] Intento de soldadura detectado. Verificando EPI...");

        // 1. Verificar ítems de seguridad CRÍTICOS (EPI)
        if (SafetyTutorialManager.Instance != null)
        {
            foreach (var itemID in criticalItems)
            {
                bool hasItem = SafetyTutorialManager.Instance.HasItem(itemID);
                Debug.Log($"[SAFETY CHECK] Buscando ítem: '{itemID}' | ¿Encontrado?: {hasItem}");

                if (!hasItem)
                {
                    canWeldEPI = false;
                    missingItemId = itemID;
                    break;
                }
            }
        }
        else
        {
            Debug.LogError("[SAFETY GUARD ERROR] SafetyTutorialManager.Instance es NULL. PERMITIENDO SOLDADURA POR DEFECTO (RIESGO).");
            canWeldEPI = true;
        }

        // 1b. **NUEVA VERIFICACIÓN CRÍTICA:** Si la careta es un ítem crítico Y está presente, verificar que esté oscura.
        if (canWeldEPI && weldingHelmet != null && criticalItems.Contains("Careta"))
        {
            // Verificamos si la careta está en la lista de ítems ya recolectados.
            bool helmetCollected = SafetyTutorialManager.Instance?.HasItem("Careta") ?? true;

            if (helmetCollected && !weldingHelmet.IsDarkened)
            {
                canWeldEPI = false;
                missingItemId = "NO_CASCO_OSCURO"; // ID de feedback específico
                Debug.LogWarning("[SAFETY CHECK] Careta puesta, pero el panel NO está oscuro.");
            }
        }


        if (canWeldEPI)
        {
            // 2. EPI OK: Verificar el contacto con el metal
            if (IsMetalReadyForWeld())
            {
                // TODO OK: Activamos la soldadura y llamamos al método original de la pistola.
                if (sphereCastMerge != null)
                {
                    sphereCastMerge.IsWeldingAllowed = true;
                }

                Debug.Log("[SAFETY GUARD ÉXITO] EPI y Contacto con Metal OK. Iniciando soldadura.");
                weldingGun?.StartWelding(arg);
            }
            else
            {
                // Contacto fallido: BLOQUEO.
                if (sphereCastMerge != null) sphereCastMerge.IsWeldingAllowed = false;

                Debug.Log("[SAFETY GUARD VIOLACIÓN] BLOQUEANDO SOLDADURA. No hay contacto válido con el metal (SphereCastMerge).");
                ShowViolationFeedback("NO_CONTACTO");
            }
        }
        else
        {
            // 3. EPI Faltante/Casco Claro: Bloqueo de seguridad definitivo.
            if (sphereCastMerge != null)
            {
                // Bloqueamos la lógica de FixedUpdate de la pistola. ESTE ES EL BLOQUEO CRÍTICO.
                sphereCastMerge.IsWeldingAllowed = false;
            }

            Debug.Log($"[SAFETY GUARD VIOLACIÓN] BLOQUEANDO SOLDADURA. Falta ítem crítico/condición: '{missingItemId}'.");
            ShowViolationFeedback(missingItemId);
        }
        Debug.Log("=================================================");
    }

    private void ShowViolationFeedback(string missingItemIdOrCode)
    {
        if (FeedbackManager.Instance == null)
        {
            Debug.LogError("[SAFETY GUARD ERROR] FeedbackManager.Instance es NULL.");
            return;
        }

        // Aseguramos que el ID de feedback tenga el prefijo NO_
        string feedbackId = missingItemIdOrCode.StartsWith("NO_") ? missingItemIdOrCode : $"NO_{missingItemIdOrCode.ToUpper()}";

        Debug.Log($"[SAFETY GUARD] Buscando Feedback con ID: {feedbackId}");

        FeedbackData violation = safetyViolationFeedback.Find(d => d.id == feedbackId);

        if (violation != null)
        {
            FeedbackManager.Instance.ShowFeedback(violation);
        }
        else
        {
            Debug.LogError($"[SAFETY GUARD ERROR] No se encontró el FeedbackData con ID: '{feedbackId}'.");

            FeedbackData genericError = ScriptableObject.CreateInstance<FeedbackData>();
            genericError.type = FeedbackType.Error;
            genericError.messageText_ES = $"ERROR CRÍTICO: Condición de bloqueo fallida: {missingItemIdOrCode}.";
            FeedbackManager.Instance.ShowFeedback(genericError);
            Destroy(genericError);
        }
    }
}