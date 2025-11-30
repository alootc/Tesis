using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

// Este script se adjunta a la pistola de soldar para verificar la seguridad antes de permitir el arco.
public class WeldingSafetyGuard : MonoBehaviour
{
    [Header("Configuración de Seguridad")]
    [Tooltip("Lista de FeedbackData para los ítems de seguridad (Ej: NO_CARETA, NO_GUANTES).")]
    public List<FeedbackData> safetyViolationFeedback;

    [Tooltip("IDs de los ítems de seguridad CRÍTICOS (Ej: Careta, Guantes).")]
    public List<string> criticalItems;

    private XRGrabInteractable grabbable;

    void Start()
    {
        grabbable = GetComponent<XRGrabInteractable>();
        if (grabbable != null)
        {
            // Reemplazamos la llamada directa a StartWelding por una verificación segura
            // Asegurarse de que WeldingGunOnActivate esté presente antes de intentar remover el Listener.
            var weldingGun = GetComponent<WeldingGunOnActivate>();
            if (weldingGun != null)
            {
                grabbable.activated.RemoveListener(weldingGun.StartWelding); // Remover la conexión directa
                grabbable.activated.AddListener(AttemptStartWelding);
            }
            else
            {
                Debug.LogError("[SAFETY GUARD] No se encontró WeldingGunOnActivate en el mismo GameObject. El sistema de seguridad no funcionará correctamente.");
            }
        }
    }

    public void AttemptStartWelding(ActivateEventArgs arg)
    {
        bool canWeld = true;
        string missingItemId = null;

        Debug.Log("[SAFETY GUARD] Intento de soldadura detectado. Verificando EPI...");

        // 1. Verificar ítems de seguridad CRÍTICOS
        if (SafetyTutorialManager.Instance != null)
        {
            foreach (var itemID in criticalItems)
            {
                if (!SafetyTutorialManager.Instance.HasItem(itemID))
                {
                    canWeld = false;
                    missingItemId = itemID; // Guarda el primer error encontrado
                    break;
                }
            }
        }
        else
        {
            Debug.LogError("[SAFETY GUARD] SafetyTutorialManager.Instance es NULL. La verificación de seguridad no se puede realizar.");
            canWeld = true; // Permite soldar si el manager no está inicializado (puede ser un error de inicialización)
        }


        if (canWeld)
        {
            Debug.Log("[SAFETY GUARD] Verificación de EPI exitosa. Iniciando soldadura.");
            // 2. Si es seguro, permitir que la pistola inicie el proceso de soldadura real
            GetComponent<WeldingGunOnActivate>()?.StartWelding(arg);
        }
        else
        {
            Debug.Log($"[SAFETY GUARD] Violación de seguridad detectada: Falta '{missingItemId}'. Bloqueando soldadura.");
            // 3. Si no es seguro, mostrar feedback de violación
            ShowViolationFeedback(missingItemId);
            // Prevenir el inicio del arco
        }
    }

    private void ShowViolationFeedback(string missingItemId)
    {
        if (FeedbackManager.Instance == null)
        {
            Debug.LogError("[SAFETY GUARD] FeedbackManager.Instance es NULL. No se puede mostrar el mensaje de violación.");
            return;
        }

        // Buscar el mensaje de feedback correspondiente
        FeedbackData violation = safetyViolationFeedback.Find(d => d.id == $"NO_{missingItemId.ToUpper()}");

        if (violation != null)
        {
            FeedbackManager.Instance.ShowFeedback(violation);
        }
        else
        {
            // Feedback genérico si no se encuentra uno específico
            FeedbackData genericError = ScriptableObject.CreateInstance<FeedbackData>();
            genericError.type = FeedbackType.Error;
            genericError.messageText_ES = $"ERROR CRÍTICO: Falta el equipo de seguridad: {missingItemId}. (ID de Feedback no encontrado)";
            FeedbackManager.Instance.ShowFeedback(genericError);
            Destroy(genericError); // Destruir la instancia temporal
        }
    }
}