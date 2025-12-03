
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#if UNITY_EDITOR 
using UnityEditor;
using System.Reflection;
#endif 

public class SaveLoadData : MonoBehaviour
{
    public string url = "https://script.google.com/macros/s/AKfycbz89hAS-wV8owwfbR7JvZBh8VPD9C3xTlavnEIAiG-aURbT035ItX-VBMfNR0CokGOS/exec ";

    [Header("Parámetros de Prueba (Envío)")]
    public string nombrePrueba = "Jugador1";
    public string r1Prueba = "A";
    public string r2Prueba = "B";
    public string r3Prueba = "C";


    void Start()
    {
        
    }

    [Button]
    public void GetDataCoroutine()
    {
        StartCoroutine(GetData());
    }

    [Button]
    public void SendDataCoroutine()
    {
        StartCoroutine(SendData(nombrePrueba, r1Prueba, r2Prueba, r3Prueba));
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

    private IEnumerator SendData(string nombre, string r1, string r2, string r3)
    {
        Debug.Log($"Iniciando solicitud POST para: {nombre}");
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombre);
        form.AddField("r1", r1);
        form.AddField("r2", r2);
        form.AddField("r3", r3);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(" Datos enviados correctamente.");
            }
            else
            {
                Debug.LogError($" Error al enviar datos: {www.error}");
            }
        }
    }
}


// ----------------------------------------------------
// CÓDIGO DEL EDITOR (Solo se compila en el Editor de Unity)
// ----------------------------------------------------

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