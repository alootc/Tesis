using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Reflection; // Necesario para el atributo [Button]

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveLoadData : MonoBehaviour
{
    // *** 1. CONFIGURACIÓN ***
    // (Mantén aquí la URL de tu último despliegue CORREGIDO de Apps Script)
    public string url = "https://script.google.com/macros/s/AKfycbwO8ybjwf5Gs3qSlLdS2oVkVsdKxsT6tdAUJY5HjW3SGO3tIbO3HiGg33AprcLrSpw/exec";
    private const string Separador = "|"; // Separador para concatenar las respuestas

    [Header("ID del Evaluador")]
    public string nombreEvaluador = "Estudiante_001";

    // *** 2. VARIABLES DEL CUESTIONARIO (16 RESPUESTAS) ***

    // 1. Datos del Evaluador
    [Header("1. Datos del Evaluador")]
    public string Q1_2_Experiencia = ""; // Experiencia Previa en Soldadura

    // 2. Usabilidad e Interfaz (1 a 5)
    [Header("2. Usabilidad (1 a 5)")]
    public string Q2_1_Navegacion = "";     // Facilidad para moverse
    public string Q2_2_InteraccionEPI = "";  // Naturalidad con EPI
    public string Q2_5_ListaTareas = "";     // Claridad de Checklist
    public string Q2_6_Rendimiento = "";     // Ausencia de lag

    // 3. Funcionalidad (Sí / No / Etc)
    [Header("3. Funcionalidad (Sí/No)")]
    public string Q3_1_DeteccionEPI = "";    // Detección de EPI Faltante
    public string Q3_2_ProgresionFase = "";  // Progresión de Fase
    public string Q3_4_ErroresCriticos = ""; // Detección de Errores Críticos

    // 4. Realismo (1 a 5 / Texto)
    [Header("4. Realismo y Parámetros")]
    public string Q4_1_RealismoArco = "";     // Realismo Visual del Arco
    public string Q4_4_ComportamientoACDC = ""; // Comportamiento AC vs DC

    // 5. Reporte Final (1 a 5)
    [Header("5. Reporte Final (1 a 5)")]
    public string Q5_1_ClaridadPuntuacion = ""; // Claridad de Puntuación
    public string Q5_2_DetalleReporte = "";     // Detalle del Reporte
    public string Q5_3_UtilidadFallas = "";     // Utilidad de Fallas Registradas

    // 6. Satisfacción General
    [Header("6. Satisfacción (1 a 5 / Sí-No)")]
    public string Q6_1_SatisfaccionGeneral = ""; // Satisfacción General
    public string Q6_2_PotencialAprendizaje = ""; // Potencial de Aprendizaje
    public string Q6_3_Recomendacion = "";      // ¿Recomendarías?


    // *** 3. FUNCIONES DE ASIGNACIÓN (PARA BOTONES DE UNITY) ***

    public void SetNombreEvaluador(string nombre) { nombreEvaluador = nombre; Debug.Log($"ID asignado: {nombre}"); }

    // 1. Datos del Evaluador
    public void SetExperienciaPrevia(string valor) { Q1_2_Experiencia = valor; Debug.Log($"Q1.2 (Experiencia) asignada: {valor}"); }

    // 2. Usabilidad e Interfaz
    public void SetNavegacion(string valor) { Q2_1_Navegacion = valor; Debug.Log($"Q2.1 (Navegación) asignada: {valor}"); }
    public void SetInteraccionEPI(string valor) { Q2_2_InteraccionEPI = valor; Debug.Log($"Q2.2 (EPI) asignada: {valor}"); }
    public void SetListaTareas(string valor) { Q2_5_ListaTareas = valor; Debug.Log($"Q2.5 (Lista Tareas) asignada: {valor}"); }
    public void SetRendimiento(string valor) { Q2_6_Rendimiento = valor; Debug.Log($"Q2.6 (Rendimiento) asignada: {valor}"); }

    // 3. Funcionalidad
    public void SetDeteccionEPI(string valor) { Q3_1_DeteccionEPI = valor; Debug.Log($"Q3.1 (Detección EPI) asignada: {valor}"); }
    public void SetProgresionFase(string valor) { Q3_2_ProgresionFase = valor; Debug.Log($"Q3.2 (Progresión Fase) asignada: {valor}"); }
    public void SetErroresCriticos(string valor) { Q3_4_ErroresCriticos = valor; Debug.Log($"Q3.4 (Errores Críticos) asignada: {valor}"); }

    // 4. Realismo
    public void SetRealismoArco(string valor) { Q4_1_RealismoArco = valor; Debug.Log($"Q4.1 (Realismo Arco) asignada: {valor}"); }
    public void SetComportamientoACDC(string valor) { Q4_4_ComportamientoACDC = valor; Debug.Log($"Q4.4 (AC/DC) asignada: {valor}"); }

    // 5. Reporte Final
    public void SetClaridadPuntuacion(string valor) { Q5_1_ClaridadPuntuacion = valor; Debug.Log($"Q5.1 (Puntuación) asignada: {valor}"); }
    public void SetDetalleReporte(string valor) { Q5_2_DetalleReporte = valor; Debug.Log($"Q5.2 (Detalle Reporte) asignada: {valor}"); }
    public void SetUtilidadFallas(string valor) { Q5_3_UtilidadFallas = valor; Debug.Log($"Q5.3 (Utilidad Fallas) asignada: {valor}"); }

    // 6. Satisfacción
    public void SetSatisfaccionGeneral(string valor) { Q6_1_SatisfaccionGeneral = valor; Debug.Log($"Q6.1 (Satisfacción) asignada: {valor}"); }
    public void SetPotencialAprendizaje(string valor) { Q6_2_PotencialAprendizaje = valor; Debug.Log($"Q6.2 (Potencial) asignada: {valor}"); }
    public void SetRecomendacion(string valor) { Q6_3_Recomendacion = valor; Debug.Log($"Q6.3 (Recomendación) asignada: {valor}"); }


    // *** 4. LÓGICA DE ENVÍO Y CONCATENACIÓN (16 Respuestas en 5 bloques) ***

    [Button]
    public void SendDataCoroutine()
    {
        // Distribución de 16 respuestas en 5 bloques (4 + 4 + 4 + 3 + 1)

        // BLOQUE R1 (4 Preguntas: Q1.2, Q2.1, Q2.2, Q2.5)
        string r1_concatenado =
            Q1_2_Experiencia + Separador +
            Q2_1_Navegacion + Separador +
            Q2_2_InteraccionEPI + Separador +
            Q2_5_ListaTareas;

        // BLOQUE R2 (4 Preguntas: Q2.6, Q3.1, Q3.2, Q3.4)
        string r2_concatenado =
            Q2_6_Rendimiento + Separador +
            Q3_1_DeteccionEPI + Separador +
            Q3_2_ProgresionFase + Separador +
            Q3_4_ErroresCriticos;

        // BLOQUE R3 (4 Preguntas: Q4.1, Q4.4, Q5.1, Q5.2)
        string r3_concatenado =
            Q4_1_RealismoArco + Separador +
            Q4_4_ComportamientoACDC + Separador +
            Q5_1_ClaridadPuntuacion + Separador +
            Q5_2_DetalleReporte;

        // BLOQUE R4 (3 Preguntas: Q5.3, Q6.1, Q6.2)
        string r4_concatenado =
            Q5_3_UtilidadFallas + Separador +
            Q6_1_SatisfaccionGeneral + Separador +
            Q6_2_PotencialAprendizaje;

        // BLOQUE R5 (1 Pregunta: Q6.3)
        string r5_concatenado =
            Q6_3_Recomendacion;

        Debug.Log("Iniciando envío de 16 respuestas en 5 bloques concatenados...");

        StartCoroutine(SendData(
            nombreEvaluador,
            r1_concatenado,
            r2_concatenado,
            r3_concatenado,
            r4_concatenado,
            r5_concatenado));
    }

    // La función SendData, GetData y el Editor no necesitan cambios.
    private IEnumerator SendData(string nombre, string r1, string r2, string r3, string r4, string r5)
    {
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombre);
        form.AddField("r1", r1);
        form.AddField("r2", r2);
        form.AddField("r3", r3);
        form.AddField("r4", r4);
        form.AddField("r5", r5);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Datos enviados correctamente. OK recibido.");
            }
            else
            {
                Debug.LogError($"Error al enviar datos: {www.error}");
            }
        }
    }

    [Button]
    public void GetDataCoroutine()
    {
        StartCoroutine(GetData());
    }

    private IEnumerator GetData()
    {
        Debug.Log("Iniciando solicitud GET...");
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Datos recibidos: {www.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"Error al recibir datos: {www.error}");
            }
        }
    }
}

