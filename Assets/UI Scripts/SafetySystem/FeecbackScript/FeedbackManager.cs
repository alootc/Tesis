using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("UI del Panel de Feedback")]
    public GameObject feedbackPanel;
    public Image iconImage;
    public TextMeshProUGUI messageText;

    [Header("Iconografía")]
    public Sprite warningIcon;
    public Sprite errorIcon;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        feedbackPanel?.SetActive(false);
    }

    public void ShowFeedback(FeedbackData data, float displayTime = 5.0f)
    {
        if (feedbackPanel == null)
        {
            Debug.LogError("Panel de feedback no asignado.");
            return;
        }

        // 1. Configurar Icono (Advertencia o Error)
        if (data.type == FeedbackType.Error)
        {
            iconImage.sprite = errorIcon;
            iconImage.color = Color.red;
        }
        else // Warning
        {
            iconImage.sprite = warningIcon;
            iconImage.color = Color.yellow;
        }

        // 2. Configurar Texto e Imagen
        messageText.text = data.messageText_ES;
        // Asume que hay un segundo componente Image para la imagen específica del feedback si es necesario
        // Pero usamos la misma iconImage por simplicidad.

        // 3. Mostrar Panel
        feedbackPanel.SetActive(true);

        // 4. Temporizar Ocultamiento
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(HideAfterDelay(displayTime));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackPanel.SetActive(false);
    }
}
