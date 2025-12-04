using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // --- NUEVAS ESTRUCTURAS DE DATOS ---
    // Diccionario para contar la frecuencia de cada mensaje de error crítico
    private Dictionary<string, int> criticalErrorCounter = new Dictionary<string, int>();
    // Lista para registrar fallas de EPI/Secuencia (errores no repetitivos)
    private List<string> nonCriticalFailures = new List<string>();

    // Define el número de fallos críticos (cooldown ajustado) para obtener 0% en la sección de errores críticos.
    private const int MAX_CRITICAL_ERRORS_FOR_PENALTY = 5;

    // --- NUEVO ESTADO PARA EL CÁLCULO REAL DE PUNTUACIÓN ---
    // Bandera para rastrear si el EPI obligatorio fue completado. Inicia en 'false'.
    private bool epiCompleted = false;
    // Bandera para rastrear si la secuencia fue completada. Inicia en 'false'.
    private bool sequenceCompleted = false;
    // --- FIN NUEVO ESTADO ---


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        reportPanel.SetActive(false); // Asegurar que inicie oculto
        ResetCounters(); // Asegurarse de que los contadores estén limpios al inicio
    }

    // Método para limpiar todos los contadores antes de una nueva simulación
    private void ResetCounters()
    {
        criticalErrorCounter.Clear();
        nonCriticalFailures.Clear();
        // Resetear los estados de finalización al inicio de una nueva simulación
        epiCompleted = false;
        sequenceCompleted = false;
    }

    // --- NUEVOS MÉTODOS PARA RECIBIR EL ESTADO DE COMPLETADO ---
    /// <summary>
    /// Método a llamar por el SafetyTutorialManager para indicar si la EPI fue recolectada al 100%.
    /// </summary>
    public void SetEPICompletionStatus(bool completed)
    {
        epiCompleted = completed;
        Debug.Log($"[REPORTE] Estado de finalización de EPI establecido a: {completed}");
    }

    /// <summary>
    /// Método a llamar por el SafetyTutorialManager para indicar si la Secuencia fue completada al 100%.
    /// </summary>
    public void SetSequenceCompletionStatus(bool completed)
    {
        sequenceCompleted = completed;
        Debug.Log($"[REPORTE] Estado de finalización de Secuencia establecido a: {completed}");
    }
    // --- FIN NUEVOS MÉTODOS ---


    // Método público para que otras clases registren una falla general (EPI o Secuencia, no repetitiva)
    public void RecordFailure(string failureMessage)
    {
        // Solo registra el mensaje de falla si no está ya presente
        if (!nonCriticalFailures.Contains(failureMessage))
        {
            nonCriticalFailures.Add(failureMessage);
        }
        Debug.Log($"[REPORTE] Falla de Seguridad/Procedimiento registrada: {failureMessage}");
    }

    /// <summary>
    /// Registra y cuenta un error crítico de soldadura (ej. intentar soldar sin casco, sin contacto).
    /// </summary>
    /// <param name="errorMessage">El tipo de error, usado como clave para el conteo.</param>
    public void RecordCriticalWeldError(string errorMessage)
    {
        // Incrementa el contador para este tipo específico de error
        if (criticalErrorCounter.ContainsKey(errorMessage))
        {
            criticalErrorCounter[errorMessage]++;
        }
        else
        {
            criticalErrorCounter.Add(errorMessage, 1);
        }
        // Debug.Log($"[REPORTE] Error Crítico registrado: {errorMessage}. Conteo total: {GetTotalCriticalErrors()}");
    }

    /// <summary>
    /// Devuelve el número total de errores críticos (suma de todos los tipos).
    /// </summary>
    private int GetTotalCriticalErrors()
    {
        return criticalErrorCounter.Values.Sum();
    }


    // Método a llamar desde un botón "Finalizar" de la UI principal
    public void GenerateAndShowReport()
    {
        Debug.Log("[REPORTE] Generando informe final...");

        // --- LOGGING ADICIONAL PARA DIAGNÓSTICO ---
        // Verificar el estado de los contadores justo antes de generar el reporte
        Debug.Log("=============================================");
        Debug.Log($"[REPORTE DIAGNÓSTICO] Verificando datos antes de reportar:");
        Debug.Log($"[REPORTE DIAGNÓSTICO] Estado EPI Completado: {epiCompleted}"); // Diagnóstico de nuevo estado
        Debug.Log($"[REPORTE DIAGNÓSTICO] Estado Secuencia Completada: {sequenceCompleted}"); // Diagnóstico de nuevo estado
        Debug.Log($"[REPORTE DIAGNÓSTICO] Total de Errores Críticos Contados (Suma): {GetTotalCriticalErrors()}");
        Debug.Log($"[REPORTE DIAGNÓSTICO] Tipos de Errores Críticos Únicos: {criticalErrorCounter.Count}");
        Debug.Log($"[REPORTE DIAGNÓSTICO] Advertencias/Fallas No Repetitivas: {nonCriticalFailures.Count}");
        Debug.Log("=============================================");
        // --- FIN LOGGING ADICIONAL ---

        // 1. Obtener y analizar los datos de seguridad
        float epiScore = CalculateEPIScore();
        float sequenceScore = CalculateSequenceScore();
        float criticalErrorScore = CalculateCriticalWeldErrorScore();

        // 2. Calcular la puntuación total
        float finalScore =
            (epiScore * (epiWeight / 100f)) +
            (sequenceScore * (sequenceWeight / 100f)) +
            (criticalErrorScore * (criticalWeldErrorWeight / 100f));

        finalScore = Mathf.Max(0f, finalScore); // Asegurar que la puntuación no baje de 0

        // 3. Generar el texto del reporte
        string summary = GetSummaryText(finalScore);
        string details = GetDetailedReport(epiScore, sequenceScore, criticalErrorScore);

        // 4. Actualizar la UI
        percentageText.text = $"{Mathf.RoundToInt(finalScore)}%";
        summaryText.text = summary;
        detailsText.text = details;

        // 5. Mostrar el panel
        reportPanel.SetActive(true);
        Time.timeScale = 0f; // Pausar el tiempo de la simulación mientras se ve el reporte

        // Asignar listener al botón 
        var buttonComponent = continueButton.GetComponent<UnityEngine.UI.Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(ResetSimulation);
        }
        else
        {
            Debug.LogError("El GameObject 'continueButton' no tiene un componente Button.");
        }
    }

    /// <summary>
    /// Calcula la puntuación de errores críticos de soldadura, basándose en el total de errores contados.
    /// </summary>
    private float CalculateCriticalWeldErrorScore()
    {
        int totalErrors = GetTotalCriticalErrors();
        if (totalErrors == 0) return 100f;

        // Penalización lineal: cada error resta un porcentaje hasta alcanzar 0% en el límite.
        float score = 100f - ((float)totalErrors / MAX_CRITICAL_ERRORS_FOR_PENALTY) * 100f;

        // Aseguramos que la puntuación no sea negativa.
        return Mathf.Max(0f, score);
    }

    /// <summary>
    /// Calcula la puntuación del EPI (Fase 1 de orden libre). (Lógica REAL usando el estado 'epiCompleted')
    /// </summary>
    private float CalculateEPIScore()
    {
        // Si la bandera 'epiCompleted' no fue seteada a true, la puntuación es 0.
        // Esto asume que el EPI es un requisito de todo o nada (100% o 0%).
        return epiCompleted ? 100f : 0f;
    }

    /// <summary>
    /// Calcula la puntuación de la Secuencia (Fase 2 de orden secuencial). (Lógica REAL usando el estado 'sequenceCompleted')
    /// </summary>
    private float CalculateSequenceScore()
    {
        // Si la bandera 'sequenceCompleted' no fue seteada a true, la puntuación es 0.
        // Esto asume que la Secuencia es un requisito de todo o nada (100% o 0%).
        return sequenceCompleted ? 100f : 0f;
    }

    private string GetSummaryText(float finalScore)
    {
        if (finalScore >= 90) return "¡Éxito Excepcional! Dominaste la seguridad y el procedimiento.";
        if (finalScore >= 70) return "Buen Desempeño. Cumpliste con lo básico, pero revisa los detalles.";
        if (finalScore >= 50) return "Necesitas Repasar. Hay fallos graves en seguridad o procedimiento.";
        return "Fallo Crítico. La simulación terminó con fallos de seguridad mayores.";
    }

    private string GetDetailedReport(float epiScore, float sequenceScore, float criticalScore)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        // Las advertencias/fallas no críticas solo se muestran si la fase falló, aunque la lógica del puntaje ya lo maneja.
        int totalWarnings = nonCriticalFailures.Count + GetTotalCriticalErrors();

        sb.AppendLine("--- RESUMEN DETALLADO ---");
        sb.AppendLine($"Puntuación de EPI (Colección): {epiScore:F0}%");
        sb.AppendLine($"Puntuación de Secuencia: {sequenceScore:F0}%");
        sb.AppendLine($"Puntuación de Errores Críticos: {criticalScore:F0}%");
        sb.AppendLine("--------------------------");

        sb.AppendLine($"\n**TOTAL DE ERRORES/ADVERTENCIAS REGISTRADAS: {totalWarnings}**");

        // *** DIAGNÓSTICO PARA ERRORES CRÍTICOS ***
        if (GetTotalCriticalErrors() > 0 && criticalScore == 100)
        {
            sb.AppendLine("\n🚨 **ADVERTENCIA:** Se registraron errores críticos, pero la puntuación de 100% indica que el sistema de penalización podría estar inactivo o los pesos son bajos.");
        }
        // *** FIN DIAGNÓSTICO ***


        if (totalWarnings > 0)
        {
            sb.AppendLine("\n**DETALLE POR TIPO DE FALLA:**");

            // 1. Errores Críticos (Repetitivos)
            if (criticalErrorCounter.Any())
            {
                sb.AppendLine("\n**A) ERRORES CRÍTICOS DE SOLDADURA:**");
                foreach (var pair in criticalErrorCounter)
                {
                    // Formato: - Error Crítico: Soldadura intentada sin contacto (x4)
                    sb.AppendLine($"- {pair.Key.Replace("Error Crítico: ", "")} ({pair.Value} veces)");
                }
            }

            // 2. Fallas No Críticas (No Repetitivas)
            if (nonCriticalFailures.Any())
            {
                // Se asume que estos son los mensajes que explican por qué EPI o Secuencia falló.
                sb.AppendLine("\n**B) ADVERTENCIAS DE SEGURIDAD/PROCEDIMIENTO (Detalle de Fallo):**");
                foreach (var failure in nonCriticalFailures)
                {
                    sb.AppendLine($"- {failure}");
                }
            }
        }
        else
        {
            sb.AppendLine("\n**¡Excelente! No se registraron fallos de seguridad o procedimiento.**");
        }

        // Mensaje de diagnóstico si el EPI falló pero no se registraron fallas explícitas
        if (epiScore == 0)
        {
            sb.AppendLine("\n⚠️ **NOTA DE FALLA DE EPI:** La fase de EPI no fue completada (0%). Asegúrate de haber recogido **TODOS** los ítems de la Fase 1 antes de finalizar la simulación.");
        }


        return sb.ToString();
    }

    // Método para reanudar o resetear la simulación
    public void ResetSimulation()
    {
        Time.timeScale = 1f;
        reportPanel.SetActive(false);
        ResetCounters(); // Resetear contadores y fallos registrados

        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("[REPORTE] Simulación reseteada y escena recargada.");
    }
}