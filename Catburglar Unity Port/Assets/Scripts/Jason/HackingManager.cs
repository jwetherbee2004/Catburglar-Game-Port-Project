using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;

public class HackingManager : MonoBehaviour
{
    [SerializeField] private List<Slider> timerSliders;
    [SerializeField] private List<GameObject> arrowObjects;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private GameObject hackingUI;
    private int[] arrowList = new int[5] { 0, 0, 0, 0, 0 };
    private int index = 0;
    private bool ongoing = false;
    static public HackingManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void StartHacking()
    {
        player.LockMovement();
        index = 0;
        hackingUI.SetActive(true);
        PopulateList();
        ongoing = true;
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
        if (arrowList[index] == direction)
        {
            index++;
            if (index >= 4)
            {
                EndHacking();
                return;
            }
            foreach (GameObject obj in arrowObjects)
            {
                obj.SetActive(false);
            }
            arrowObjects[arrowList[index]].SetActive(true);
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
    }

    private void EndHacking()
    {
        ongoing = false;
        player.UnlockMovement();
        hackingUI.SetActive(false);
    }
}
