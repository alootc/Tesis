using UnityEngine;
// ScriptableObject para definir los mensajes y el tipo de feedback
[CreateAssetMenu(menuName = "Feedback/Feedback Message")]
public class FeedbackData : ScriptableObject
{
    [Header("Identificación")]
    public string id = "NO_GLOVES";

    [Tooltip("Tipo de feedback: Advertencia (Warning) o Error (Error).")]
    public FeedbackType type = FeedbackType.Warning;

    [Header("Contenido del Mensaje")]
    [Tooltip("Texto principal que verá el jugador.")]
    [TextArea] public string messageText_ES = "¡Cuidado! No tienes puesto/recogido el elemento de seguridad crítico. Peligro de quemaduras.";

    [Tooltip("Imagen que acompaña al mensaje (Ej: signo de peligro, mano quemada).")]
    public Sprite displayImage;
}
public enum FeedbackType { Warning, Error }
