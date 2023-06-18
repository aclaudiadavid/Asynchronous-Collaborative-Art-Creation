using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PanelTimer : MonoBehaviour
{
    float startTime;
    public bool start = false;

    void Update()
    {
        if (start)
        {
            float currentTime = Time.time - startTime;
            float timeLeft = 15*60 - currentTime;
            int minutesLeft = (int)timeLeft / 60;
            int secondsLeft = (int)timeLeft % 60;
            int milisecondsLeft = (int)(timeLeft * 100) % 100;

            if (secondsLeft == 60)
            {
                secondsLeft = 0;
            }

            if (minutesLeft == 0 && secondsLeft == 0)
            {
                start = false;
            }

            GetComponent<TextMeshPro>().SetText(minutesLeft.ToString("00") + ":" + secondsLeft.ToString("00"));

            GameObject.Find("DataPrinter").GetComponent<TestPrinter>().SetTime(((int)currentTime/60).ToString("00") + ":" + ((int)currentTime%60).ToString("00") + "." + ((int)(currentTime * 100) % 100).ToString("00"));
        }  
    }

    public void StartTimer()
    {
        start = true;
        startTime = Time.time;
    }
}
