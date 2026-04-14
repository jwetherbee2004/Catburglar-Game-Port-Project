using UnityEngine;
using System.Collections;

public class SecurityCamera : MonoBehaviour
{
    public Sprite[] cameraSprites;
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkingCoroutine;
    private float blinkInterval = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (blinkingCoroutine != null)
                StopCoroutine(blinkingCoroutine);
            blinkingCoroutine = StartCoroutine(BlinkCamera());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (blinkingCoroutine != null)
                StopCoroutine(blinkingCoroutine);
        }
        spriteRenderer.sprite = cameraSprites[0];
    }

    private IEnumerator BlinkCamera()
    {
        int spriteIndex = 0;
        while (true)
        {
            spriteRenderer.sprite = cameraSprites[spriteIndex];
            spriteIndex = (spriteIndex + 1) % cameraSprites.Length;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
