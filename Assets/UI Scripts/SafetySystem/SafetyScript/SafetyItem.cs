using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
// Este script se adjunta a los objetos físicos (guantes, casco, etc.)
[RequireComponent(typeof(XRGrabInteractable))]
public class SafetyItem : MonoBehaviour
{
    [Tooltip("ID único que debe coincidir con el SafetyChecklistData (Ej: Guantes, Careta).")]
    public string itemID;

    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Conectar el evento de agarre al tutorial para marcar el ítem como "recogido" o "usado".
        grabInteractable.selectEntered.AddListener(OnItemGrabbed);

        // Opcional: Si el ítem es un "wearable" (como la careta), puedes usar el evento de adjuntar a un socket.
        // grabInteractable.selectExited.AddListener(OnItemDropped);
    }

    private void OnItemGrabbed(SelectEnterEventArgs args)
    {
        // Notificar al Manager que este elemento ha sido agarrado/equipado.
        if (SafetyTutorialManager.Instance != null)
        {
            SafetyTutorialManager.Instance.MarkItemCollected(itemID);
        }
        else
        {
            Debug.LogWarning($"SafetyTutorialManager no está disponible. No se pudo registrar la recolección de: {itemID}");
        }
    }

    // Puedes añadir lógica para OnItemEquipped (si se usa un socket) aquí
}
