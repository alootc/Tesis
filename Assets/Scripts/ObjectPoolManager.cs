using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject prefab;    // Prefab del objeto a instanciar
    [SerializeField] private int poolSize = 10; // Cantidad de objetos a crear

    private readonly List<GameObject> pool = new List<GameObject>(); // Lista de objetos creados
    public int activeCount = 0; // Cantidad de objetos activos
    
    #region Unity Methods
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
    #endregion

    #region Public API

    public GameObject GetObject(Vector3 position, Quaternion rotation,Transform parent)
    {
        //GameObject obj = null;
        GameObject obj = pool.Find(item => !item.activeInHierarchy);

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
    #endregion

}
