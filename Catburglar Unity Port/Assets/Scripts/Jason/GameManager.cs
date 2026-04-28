using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    int loot = 0;
    public int lightCount = 0;
    public bool inLight => lightCount > 0;
    public bool isHidden;
    public static GameManager Instance;
    private bool isResettingLevel = false;
    public TimeTracker timeTracker { get; private set; }

    [Header("Audio")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip levelReset;
    [SerializeField] private AudioClip inLightSound;
    [SerializeField] private AudioClip outOfLightSound;

    [Header("Light Indicator")]
    [SerializeField] private Image Eye1;
    [SerializeField] private Image Eye2;
    [SerializeField] private TextMeshProUGUI LightText;

    private void Awake()
    {
        Instance = this;
        
        timeTracker = GetComponent<TimeTracker>();
        if (timeTracker == null)
        {
            timeTracker = gameObject.AddComponent<TimeTracker>();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(inLight)
        {
            Eye1.gameObject.SetActive(true);
            Eye2.gameObject.SetActive(true);
            LightText.text = "IN LIGHT";
        }
        else
        {
            Eye1.gameObject.SetActive(false);
            Eye2.gameObject.SetActive(false);
            LightText.text = "IN SHADOW";
        }
    }

    public void AddMoney(int amount)
    {
        loot += amount;
        Debug.Log("Loot: $" +  loot);
    }

    public int GetLoot()
    {
        return loot;
    }

    public void EnterLight()
    {
        lightCount++;
        Debug.Log("Entered light. Light count: " + lightCount);

        if (audioSource != null && inLightSound != null && lightCount == 1)
        {
            audioSource.PlayOneShot(inLightSound);
        }
    }

    public void ExitLight()
    {
        lightCount = Mathf.Max(0, lightCount - 1);
        Debug.Log("Exited light. Light count: " + lightCount);
        if (audioSource != null && outOfLightSound != null && lightCount == 0)
        {
            audioSource.PlayOneShot(outOfLightSound);
        }
    }

    public void SetHidden(bool hidden)
    {
        isHidden = hidden;
        Debug.Log("Player is " + (isHidden ? "hidden" : "visible"));
    }

    public void ResetLevel()
    {
        if (isResettingLevel) return;
        isResettingLevel = true;
        
        // Pause music before level reset
        AudioManager.Instance.PauseMusic();
        
        PlayLevelReset();
        StartCoroutine(FadeAndRestart());
    }

    private IEnumerator FadeAndRestart()
    {
        float duration = 1f;
        float elapsed;

        foreach (Camera cam in Camera.allCameras)
        {
            UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null) camData.renderPostProcessing = true;
        }

        // --- Phase 1: desaturate to black & white via a runtime URP Volume ---
        GameObject volumeGO = new GameObject("BWVolume");
        Volume volume = volumeGO.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 100;
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.profile = profile;
        ColorAdjustments colorAdj = profile.Add<ColorAdjustments>(true);
        colorAdj.saturation.overrideState = true;
        colorAdj.saturation.value = 0f;

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            colorAdj.saturation.value = Mathf.Lerp(0f, -100f, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        // --- Phase 2: fade to black ---
        GameObject canvasGO = new GameObject("FadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);

        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        Image fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        Time.timeScale = 1f;
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Reloading scene: " + sceneName);
        
        DestroyImmediate(canvasGO);
        DestroyImmediate(volumeGO);
        
        SceneManager.LoadScene(sceneName);
    }

    private void PlayLevelReset()
    {
        if (audioSource != null && levelReset != null)
        {
            audioSource.PlayOneShot(levelReset);
        }
    }
}
