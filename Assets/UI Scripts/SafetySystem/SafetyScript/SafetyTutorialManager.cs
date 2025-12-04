using System;
using System.Collections.Generic;
using System.Linq;
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

    // Esta lista registra TODOS los ítems de EPI que el jugador ha "equipado" a lo largo del juego.
    private HashSet<string> collectedItems = new HashSet<string>();

    // Este contador solo se usa para la Fase 2 (Secuencial).
    private int nextSequentialIndex = 0;

    // Conjunto de todos los ítems válidos para una verificación rápida.
    private HashSet<string> allValidItems = new HashSet<string>();

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
            // Subscribe to mode selection event
            MachineSelectionManager.Instance.OnModeSelected += StartTutorialIfPractice;

            // Check if mode is already set and start immediately if in Practice
            // Note: This line might need adaptation depending on when MachineSelectionManager sets the mode.
            // If the mode is set in a previous scene, you might need to check the public property here.
        }

        if (checklistData != null)
        {
            InitializeData(); // Calls data initialization and UI setup
        }
    }

    // Method to initialize the set of all valid items from both lists
    private void InitializeData()
    {
        // 1. Create the set of all valid items (used for generic validation)
        allValidItems.Clear();
        if (checklistData.freeOrderItems != null)
        {
            foreach (var item in checklistData.freeOrderItems)
            {
                allValidItems.Add(item);
            }
        }
        if (checklistData.sequentialItems != null)
        {
            foreach (var item in checklistData.sequentialItems)
            {
                allValidItems.Add(item);
            }
        }

        // 2. Initialize UI (assuming UI logic that integrates both lists)
        InitializeUI();
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
        }
    }

    private void InitializeUI()
    {
        // Logic for initializing the UI of the checklist (omitting the actual prefab creation)
        foreach (Transform child in checklistContainer)
        {
            Destroy(child.gameObject);
        }
        // ... (Your code to create UI elements)
    }

    public void StartPhase(TutorialPhase phase)
    {
        currentPhase = phase;
        nextSequentialIndex = 0; // Always reset the sequential index

        if (phase == TutorialPhase.FreeOrder)
        {
            collectedItems.Clear(); // <-- Only cleared when starting the tutorial from scratch (Phase 1)
            phaseText.text = "FASE 1: Colección Libre de EPI";
            Debug.Log("[SAFETY TUTORIAL] Phase 1 Started: Free Collection. EPI list cleared.");
        }
        else if (phase == TutorialPhase.Sequential)
        {
            // collectedItems is NOT cleared. Items collected in Phase 1 are still valid for welding.
            phaseText.text = "FASE 2: Colección Secuencial de EPI";
            Debug.Log("[SAFETY TUTORIAL] Phase 2 Started: Sequential Collection. Phase 1 items MAINTAINED.");
        }
    }

    public void MarkItemCollected(string itemID)
    {
        // 1. Handle collection or order
        bool isAlreadyCollected = collectedItems.Contains(itemID);
        bool shouldProgress = false;

        if (currentPhase == TutorialPhase.Off)
        {
            Debug.Log($"[SAFETY TUTORIAL] Ignoring collection of '{itemID}'. Phase OFF.");
            return;
        }

        // The item must be a valid safety item or sequential step
        if (!allValidItems.Contains(itemID))
        {
            Debug.LogWarning($"[SAFETY TUTORIAL] Item '{itemID}' is not a valid safety element in any phase.");
            return;
        }


        if (currentPhase == TutorialPhase.FreeOrder)
        {
            // Phase 1 only progresses if the item is in the FreeOrder list and has not been collected.
            if (isAlreadyCollected || !checklistData.freeOrderItems.Contains(itemID)) return;

            collectedItems.Add(itemID); // Register as "equipped"
            shouldProgress = true;
            Debug.Log($"[SAFETY TUTORIAL] Item successfully collected: {itemID} (Free Phase).");
        }
        else if (currentPhase == TutorialPhase.Sequential)
        {
            // Phase 2 only progresses if the item is the next one in the Sequential list.

            if (nextSequentialIndex < checklistData.sequentialItems.Count && checklistData.sequentialItems[nextSequentialIndex] == itemID)
            {
                // Correct Order: Progress the sequence and register the item if it wasn't already.
                if (!isAlreadyCollected)
                {
                    collectedItems.Add(itemID); // Register as "equipped" if it's an equipment step
                }
                nextSequentialIndex++;
                shouldProgress = true;
                Debug.Log($"[SAFETY TUTORIAL] Correct item in sequence: {itemID}. Progress: {nextSequentialIndex}/{checklistData.sequentialItems.Count}");
            }
            else if (checklistData.sequentialItems.Contains(itemID) && !isAlreadyCollected)
            {
                // Incorrect order
                Debug.LogWarning($"[SAFETY TUTORIAL] Incorrect Order! Expected: {checklistData.sequentialItems[nextSequentialIndex]}, Collected: {itemID}");
                return; // No progress if the order is incorrect
            }
            // If the item is already collected and not the next one, it is simply ignored.
        }

        // 2. Check phase completion
        if (shouldProgress)
        {
            OnItemCollected?.Invoke(itemID);

            // PHASE 1: Completes when all items in freeOrderItems have been collected.
            // Uses LINQ to count only the collected items that belong to the freeOrderItems list.
            if (currentPhase == TutorialPhase.FreeOrder && collectedItems.Count(item => checklistData.freeOrderItems.Contains(item)) >= checklistData.freeOrderItems.Count)
            {
                // *** AVISO CRÍTICO AL REPORTE: EPI (Fase 1) COMPLETADO ***
                if (FeedbackReportManager.Instance != null)
                {
                    FeedbackReportManager.Instance.SetEPICompletionStatus(true);
                }

                Debug.Log("[SAFETY TUTORIAL] Phase 1 (Collection) Completed. Starting Phase 2 (Sequential).");
                StartPhase(TutorialPhase.Sequential);
            }
            // PHASE 2: Completes when the sequence index reaches the end of sequentialItems.
            else if (currentPhase == TutorialPhase.Sequential && nextSequentialIndex >= checklistData.sequentialItems.Count)
            {
                // *** AVISO CRÍTICO AL REPORTE: SECUENCIA (Fase 2) COMPLETADA ***
                if (FeedbackReportManager.Instance != null)
                {
                    FeedbackReportManager.Instance.SetSequenceCompletionStatus(true);
                }

                Debug.Log("[SAFETY TUTORIAL] Phase 2 (Sequential) Completed. Safety Tutorial FINISHED.");
                currentPhase = TutorialPhase.Off; // Tutorial completed
            }
        }
    }

    // Method used by WeldingSafetyGuard to verify a specific item
    public bool HasItem(string itemID)
    {
        // If the tutorial is off (completed or in Evaluation mode), allow welding.
        if (currentPhase == TutorialPhase.Off) return true;

        // If the tutorial is active, verify if the item is in the collected EPI list.
        bool result = collectedItems.Contains(itemID);
        return result;
    }

    // Auxiliary method to allow other scripts (like the UI) to know the status.
    public bool IsItemCollected(string itemID)
    {
        return collectedItems.Contains(itemID);
    }
}