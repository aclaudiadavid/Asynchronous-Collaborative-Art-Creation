using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TiltBrush;
using Ubiq.Samples;
using TMPro;

public class LineUbiq : MonoBehaviour
{
    private GameObject selected;
    private float timer;
    GameObject joinUbiqButton;
    GameObject exitUbiqButton;

    void Start()
    {
        timer = 0f;
    }

    void Update()
    {
        if (joinUbiqButton == null || exitUbiqButton == null) {
            joinUbiqButton = SketchControlsScript.m_Instance.joinUbiqButton;
            exitUbiqButton = SketchControlsScript.m_Instance.exitUbiqButton;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("UbiqButtons"))) {
            
            if (selected == null || selected != hit.transform.gameObject) {
                selected = hit.transform.gameObject;
                timer = 0f;
            }
            else {                
                timer += Time.deltaTime;

                if (timer >= 3.0f) {
                    Debug.Log("Ubiq room loading...");
                    GameObject.Find("UbiqRoomUI").GetComponent<UbiqRoomUI>().destroyRoomsUI();

                    Debug.Log("Room code: " + selected.GetComponentInChildren<TextMeshPro>().text);

                    if (selected.name.StartsWith("Join")) {
                        joinUbiqButton.GetComponent<JoinRoomButton>().JoinRoom(selected.GetComponentInChildren<TextMeshPro>().text);
                    }
                    else {
                        joinUbiqButton.GetComponent<JoinRoomButton>().JoinNewRoom(selected.GetComponentInChildren<TextMeshPro>().text.Split('\n')[0]);
                    }

                    SketchControlsScript.m_Instance.IssueGlobalCommand(SketchControlsScript.GlobalCommands.NewSketch);

                    /*selected.GetComponentInChildren<LoadSketchButton>().OnButtonPressed2();*/
                    GameObject.Find("DataPrinter").GetComponent<TestPrinter>().PanelOut();

                    joinUbiqButton.SetActive(false);
                    exitUbiqButton.SetActive(true);
                }
            }
            
        }
        else {
            timer = 0f;
        }
    }
}
