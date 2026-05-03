using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Image arrowImage;
    [SerializeField] private float arrowMoveDistance = 50f;
    [SerializeField] private float arrowMoveSpeed = 0.1f;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip briefingMusic;
    [SerializeField] private AudioClip briefingVoiceover;
    private AudioSource audioSource;

    [SerializeField] private GameObject briefingPanel;
    private bool briefingHappening = false;
    
    private int currentSelectionIndex = 0;
    private RectTransform arrowRect;
    private Vector2 arrowStartPosition;
    private Coroutine arrowMoveCoroutine;
    private Image fadeScreen;

    private void Start()
    {
        arrowRect = arrowImage.rectTransform;
        arrowStartPosition = arrowRect.anchoredPosition;
        audioSource = GetComponent<AudioSource>();
        CreateFadeScreen();
        if (audioSource != null && menuMusic != null)
        {
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelectionIndex = Mathf.Max(0, currentSelectionIndex - 1);
            MoveArrowToIndex(currentSelectionIndex);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelectionIndex = Mathf.Min(2, currentSelectionIndex + 1);
            MoveArrowToIndex(currentSelectionIndex);
        }

        if (briefingHappening && Input.GetKeyDown(KeyCode.X))
        {
            StartCoroutine(FadeAndLoadLevel("LevelOne"));
        }

        if (!briefingHappening && Input.GetKeyDown(KeyCode.X))
        {
            if (currentSelectionIndex == 0)
            {
                // Start Game
                StartCoroutine(FadeAndPlayBriefing());
                Input.ResetInputAxes();
            }
            else if (currentSelectionIndex == 1)
            {
                // Options
            }
            else if (currentSelectionIndex == 2)
            {
                // Credits
            }
        }
    }

    private void MoveArrowToIndex(int index)
    {
        if (arrowMoveCoroutine != null)
        {
            StopCoroutine(arrowMoveCoroutine);
        }
        
        Vector2 targetPosition = arrowStartPosition + Vector2.down * (index * arrowMoveDistance);
        arrowMoveCoroutine = StartCoroutine(SmoothMoveArrow(targetPosition));
    }

    private IEnumerator SmoothMoveArrow(Vector2 targetPosition)
    {
        Vector2 startPosition = arrowRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < arrowMoveSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / arrowMoveSpeed);
            arrowRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        arrowRect.anchoredPosition = targetPosition;
    }

    private void PlayBriefing()
    {
        briefingHappening = true;

        if (briefingPanel != null)
        {
            briefingPanel.SetActive(true);
        }

        if (audioSource != null)
        {
            if (briefingMusic != null)
            {
                audioSource.clip = briefingMusic;
                audioSource.loop = true;
                audioSource.Play();
            }
            if (briefingVoiceover != null)
            {
                audioSource.PlayOneShot(briefingVoiceover);
            }
        }
        
        StartCoroutine(FadeFromBlack());
    }

    private IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

    private void CreateFadeScreen()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        fadeScreen = imageObj.AddComponent<Image>();
        fadeScreen.color = new Color(0, 0, 0, 0);
        
        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private IEnumerator FadeToBlack(float duration = 1.5f)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / duration);
            fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeScreen.color = new Color(0, 0, 0, 1);
    }

    private IEnumerator FadeFromBlack(float duration = 1.5f)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsedTime / duration));
            fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeScreen.color = new Color(0, 0, 0, 0);
    }

    private IEnumerator FadeAndPlayBriefing()
    {
        // Fade to black while music fades
        StartCoroutine(FadeOutAudio(audioSource, 1.5f));
        yield return StartCoroutine(FadeToBlack(1.5f));
        
        // Play briefing (includes fade back in)
        PlayBriefing();
    }

    private IEnumerator FadeAndLoadLevel(string sceneName)
    {
        yield return StartCoroutine(FadeToBlack(1.5f));
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}