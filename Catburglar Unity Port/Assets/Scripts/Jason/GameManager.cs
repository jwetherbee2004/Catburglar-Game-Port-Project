using UnityEngine;

public class GameManager : MonoBehaviour
{
    int loot = 0;
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
}
