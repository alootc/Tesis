using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject prefab; // Prefab del objeto a instanciar
    public int poolSize = 10; // Cantidad de objetos a crear
    public int activeCount = 0; // Cantidad de objetos activos
    private List<GameObject> pool = new List<GameObject>(); // Lista de objetos creados

    void Start()
    {
        // Crear la cantidad inicial de objetos en (0,0,0) y desactivarlos
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    /// <summary>
    /// Toma un objeto del pool, lo activa y lo coloca en la posición y orientación dadas.
    /// </summary>
    /// <param name="position">Posición donde colocar el objeto.</param>
    /// <param name="rotation">Rotación del objeto.</param>
    /// <returns>El objeto activado.</returns>
    public GameObject GetObject(Vector3 position, Quaternion rotation,Transform parent)
    {
        GameObject obj = null;

        // Buscar un objeto inactivo en la lista
        foreach (GameObject item in pool)
        {
            if (!item.activeInHierarchy)
            {
                obj = item;
                break;
            }
        }

        // Si no hay objetos inactivos, usa el primero creado
        if (obj == null)
        {
            obj = pool[0];
        }

        // Activar y configurar el objeto
        obj.SetActive(true);
        obj.transform.parent = parent;
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        activeCount++; // Incrementar el conteo de activos
        return obj;
    }
}
