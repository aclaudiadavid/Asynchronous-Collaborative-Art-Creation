using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TiltBrush {
    public class PanelTimer : MonoBehaviour
    {
        float startTime;
        public bool start = false;
        private AudioSource audioSource;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (start)
            {
                float currentTime = Time.time - startTime;
                float timeLeft = 15*60 - currentTime;
                int minutesLeft = (int)timeLeft / 60;
                int secondsLeft = (int)timeLeft % 60;
                int milisecondsLeft = (int)(timeLeft * 100) % 100;

                if (minutesLeft == 0 && secondsLeft == 0)
                {
                    audioSource.Play();
                    start = false;
                }

                if (minutesLeft == 0 && secondsLeft == 30)
                {
                    audioSource.Play();
                }

                GetComponent<TextMeshPro>().SetText(minutesLeft.ToString("00") + ":" + secondsLeft.ToString("00"));

                GameObject.Find("DataPrinter").GetComponent<TestPrinter>().SetTime(((int)currentTime/60).ToString("00") + ":" + ((int)currentTime%60).ToString("00") + "." + ((int)(currentTime * 100) % 100).ToString("00"));
            }  
        }

        public void StartTimer()
        {
            start = true;
            startTime = Time.time;
            audioSource.Play();
        }
    }
}