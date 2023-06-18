﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubiq.Rooms;

namespace Ubiq.Samples
{
    public class NewRoomButton : MonoBehaviour
    {
        public MainMenu mainMenu;
        public Text nameText;
        public bool publish;

        // Expected to be called by a UI element
        public void NewRoom ()
        {
            mainMenu.roomClient.JoinNew(nameText.text,publish);
        }

        public void NewRoomOB() {
            mainMenu = GameObject.Find("Menu").GetComponent<MainMenu>();
            mainMenu.roomClient.JoinNew(Random.Range(0, 1000).ToString(),false);
        }
    }
}
