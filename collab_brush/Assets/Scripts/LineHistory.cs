using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TiltBrush;

public class LineHistory : MonoBehaviour
{
    private GameObject selected;
    private float timer;

    void Start()
    {
        timer = 0f;
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("SketchButtons"))) {
            
            if (selected == null || selected != hit.transform.gameObject) {
                selected = hit.transform.gameObject;
                timer = 0f;
            }
            else {
                timer += Time.deltaTime;

                if (timer >= 5.0f) {
                    GameObject.Find("HistoryManager").GetComponent<HistoryManager>().destroyGraph();
                    selected.GetComponentInChildren<LoadSketchButton>().OnButtonPressed2();
                    GameObject.Find("DataPrinter").GetComponent<TestPrinter>().PanelOut();
                    if (!GameObject.Find("HistoryManager").GetComponent<HistoryManager>().isLeaf()) {
                        GameObject.Find("DataPrinter").GetComponent<TestPrinter>().newBranch();
                    }
                }
            }
            
        }
        else {
            timer = 0f;
        }
    }
}
