using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MachineCardController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI machineNameText;
    [SerializeField] private Transform modelContainer; // where model is instantiated (local)


    [Header("Animation")]
    [SerializeField] private float expandedScale = 1.12f;
    [SerializeField] private float animationDuration = 0.35f;

    private MachineData data;
    private GameObject modelInstance;
    private Button button;
    private Vector3 defaultScale;

    public event Action<MachineCardController> OnCardSelected;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
        defaultScale = transform.localScale;
    }

    public void Initialize(MachineData machineData, MachineFactory factory)
    {
        data = machineData;
        machineNameText.text = data.machineName;
        // instantiate model (inactive initially to save performance)
        if (modelInstance != null) Destroy(modelInstance);
        if (data.modelPrefab != null && modelContainer != null)
        {
            modelInstance = factory.CreateModelInstance(data, modelContainer);
            modelInstance.SetActive(false); // only active in expanded view
        }
    }

    private void HandleClick()
    {
        OnCardSelected?.Invoke(this);
    }

    public void Expand()
    {
        transform.DOScale(expandedScale, animationDuration).SetEase(Ease.OutBack);
        if (modelInstance != null) modelInstance.SetActive(true);
    }

    public void Collapse()
    {
        transform.DOScale(defaultScale, animationDuration).SetEase(Ease.OutBack);
        if (modelInstance != null) modelInstance.SetActive(false);
    }

    public MachineData GetMachineData() => data;

    private void OnDestroy()
    {
        button.onClick.RemoveListener(HandleClick);
    }
}


