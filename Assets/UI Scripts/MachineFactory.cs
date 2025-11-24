using UnityEngine;

public class MachineFactory : MonoBehaviour
{
    public GameObject CreateModelInstance(MachineData data, Transform parent)
    {
        if (data == null || data.modelPrefab == null) return null;
        var go = Instantiate(data.modelPrefab, parent);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }
}
