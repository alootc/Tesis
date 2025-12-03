using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FeedbackReportManager : MonoBehaviour
{
    public static FeedbackReportManager Instance { get; private set; }

    [Header("Configuración de UI")]
    [Tooltip("Panel UI que contiene el reporte final. Debe estar desactivado por defecto.")]
    public GameObject reportPanel;
    public TextMeshProUGUI percentageText;
    public TextMeshProUGUI summaryText;
    public TextMeshProUGUI detailsText;
    public GameObject continueButton; // El nuevo botón para continuar/resetear

    [Header("Pesos de Puntuación (Suma 100%)")]
    [Tooltip("Peso de la sección de Equipamiento de Protección Personal (EPI).")]
    [Range(0f, 100f)] public float epiWeight = 40f;
    [Tooltip("Peso de la sección de Procedimiento Secuencial (Fase 2).")]
    [Range(0f, 100f)] public float sequenceWeight = 40f;
    [Tooltip("Peso de la sección de Errores Críticos durante la Soldadura (ej. casco claro, contacto).")]
    [Range(0f, 100f)] public float criticalWeldErrorWeight = 20f;

    // Lista interna para registrar las fallas que afectarán el porcentaje
    private List<string> recordedFailures = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        reportPanel.SetActive(false); // Asegurar que inicie oculto
    }

    // Método a llamar desde un botón "Finalizar" de la UI principal
    public void GenerateAndShowReport()
    {
        Debug.Log("[REPORTE] Generando informe final...");

        // 1. Obtener y analizar los datos de seguridad
        float epiScore = CalculateEPIScore();
        float sequenceScore = CalculateSequenceScore();

        // 2. Calcular la puntuación total
        // NOTA: El sistema de errores críticos de soldadura se registraría
        // continuamente durante la simulación (aún no implementado). 
        // Por ahora, asumimos 100% en errores críticos si no se ha registrado nada.
        float criticalErrorScore = 100f; // Asumimos que no hubo fallos críticos de soldadura

        float finalScore =
            (epiScore * (epiWeight / 100f)) +
            (sequenceScore * (sequenceWeight / 100f)) +
            (criticalErrorScore * (criticalWeldErrorWeight / 100f));

        // 3. Generar el texto del reporte
        string summary = GetSummaryText(finalScore);
        string details = GetDetailedReport(epiScore, sequenceScore);

        // 4. Actualizar la UI
        percentageText.text = $"{Mathf.RoundToInt(finalScore)}%";
        summaryText.text = summary;
        detailsText.text = details;

        // 5. Mostrar el panel
        reportPanel.SetActive(true);
        Time.timeScale = 0f; // Pausar el tiempo de la simulación mientras se ve el reporte

        // Asignar listener al botón (Ejemplo: Volver al menú o resetear)
        continueButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        continueButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ResetSimulation);
    }

    /// <summary>
    /// Calcula la puntuación del EPI (Fase 1 de orden libre).
    /// </summary>
    private float CalculateEPIScore()
    {
        if (SafetyTutorialManager.Instance == null || SafetyTutorialManager.Instance.checklistData == null) return 0f;

        var freeItems = SafetyTutorialManager.Instance.checklistData.freeOrderItems;
        if (freeItems.Count == 0) return 100f;

        // Contar cuántos EPI críticos fueron recolectados correctamente
        int collectedCount = freeItems.Count(item => SafetyTutorialManager.Instance.IsItemCollected(item));

        float score = (float)collectedCount / freeItems.Count * 100f;

        if (score < 100f)
        {
            var missingItems = freeItems.Where(item => !SafetyTutorialManager.Instance.IsItemCollected(item));
            foreach (var item in missingItems)
            {
                recordedFailures.Add($"EPI Faltante: {item}");
            }
        }
        return score;
    }

    /// <summary>
    /// Calcula la puntuación de la Secuencia (Fase 2 de orden secuencial).
    /// Asume que si la fase 2 se completó, la secuencia fue perfecta. 
    /// En una versión más compleja, se registrarían errores de secuencia en el Manager.
    /// </summary>
    private float CalculateSequenceScore()
    {
        // En una implementación real, SafetyTutorialManager necesitaría 
        // registrar errores de orden y si se completó.
        if (SafetyTutorialManager.Instance == null) return 0f;

        // Si la fase 2 terminó (el Manager está en OFF o fue a la siguiente fase sin errores)
        // Por simplicidad, asumimos que si no está en Sequential, la completó con éxito.
        if (SafetyTutorialManager.Instance.currentPhase == SafetyTutorialManager.TutorialPhase.Sequential)
        {
            recordedFailures.Add("Secuencia: FASE 2 NO completada.");
            return 0f; // Falla total si la secuencia no se terminó.
        }

        // Aquí podrías agregar lógica más granular si el SafetyTutorialManager registrara
        // "pasos perdidos" o "pasos en orden incorrecto".
        return 100f; // Si no está en secuencial, asumimos éxito (o se manejó en EPI).
    }

    private string GetSummaryText(float finalScore)
    {
        if (finalScore >= 90) return "¡Éxito Excepcional! Dominaste la seguridad y el procedimiento.";
        if (finalScore >= 70) return "Buen Desempeño. Cumpliste con lo básico, pero revisa los detalles.";
        if (finalScore >= 50) return "Necesitas Repasar. Hay fallos graves en seguridad o procedimiento.";
        return "Fallo Crítico. La simulación terminó con fallos de seguridad mayores.";
    }

    private string GetDetailedReport(float epiScore, float sequenceScore)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("--- RESUMEN DETALLADO ---");
        sb.AppendLine($"Puntuación de EPI (Colección): {epiScore:F0}%");
        sb.AppendLine($"Puntuación de Secuencia: {sequenceScore:F0}%");
        sb.AppendLine("--------------------------");

        if (recordedFailures.Any())
        {
            sb.AppendLine("\n**ERRORES Y ADVERTENCIAS:**");
            foreach (var failure in recordedFailures)
            {
                sb.AppendLine($"- {failure}");
            }
        }
        else
        {
            sb.AppendLine("\n**¡Excelente! No se registraron fallos de seguridad.**");
        }

        // Limpiar para la próxima simulación
        recordedFailures.Clear();

        return sb.ToString();
    }

    // Método para reanudar o resetear la simulación
    public void ResetSimulation()
    {
        // Reanudar el tiempo
        Time.timeScale = 1f;
        reportPanel.SetActive(false);

        // TODO: Implementar aquí la lógica para:
        // 1. Cargar la escena de inicio (si aplica)
        // 2. O, Resetear el estado de todos los objetos de la escena actual
        Debug.Log("[REPORTE] Simulación reseteada. Implementar recarga de escena aquí.");
        // Ejemplo de recarga de escena: SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}