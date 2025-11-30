using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class SafetyTutorialManager : MonoBehaviour
{
    public static SafetyTutorialManager Instance { get; private set; }

    public enum TutorialPhase { Off, FreeOrder, Sequential }

    [Header("Configuración")]
    public SafetyChecklistData checklistData;
    public TutorialPhase currentPhase = TutorialPhase.Off;

    [Header("UI (Canvas VR)")]
    public TextMeshProUGUI phaseText;
    public Transform checklistContainer; // Contenedor para la lista de ítems de la UI
    public GameObject itemUIPrefab; // Prefab de la UI para cada ítem

    private HashSet<string> collectedItems = new HashSet<string>();
    private int nextSequentialIndex = 0;

    // Evento para notificar a la UI sobre el progreso
    public event Action<string> OnItemCollected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (MachineSelectionManager.Instance != null)
        {
            // Solo iniciamos el tutorial si estamos en modo Práctica
            MachineSelectionManager.Instance.OnModeSelected += StartTutorialIfPractice;

            // Asumiendo que el modo ya fue seleccionado antes de iniciar la escena:
            StartTutorialIfPractice(MachineSelectionManager.Instance.SelectedMode);
        }

        if (checklistData != null)
        {
            InitializeUI();
        }
    }

    private void StartTutorialIfPractice(PlayMode mode)
    {
        if (mode == PlayMode.Practice)
        {
            StartPhase(TutorialPhase.FreeOrder);
        }
        else
        {
            currentPhase = TutorialPhase.Off;
            // Ocultar UI de tutorial si no estamos en práctica
        }
    }

    private void InitializeUI()
    {
        // Limpiar contenedor y crear elementos UI para cada ítem en checklistData
        foreach (Transform child in checklistContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in checklistData.requiredItems)
        {
            // Crear instancia de itemUIPrefab (debe tener un TextMeshPro para el nombre del ítem)
            // GameObject uiItem = Instantiate(itemUIPrefab, checklistContainer);
            // uiItem.GetComponentInChildren<TextMeshProUGUI>().text = item;
            // Se asume esta UI está lista en la escena
        }
    }

    public void StartPhase(TutorialPhase phase)
    {
        currentPhase = phase;
        collectedItems.Clear();
        nextSequentialIndex = 0;

        if (phase == TutorialPhase.FreeOrder)
        {
            phaseText.text = "FASE 1: Colección Libre de EPI";
            Debug.Log("[SAFETY TUTORIAL] Fase 1 Iniciada: Colección Libre");
        }
        else if (phase == TutorialPhase.Sequential)
        {
            phaseText.text = "FASE 2: Colección Secuencial de EPI";
            Debug.Log("[SAFETY TUTORIAL] Fase 2 Iniciada: Colección Secuencial");
        }
    }

    public void MarkItemCollected(string itemID)
    {
        if (currentPhase == TutorialPhase.Off || collectedItems.Contains(itemID))
        {
            Debug.Log($"[SAFETY TUTORIAL] Ignorando recolección de '{itemID}'. Fase OFF o ítem ya recogido.");
            return;
        }

        bool success = false;

        if (currentPhase == TutorialPhase.FreeOrder)
        {
            success = checklistData.requiredItems.Contains(itemID);
        }
        else if (currentPhase == TutorialPhase.Sequential)
        {
            // Verificar si es el siguiente ítem en la secuencia
            if (nextSequentialIndex < checklistData.requiredItems.Count && checklistData.requiredItems[nextSequentialIndex] == itemID)
            {
                success = true;
                nextSequentialIndex++;
            }
            else if (checklistData.requiredItems.Contains(itemID) && !collectedItems.Contains(itemID))
            {
                // Feedback si el orden es incorrecto
                Debug.LogWarning($"[SAFETY TUTORIAL] ¡Orden Incorrecto! Esperaba: {checklistData.requiredItems[nextSequentialIndex]}, Recogió: {itemID}");
                // Aquí podrías llamar al FeedbackManager con un mensaje de advertencia específico si lo deseas.
                return;
            }
        }

        if (success)
        {
            collectedItems.Add(itemID);
            OnItemCollected?.Invoke(itemID);

            Debug.Log($"[SAFETY TUTORIAL] Ítem recogido con éxito: {itemID}. Total: {collectedItems.Count}/{checklistData.requiredItems.Count}");

            // Verificar finalización de fase
            if (collectedItems.Count >= checklistData.requiredItems.Count)
            {
                if (currentPhase == TutorialPhase.FreeOrder)
                {
                    Debug.Log("[SAFETY TUTORIAL] Fase 1 Completada. Iniciando Fase 2.");
                    StartPhase(TutorialPhase.Sequential);
                }
                else if (currentPhase == TutorialPhase.Sequential)
                {
                    Debug.Log("[SAFETY TUTORIAL] Fase 2 Completada. Tutorial de Seguridad FINALIZADO.");
                    currentPhase = TutorialPhase.Off; // Tutorial completado
                }
            }
        }
        else
        {
            Debug.LogWarning($"[SAFETY TUTORIAL] Ítem '{itemID}' no es parte de la lista requerida o la lógica de fase falló.");
        }
    }

    // Método que usa WeldingSafetyGuard para verificar si se puede soldar.
    public bool AreAllItemsCollected()
    {
        if (currentPhase == TutorialPhase.Off || checklistData == null)
        {
            // Si el tutorial está apagado (Ej: Modo Evaluación), asumimos que el EPI es verificable por otros medios o es opcional.
            return true;
        }

        return collectedItems.Count >= checklistData.requiredItems.Count;
    }

    // Método para verificar la existencia de un ítem específico para el WeldingSafetyGuard
    public bool HasItem(string itemID)
    {
        bool result = collectedItems.Contains(itemID);
        Debug.Log($"[SAFETY TUTORIAL CHECK] Verificando ítem '{itemID}': Resultado -> {result}");
        return result;
    }
}