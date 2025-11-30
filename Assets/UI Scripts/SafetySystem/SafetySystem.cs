using System.Collections.Generic;
using UnityEngine;

// ScriptableObject para definir los elementos de seguridad requeridos y su orden.
[CreateAssetMenu(menuName = "Safety/Safety Checklist Data")]
public class SafetyChecklistData : ScriptableObject
{
    [Tooltip("Lista de todos los elementos de EPI requeridos para la soldadura.")]
    public List<string> requiredItems = new List<string>();

    [Tooltip("Nombre de las escenas o módulos donde se utiliza esta lista (opcional).")]
    public string moduleName = "Preparación SMAW";
}
