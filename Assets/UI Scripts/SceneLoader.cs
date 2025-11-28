using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private Image fadeImage; 
    [SerializeField] private float fadeDuration = 0.6f;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Reset()
    {
        fadeImage = GetComponentInChildren<Image>();
        if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }
    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        if (fadeImage != null)
        {
            yield return fadeImage
                .DOFade(1f, fadeDuration)
                .SetUpdate(true)
                .WaitForCompletion();
        }

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;
        while (async.progress < 0.9f)
            yield return null;
        async.allowSceneActivation = true;
        while (!async.isDone)
            yield return null;
        if (fadeImage != null)
        {
            fadeImage.DOFade(0f, fadeDuration)
                     .SetUpdate(true)
                     .WaitForCompletion();
        }
    }
}

