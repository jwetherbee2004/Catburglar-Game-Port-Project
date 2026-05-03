// using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class WinCondition : MonoBehaviour
{
    private TimeTracker timeTracker;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI lootText;
    [SerializeField] private Canvas winScreen;
    [SerializeField] private Image lightIndicator;
    private PlayerMovement playerMovement;
    private bool winScreenActive = false;
    private Image fadeScreen;

    private void Start()
    {
        winScreen.gameObject.SetActive(false);
        timeTracker = GameManager.Instance.timeTracker;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        
        // Create full-screen fade canvas
        CreateFadeScreen();
    }

    private void CreateFadeScreen()
    {
        // Create a new canvas for the fade effect
        GameObject canvasObj = new GameObject("FadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Ensure it's on top
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create the fade image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        fadeScreen = imageObj.AddComponent<Image>();
        fadeScreen.color = new Color(0, 0, 0, 0); // Transparent black
        
        // Set the image to fill the entire screen
        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void Update()
    {
        if (winScreenActive && Input.GetKeyDown(KeyCode.X))
        {
            SceneManager.LoadScene("LevelTwo");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (timeTracker != null && timeTracker.IsTracking() && GameManager.Instance.GetLoot() >= 300)
            {
                playerMovement.LockMovement();
                StartCoroutine(WinSequence());
            }
        }    
    }

    private IEnumerator WinSequence()
    {
        // Fade to black and fade out music simultaneously
        StartCoroutine(AudioManager.Instance.FadeOutMusic());
        AudioManager.Instance.StopAllAudioExceptMusic();
        yield return StartCoroutine(FadeToBlack());
        
        // Turn off light indicator
        lightIndicator.enabled = false;
        
        // Activate win screen and set text
        winScreen.gameObject.SetActive(true);
        timeText.text = timeTracker.GetFormattedTime();
        lootText.text = "$" + GameManager.Instance.GetLoot().ToString();
        
        // Play level complete music (starts immediately, no fade in)
        AudioManager.Instance.PlayLevelCompleteMusic();
        
        // Fade back in
        yield return StartCoroutine(FadeFromBlack());
        
        winScreenActive = true;
        timeTracker.TriggerWinCondition();
    }

    private IEnumerator FadeToBlack()
    {
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeScreen.color = new Color(0, 0, 0, 1); // Ensure fully black
    }

    private IEnumerator FadeFromBlack()
    {
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeScreen.color = new Color(0, 0, 0, 0); // Ensure fully transparent
    }
}
