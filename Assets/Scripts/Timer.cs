using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float startTime = 0f;
    public bool isRunning = true;

    private float timer;

    void Start()
    {
        timer = startTime;
    }

    void Update()
    {
        if (!isRunning) return;

        timer += Time.deltaTime;
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ResetTimer()
    {
        timer = 0f;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void StartTimer()
    {
        isRunning = true;
    }
}
