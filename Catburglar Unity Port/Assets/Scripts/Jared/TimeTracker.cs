using UnityEngine;

public class TimeTracker : MonoBehaviour
{
    private float elapsedTime = 0f;
    private bool isTracking = true;
    private string formattedTime = "00:00.000";

    private void Start()
    {
        elapsedTime = 0f;
    }

    private void Update()
    {
        if (isTracking)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    public void TriggerWinCondition()
    {
        if (!isTracking) return;

        isTracking = false;
        formattedTime = FormatTime(elapsedTime);
    }

    private string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        int secs = (int)(seconds % 60f);
        int milliseconds = (int)((seconds % 1f) * 1000f);

        return $"{minutes:D2}:{secs:D2}.{milliseconds:D3}";
    }

    public string GetFormattedTime()
    {
        if (isTracking)
        {
            return FormatTime(elapsedTime);
        }
        return formattedTime;
    }

    public float GetElapsedSeconds()
    {
        return elapsedTime;
    }

    public bool IsTracking()
    {
        return isTracking;
    }
}
