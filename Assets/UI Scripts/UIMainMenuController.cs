using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIMainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button practiceButton;
    [SerializeField] private Button evaluationButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitButton;

    [Header("Panels")]
    [SerializeField] private GameObject machineSelectorPanel;
    [SerializeField] private RectTransform machineSelectorContainer;

    [Header("References")]
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject panelOptions;
    [SerializeField] private RectTransform optionsContainer;

    [Header("Helper")]
    [SerializeField] private UIAnimationHelper animator;

    private void Awake()
    {
        machineSelectorPanel.SetActive(false);
        panelOptions.SetActive(false);

        startButton.onClick.AddListener(OnStartClicked);

        practiceButton.onClick.AddListener(() => OnModeSelected(PlayMode.Practice));
        evaluationButton.onClick.AddListener(() => OnModeSelected(PlayMode.Evaluation));
        creditsButton.onClick.AddListener(OnCredits);
        exitButton.onClick.AddListener(OnExit);
    }

    private void OnStartClicked()
    {
        animator.ScaleDown(startButton.transform, 0.8f, 0.25f);

        startButton.gameObject.SetActive(false);

        panelOptions.SetActive(true);
        optionsContainer.localScale = Vector3.zero;

        animator.ScaleUp(optionsContainer, 1f, 0.4f);
    }

    private void OnModeSelected(PlayMode mode)
    {
        MachineSelectionManager.Instance.SetMode(mode);

        machineSelectorPanel.SetActive(true);
        machineSelectorContainer.localScale = Vector3.zero;

        animator.ScaleUp(machineSelectorContainer, 1f, 0.35f);
    }

    private void OnCredits()
    {
        Debug.Log("Open Credits...");
    }

    private void OnExit()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(OnStartClicked);

        practiceButton.onClick.RemoveAllListeners();
        evaluationButton.onClick.RemoveAllListeners();
        creditsButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
    }
}
