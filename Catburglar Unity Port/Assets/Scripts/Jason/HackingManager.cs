using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class HackingManager : MonoBehaviour
{
    [SerializeField] private List<Slider> timerSliders;
    [SerializeField] private List<GameObject> arrowObjects;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private GameObject hackingUI;
    [SerializeField] private List<Sprite> completionFrames;
    [SerializeField] private Image completionImage;
    [SerializeField] private HackObject sceneHackable;
    [SerializeField] private AudioClip hackMusic;
    [SerializeField] private AudioClip hackCorrect;
    [SerializeField] private AudioClip hackIncorrect;
    private AudioSource audioSource;

    private int[] arrowList = new int[5] { 0, 0, 0, 0, 0 };
    private int index = 0;
    public bool ongoing = false;
    static public HackingManager instance;

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void StartHacking()
    {
        player.LockMovement();
        PopulateList();
        ongoing = true;
        index = 0;
        hackingUI.SetActive(true);
        StartCoroutine(OpenCloseHacking());
    }

    // Update is called once per frame
    void Update()
    {
        if (!ongoing)
        {
            return;
        }
        int direction = -1;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = 0;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = 1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = 2;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = 3;
        }

        if (direction == -1)
        {
            return;
        }

        if (arrowList[index] == direction)
        {
            audioSource.PlayOneShot(hackCorrect);
            index++;

            if (index >= 5)
            {
                ongoing = false;
                StartCoroutine(OpenCloseHacking());
                GameManager.Instance.DisplayObjective("Secondary objective complete.");
                sceneHackable.doneInteracting = true;
                StartCoroutine(GameManager.Instance.CreateDialogue(1));
                return;
            }

            completionImage.sprite = completionFrames[index];

            foreach (GameObject obj in arrowObjects)
            {
                obj.SetActive(false);
            }

            arrowObjects[arrowList[index]].SetActive(true);
        }
        else
        {
            audioSource.PlayOneShot(hackIncorrect);
        }
    }

    private void PopulateList()
    {
        for (int i = 0; i < arrowList.Length; i++)
        {
            arrowList[i] = Random.Range(0, 4);
        }
        foreach (GameObject obj in arrowObjects)
        {
            obj.SetActive(false);
        }
        arrowObjects[arrowList[index]].SetActive(true);
        completionImage.sprite = completionFrames[index];
    }

    private IEnumerator HackTimer()
    {
        float timer = 7f;
        float elapsed = 0f;
        while (elapsed < timer && ongoing)
        {
            elapsed += Time.deltaTime;
            foreach (Slider sl in timerSliders)
            {
                sl.value = 1-(elapsed / timer);
            }
            yield return null;
        }
        if (ongoing)
        {
            ongoing = false;
            StartCoroutine(OpenCloseHacking());
        }
    }

    private IEnumerator OpenCloseHacking()
    {
        float timer = 1f;
        float elapsed = 0f;
        Vector2 startPos = hackingUI.transform.position;
        Vector2 endPos;
        if (!ongoing)
        {
            endPos = new Vector2(startPos.x + 230, startPos.y);
        }
        else
        {
            audioSource.clip = hackMusic;
            audioSource.Play();
            endPos = new Vector2(startPos.x - 230, startPos.y);
        }
        while (elapsed < timer)
        {
            elapsed += Time.deltaTime;
            hackingUI.transform.position = Vector2.Lerp(startPos, endPos, elapsed/timer);
            yield return null;
        }
        if (ongoing)
        {
            StartCoroutine(HackTimer());
        } else
        {
            audioSource.Stop();
            player.UnlockMovement();
            hackingUI.SetActive(false);
        }
    }

}
