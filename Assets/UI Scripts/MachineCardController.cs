using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MachineCardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI machineNameText;
    [SerializeField] private Transform modelContainer;

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

        if (modelInstance != null)
            Destroy(modelInstance);

        if (data.modelPrefab != null && modelContainer != null)
        {
            modelInstance = factory.CreateModelInstance(data, modelContainer);
            modelInstance.SetActive(true);

            var rot = modelInstance.GetComponent<MachineModelRotator>();
            if (rot != null)
                rot.enabled = false;
        }
    }

    private void HandleClick()
    {
        OnCardSelected?.Invoke(this);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Expand();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Collapse();
    }
    public void Expand()
    {
        transform.DOScale(expandedScale, animationDuration).SetEase(Ease.OutBack);
    }
    public void Collapse()
    {
        transform.DOScale(defaultScale, animationDuration).SetEase(Ease.OutBack);
    }
    private void OnDestroy()
    {
        button.onClick.RemoveListener(HandleClick);
    }
    public MachineData GetMachineData() => data;
}
