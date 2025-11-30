using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SafetySystem : MonoBehaviour
{
    public static SafetySystem Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject warningPanel; // panel que muestra icon + text
    [SerializeField] private UnityEngine.UI.Image iconImage;
    [SerializeField] private TMPro.TextMeshProUGUI messageText;
    [SerializeField] private float warningDuration = 2.5f;

    private HashSet<string> collectedItems = new HashSet<string>();
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (warningPanel) warningPanel.SetActive(false);
    }

    public void CollectItem(SafetyItemData data)
    {
        if (data == null) return;
        collectedItems.Add(data.itemName);
    }

    public void RemoveItem(SafetyItemData data)
    {
        if (data == null) return;
        collectedItems.Remove(data.itemName);
    }

    public bool HasItem(string itemName) => collectedItems.Contains(itemName);

    // check all mandatory items from a list
    public bool HasAllMandatory(List<SafetyItemData> allItems, out SafetyItemData missing)
    {
        missing = null;
        foreach (var it in allItems)
        {
            if (it.isMandatory && !HasItem(it.itemName))
            {
                missing = it;
                return false;
            }
        }
        return true;
    }

    public void ShowWarning(SafetyItemData item)
    {
        if (warningPanel == null) return;
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        iconImage.sprite = item.icon;
        messageText.text = item.warningText;
        warningPanel.SetActive(true);
        hideCoroutine = StartCoroutine(HideAfter());
    }

    private System.Collections.IEnumerator HideAfter()
    {
        yield return new WaitForSeconds(warningDuration);
        if (warningPanel) warningPanel.SetActive(false);
        hideCoroutine = null;
    }

    // show custom warning
    public void ShowWarning(string text, Sprite icon = null)
    {
        if (warningPanel == null) return;
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        iconImage.sprite = icon;
        messageText.text = text;
        warningPanel.SetActive(true);
        hideCoroutine = StartCoroutine(HideAfter());
    }
}
