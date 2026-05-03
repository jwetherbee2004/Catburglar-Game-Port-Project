using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lootText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private GameObject dialogueObject;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private float lootAnimationDuration = 1f;
    [SerializeField] private Vector2 lootDisplayOffset = new Vector2(70, -5);
    [SerializeField] private float lootDisplayDistance = 24f;
    [SerializeField] private Vector2 lootVisiblePosition = new Vector2(10, 50);
    [SerializeField] private float lootDisplayHoldTime = 1f;
    
    private int loot = 0;
    private int level = 0;
    private int[] lootRequirements = { 300, 0, 0 };
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
        DisplayObjective();
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

    public int GetLoot()
    {
        return loot;
    }

    public void AddMoney(int amount)
    {
        loot += amount;
        moneyText.text = "$" + loot;
        StartCoroutine(MoneyTextAnimation());
    }

    private IEnumerator MoneyTextAnimation()
    {
        RectTransform lootRect = lootText.rectTransform;
        RectTransform moneyRect = moneyText.rectTransform;

        lootText.gameObject.SetActive(true);
        moneyText.gameObject.SetActive(true);

        Vector2 lootStartPos = lootVisiblePosition + Vector2.up * lootDisplayDistance;

        float elapsed = 0f;
        while (elapsed < lootAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lootAnimationDuration);

            lootRect.anchoredPosition = Vector2.Lerp(lootStartPos, lootVisiblePosition, t);
            moneyRect.anchoredPosition = lootRect.anchoredPosition + lootDisplayOffset;

            yield return null;
        }
        yield return new WaitForSeconds(lootDisplayHoldTime);

        lootRect.anchoredPosition = lootStartPos;
        moneyRect.anchoredPosition = lootStartPos + lootDisplayOffset;
        lootText.gameObject.SetActive(false);
        moneyText.gameObject.SetActive(false);
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

    private void DisplayObjective()
    {
        goalText.text = "Steal at least $" + lootRequirements[level] + " worth of loot.";
        StartCoroutine(ObjectiveAnimation());
    }

    private IEnumerator ObjectiveAnimation()
    {
        yield return new WaitForSeconds(7f);
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = new Vector3(objectiveText.transform.position.x, objectiveText.transform.position.y, 0);
        Vector3 endPos = new Vector3(objectiveText.transform.position.x, objectiveText.transform.position.y - 40, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            objectiveText.transform.position = Vector3.Lerp(startPos, endPos, t);
            goalText.transform.position = new Vector3(objectiveText.transform.position.x + 140, objectiveText.transform.position.y-6, 0);

            yield return null;
        }

    }

    public void DisplayDialogue(string dialogue)
    {
        dialogueObject.SetActive(true);
        dialogueText.text = dialogue;
        // player.WaitForMovementLock();
    }
    
    private void PlayLevelReset()
    {
        if (audioSource != null && levelReset != null)
        {
            audioSource.PlayOneShot(levelReset);
        }
    }
}
