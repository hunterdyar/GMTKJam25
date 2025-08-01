using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private int batteriesNeeded = 5;

    private float timer = 0f;
    private int batteryCount = 0;
    private bool isTiming = true;

    public static TimerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Update()
    {
        if (!isTiming) return;

        timer += Time.deltaTime;
        timerText.text = timer.ToString("F2") + "s";
    }

    public void BatteryCollected()
    {
        batteryCount++;

        if (batteryCount >= batteriesNeeded)
        {
            isTiming = false;
            Debug.Log("All batteries collected! Final time: " + timer.ToString("F2") + "s");
            // Add end-of-game logic here
        }
    }
}