internal class ButtonAttribute : Attribute
{
}

#if UNITY_EDITOR

[CustomEditor(typeof(SaveLoadData))]
public class SaveLoadDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SaveLoadData targetScript = (SaveLoadData)target;

        foreach (MethodInfo method in targetScript.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (method.IsDefined(typeof(ButtonAttribute), true))
            {
                if (GUILayout.Button($"▶️ {method.Name}"))
                {
                    method.Invoke(targetScript, null);
                }
            }
        }
    }
}

#endif
/*
public class SaveLoadData : MonoBehaviour
{

    public string url = "https://script.google.com/macros/s/AKfycbzTCmrSI8a3-3hZutUYflhM8P5ne3svzxB_qX-4n3HBu5LU5jVoZROekrpgTCV4KHPi2g/exec";
  
    void Start()
    {

    }
    [Button]
    public void GetDataCoroutine()
    {
        StartCoroutine(GetData());
    }
    [Button]
    public void SendDataCoroutine(string nombre, string r1, string r2, string r3)
    {
        StartCoroutine(SendData(nombre, r1, r2, r3));
    }
    IEnumerator GetData()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Datos recibidos:");
                Debug.Log(www.downloadHandler.text);
            }
            else
            {
                Debug.Log("Error: " + www.error);
            }
        }
    }

    IEnumerator SendData(string nombre, string r1, string r2, string r3)
    {
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombre);
        form.AddField("r1", r1);  // puede ser "A" "B" "C" "D"
        form.AddField("r2", r2);
        form.AddField("r3", r3);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Enviado correctamente");
            }
            else
            {
                Debug.Log("Error al enviar: " + www.error);
            }
        }
    }
}
*/