using UnityEngine;
using UnityEngine.InputSystem;

public class WeldingHelmet : MonoBehaviour
{
    [Header("Configuración de Entrada")]
    [Tooltip("Referencia a la acción de entrada (Input Action) que activará/desactivará el oscurecimiento (ej: Botón A de Oculus).")]
    public InputActionReference toggleDarkeningAction;

    [Header("Estado del Casco")]
    [Tooltip("Indica si el panel de la careta está activo (oscurecido) para proteger al usuario.")]
    [SerializeField]
    private bool isDarkened = false;

    // Propiedad pública de solo lectura que el WeldingSafetyGuard verificará.
    public bool IsDarkened => isDarkened;

    private void OnEnable()
    {
        // Suscribimos la acción al evento de presión (presión simple)
        if (toggleDarkeningAction != null && toggleDarkeningAction.action != null)
        {
            toggleDarkeningAction.action.performed += OnDarkeningToggle;
            toggleDarkeningAction.action.Enable();
            Debug.Log("[HELMET] Suscripción a acción de oscurecimiento OK.");
        }
    }

    private void OnDisable()
    {
        if (toggleDarkeningAction != null && toggleDarkeningAction.action != null)
        {
            toggleDarkeningAction.action.performed -= OnDarkeningToggle;
            toggleDarkeningAction.action.Disable();
        }
    }

    /// <summary>
    /// Maneja el evento de la acción de entrada para cambiar el estado de oscurecimiento.
    /// </summary>
    private void OnDarkeningToggle(InputAction.CallbackContext context)
    {
        // Solo alternar si la careta está actualmente activa o en uso (opcionalmente puedes verificar si está equipada)
        // Por simplicidad, asumimos que siempre funciona si la acción es disparada.
        ActivateDarkening(!isDarkened);
    }

    /// <summary>
    /// Método para activar/desactivar el oscurecimiento de la careta.
    /// Llama a este método para cambiar el estado.
    /// </summary>
    /// <param name="active">True para oscurecer, False para aclarar.</param>
    public void ActivateDarkening(bool active)
    {
        isDarkened = active;
        if (active)
        {
            Debug.Log("[HELMET] Careta oscurecida. ¡Seguridad ocular activada!");
            // TODO: Implementar aquí la lógica para oscurecer visualmente el panel (ej. cambiar material o activar UI oscura)
        }
        else
        {
            Debug.Log("[HELMET] Careta aclarada.");
            // TODO: Implementar aquí la lógica para aclarar visualmente el panel
        }
    }
}