using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System.Collections;
using TMPro;

public class TipPoint : MonoBehaviour
{
    public bool used = false;
    [SerializeField] public string tipText;
    [SerializeField] private GameObject tipBox;

    private void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public void SetTip()
    {
        tipBox.SetActive(true);
        tipBox.GetComponentInChildren<TextMeshProUGUI>().text = tipText;
        used = true;
        StartCoroutine(TipDisappear());
    }

    private IEnumerator TipDisappear()
    {
        yield return new WaitForSeconds(6f);
        tipBox.SetActive(false);
    }
}
