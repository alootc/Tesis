using UnityEngine;

public class AcidDecalSpawner : MonoBehaviour
{
    public ObjectPoolManager decalPoolManager; // Comentado: Usaremos un ObjectPoolManager para los decals
    public GameObject decalPrefab; // Prefab del decal con Decal Projector - COMENTADO: Mantenido, pero se usará el Pool
    public float spawnInterval = 0.1f; // Intervalo de tiempo entre cada spawn de decal

    private float lastSpawnTime = 0f;

    // Comentado: El método ahora acepta el comportamiento de la máquina para modular el decal
    public void SpawnAcidDecal(Vector3 contactPosition, IMachineBehavior machineBehavior)
    {
        // Verifica si ha pasado el tiempo suficiente desde el último spawn
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            // Comentado: Si no se asigna el pool, usamos Instantiate como fallback (por si acaso)
            if (decalPoolManager == null)
            {
                Debug.LogError("decalPoolManager no está asignado. Usando Instantiate como fallback.");
                // GameObject decal = Instantiate(decalPrefab, contactPosition, Quaternion.identity); // CÓDIGO ANTERIOR COMENTADO
                lastSpawnTime = Time.time;
                return;
            }

            // Usar el Object Pool para obtener el decal
            GameObject decalGO = decalPoolManager.GetObject(contactPosition, Quaternion.identity, null);

            // Asegúrate de que el decal esté correctamente orientado hacia la superficie
            // Comentado: La rotación se puede omitir o ajustar dependiendo del prefab del Decal Projector.
            // decalGO.transform.forward = -decalGO.transform.up; // Ajusta según la orientación de tu prefab

            // Comentado: Lógica de la máquina: Ajustar la escala del decal según la penetración/intensidad
            if (machineBehavior != null)
            {
                // Usamos valores del comportamiento para modular el tamaño del cordón.
                // Simulamos que una mayor intensidad resulta en un cordón más amplio.
                float intensity = machineBehavior.GetArcIntensity(0, 0, 25f); // Usamos valores base para un cálculo
                float scaleFactor = 1.0f + intensity * 0.2f; // Factor de escala basado en la intensidad

                // Nota: El decal debe tener un script 'DecalController' para ajustar su tamaño
                // o debes ajustar directamente el transform.localScale del DecalGO
                decalGO.transform.localScale = Vector3.one * scaleFactor;
            }

            lastSpawnTime = Time.time; // Actualiza el tiempo del último spawn
        }
    }

    // Comentado: Nuevo método para asignar el Object Pool si no se hace en el editor
    // void Start()
    // {
    //     // Si decalPoolManager es nulo, buscar uno en la escena o crear uno
    // }
}