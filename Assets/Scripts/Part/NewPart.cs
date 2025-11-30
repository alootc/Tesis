using UnityEngine;

public class NewPart : MonoBehaviour
{
    public float weight = 1;

    // Comentado: Se eliminan las variables de soldadura fijas y se obtienen en tiempo real
    /*
    public float voltage = 22.0f;  // Voltaje único de cada cubo
    public float wireSpeed = 385.0f; // Velocidad de cable única
    */

    private float totalTime = 0.0f;
    private string finalResult = "Esperando...";
    public bool isBeingWelded = false;
    /// 


    Rigidbody rigidbody;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        // Comentado: Asegurar que la pieza tenga tag "Metal" para que la pistola la detecte
        if (!gameObject.CompareTag("Metal"))
        {
            gameObject.tag = "Metal";
        }

        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        rigidbody.mass = weight;
    }


    /// 
    private void Update()
    {
        if (isBeingWelded)
        {
            totalTime += Time.deltaTime;
        }
    }

    public void StartWelding()
    {
        isBeingWelded = true;
    }

    public void StopWelding()
    {
        isBeingWelded = false;
        finalResult = GenerateWeldingResult();
    }

    // Comentado: Se añade la lógica para obtener los parámetros de la máquina
    private string GenerateWeldingResult()
    {
        float voltage = 0f;
        float current = 0f;

        // Intentar obtener la máquina seleccionada
        if (MachineSelectionManager.Instance != null && MachineSelectionManager.Instance.SelectedMachine != null)
        {
            voltage = MachineSelectionManager.Instance.SelectedMachine.defaultVoltage;
            current = MachineSelectionManager.Instance.SelectedMachine.defaultCurrent;
        }

        // Comentado: Lógica de resultado mejorada usando parámetros de la máquina
        int baseRate = 50;
        float voltageFactor = Mathf.Clamp((voltage / 25.0f) * 10f, -10f, 15f); // 25V es un buen valor.
        float currentFactor = Mathf.Clamp((current / 400.0f) * 10f, -10f, 15f); // 400A es un buen valor.

        int successRate = baseRate + Mathf.RoundToInt(voltageFactor + currentFactor) + Random.Range(-5, 5);
        successRate = Mathf.Clamp(successRate, 0, 100);

        string quality = "Malo";
        if (successRate > 80) quality = "Excelente";
        else if (successRate > 65) quality = "Bueno";
        else if (successRate > 50) quality = "Regular";

        return successRate + "% " + quality;
    }

    public float GetTotalTime()
    {
        return totalTime;
    }

    public string GetResult()
    {
        return finalResult;
    }
    ///


    public void AbsorbPiece(NewPart piece)
    {
        weight += piece.weight;
        rigidbody.mass = weight;

        GameObject obj = piece.gameObject;



        //Debug.Log("this: " + this.gameObject.name + " AbsorbPiece-> piece: " + obj.gameObject.name);

        // Comentado: Usar try-catch o verificación nula al destruir, es buena práctica
        var interactable = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable != null) Destroy(interactable);

        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        var np = obj.GetComponent<NewPart>();
        if (np != null) Destroy(np);

        obj.transform.parent = transform;
    }
}