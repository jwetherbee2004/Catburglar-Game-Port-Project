using UnityEngine;

public class GameManager : MonoBehaviour
{
    int loot = 0;
    public int lightCount = 0;
    public bool inLight => lightCount > 0;
    public bool isHidden;
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddMoney(int amount)
    {
        loot += amount;
        Debug.Log("Loot: $" +  loot);
    }

    public void EnterLight()
    {
        lightCount++;
        Debug.Log("Entered light. Light count: " + lightCount);
    }

    public void ExitLight()
    {
        lightCount = Mathf.Max(0, lightCount - 1);
        Debug.Log("Exited light. Light count: " + lightCount);
    }

    public void SetHidden(bool hidden)
    {
        isHidden = hidden;
        Debug.Log("Player is " + (isHidden ? "hidden" : "visible"));
    }
}
