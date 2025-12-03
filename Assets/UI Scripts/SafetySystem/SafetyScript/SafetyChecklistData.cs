using System.Collections.Generic;
using UnityEngine;

// ScriptableObject para definir los elementos de seguridad requeridos y su orden.
[CreateAssetMenu(menuName = "Safety/Safety Checklist Data")]
public class SafetyChecklistData : ScriptableObject
{
    [Tooltip("Lista de elementos de EPI que deben recogerse en la FASE 1 (Colección Libre).")]
    public List<string> freeOrderItems = new List<string>();

    [Tooltip("Lista de elementos y/o pasos que deben validarse en la FASE 2 (Secuencial).")]
    public List<string> sequentialItems = new List<string>();

    [Tooltip("Nombre de las escenas o módulos donde se utiliza esta lista (opcional).")]
    public string moduleName = "Preparación SMAW";
}