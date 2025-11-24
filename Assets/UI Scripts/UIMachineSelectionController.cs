using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIMachineSelectionController : MonoBehaviour
{
    #region Card List
    [Header("Cards")]
    [SerializeField] private MachineCardController cardPrefab;
    [SerializeField] private Transform cardsParent;
    [SerializeField] private List<MachineData> machineDatas;
    [SerializeField] private MachineFactory factory;

    [Header("Card Positions (Manual)")]
    [SerializeField] private List<Vector2> cardPositions;

    private List<MachineCardController> cards = new List<MachineCardController>();
    private MachineCardController expandedCard;
    #endregion

    #region Info Panel
    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanelGO;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI specsText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button closeButton;
    #endregion

    #region Lifecycle
    private void Start()
    {
        infoPanelGO.SetActive(false);
        acceptButton.onClick.AddListener(OnAccept);
        closeButton.onClick.AddListener(CloseInfo);
        PopulateCards();
    }
    #endregion

    #region Card Logic
    private void PopulateCards()
    {
        foreach (Transform t in cardsParent) Destroy(t.gameObject);
        cards.Clear();

        for (int i = 0; i < machineDatas.Count; i++)
        {
            var card = Instantiate(cardPrefab, cardsParent);
            card.Initialize(machineDatas[i], factory);
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.anchoredPosition = cardPositions[i];   // ← ← Posición manual
            card.OnCardSelected += HandleCardSelected;
            cards.Add(card);
        }
    }

    private void HandleCardSelected(MachineCardController card)
    {
        if (expandedCard != null && expandedCard != card)
            expandedCard.Collapse();

        expandedCard = card;
        expandedCard.Expand();

        ShowInfo(card.GetMachineData(), card);
    }
    #endregion

    #region Info Logic
    private void ShowInfo(MachineData data, MachineCardController card)
    {
        cardsParent.gameObject.SetActive(false);   // ⬅⬅ Oculta el panel de AC/DC

        nameText.text = data.machineName;
        descriptionText.text = data.description;
        specsText.text = FormatSpecs(data);

        infoPanelGO.SetActive(true);
    }


    private void CloseInfo()
    {
        infoPanelGO.SetActive(false);
        cardsParent.gameObject.SetActive(true);   // ⬅⬅ Muestra nuevamente AC/DC

        expandedCard?.Collapse();
    }


    private string FormatSpecs(MachineData d)
    {
        var s = $"Voltage: {d.defaultVoltage}V\nCurrent: {d.defaultCurrent}A\nComponents:\n";
        foreach (var c in d.components) s += $"- {c}\n";
        return s;
    }

    private void OnAccept()
    {
        MachineSelectionManager.Instance.SetMachine(expandedCard.GetMachineData());
        FindObjectOfType<SceneLoader>()?.LoadScene("MainVR");
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        foreach (var c in cards) c.OnCardSelected -= HandleCardSelected;
        acceptButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
    }
    #endregion
}

