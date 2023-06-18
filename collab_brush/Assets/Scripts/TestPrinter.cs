using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using TiltBrush;

public class TestPrinter : MonoBehaviour
{
    public string currentTool;
    string prevTool;
    public bool isTool;
    int CounterStrokes = 0;
    bool firstTrigger = true;
    bool gripReady = true;
    float _time = 0f;
    float _time2 = 0f;
    string time;
    string timeLeft;
    string lastUpdate;
    bool gripActive;
    FileStream fC;
    FileStream fH;
    string fileControllerPath;
    string fileHeadEyePath;
    public string currentBrush;
    public Vector4 currentColor;
    public int currentUser;

    public bool panel = false;

    public bool timerStarted = false;

    PanelTimer panelTimer;

    int change;

    // Start is called before the first frame update
    void Start()
    {
        _time = 0f;
        _time2 = 0f;
        timeLeft = "00:00.00";
        currentBrush = "Light";
        currentColor = new Vector4(0.2f, 0.2f, 0.9f, 1.0f);
        currentUser = 1;
        change = 1;
        isTool = false;
        gripActive = false;
        panelTimer = GameObject.Find("PanelTimer").GetComponent<PanelTimer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gripActive) {
            timerStarted = panelTimer.start;
        }

        List<UnityEngine.XR.InputDevice> gameControllers = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithRole(UnityEngine.XR.InputDeviceRole.RightHanded, gameControllers);
        UnityEngine.XR.InputDevice deviceR = gameControllers[0];
        UnityEngine.XR.InputDevices.GetDevicesWithRole(UnityEngine.XR.InputDeviceRole.LeftHanded, gameControllers);
        UnityEngine.XR.InputDevice deviceL = gameControllers[0];

        ControllerPosition(deviceL, deviceR);

        List<UnityEngine.XR.InputDevice> HMD = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithRole(UnityEngine.XR.InputDeviceRole.Generic, HMD);
        UnityEngine.XR.InputDevice deviceH = HMD[0];
        HeadEyePosition(deviceH);
    }

    public void SetTime(string newTime) {
        timeLeft = newTime;
    }

    public void ChangeUser() {
        if (currentUser == 1) {
            currentUser = 2;
        } else if (currentUser == 2) {
            currentUser = 1;
        }

        change = 1;
        CounterStrokes = 0;
        firstTrigger = true;
    }

    public void UpdateBrush(string brush) {
        currentBrush = brush; 
    }

    public void UpdateColor(Color col) {
        currentColor = new Vector4((float)col.r, (float)col.g, (float)col.b, (float)col.a);
    }

    public void PanelHit() {
        panel = true;
    }

    public void PanelOut() {
        panel = false;
    }

    void confirmUser() {
        if (change == 1) {
            time = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            fileControllerPath = @"C:\Users\ursin\Documents\Open Brush\controllerDetails" + time + "_User" + currentUser + ".csv";
            fileHeadEyePath = @"C:\Users\ursin\Documents\Open Brush\headeyePos" + time + "_User" + currentUser + ".csv";

            File.Create(fileControllerPath).Close();
            File.Create(fileHeadEyePath).Close();

            change = 0;

            using (FileStream fC = new FileStream(fileControllerPath,FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fC))
            {
                sw.WriteLine("Time,Side,ControllerPosition,ControllerRotation,TriggerButton,Type,StrokeNum,Name,BrushType,BrushColor");
            }
            using (FileStream fC = new FileStream(fileHeadEyePath,FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fC))
            {
                sw.WriteLine("Time,HMDPosition,HMDRotation");
            }            
        }        
    }

    void ControllerPosition(UnityEngine.XR.InputDevice deviceL, UnityEngine.XR.InputDevice deviceR) {
        float interval = 0.1F;
        Vector3 devicePosL;
        Vector3 devicePosR;
        Quaternion deviceRotL;
        Quaternion deviceRotR;
        bool triggerL;
        bool triggerR;
        bool gripL;
        bool gripR;

        bool savedFirstTrigger = firstTrigger;


        if (!deviceL.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out devicePosL) || !deviceR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out devicePosR)) {
            return;
        }
        if (!deviceL.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out deviceRotL) || !deviceR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out deviceRotR)) {
            return;
        }
        if (!deviceL.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerL) || !deviceR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerR)) {
            return;
        }
        if (!deviceL.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripL) || !deviceR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripR)) {
            return;
        }

        firstTrigger = !triggerR;

        
        _time2 += Time.deltaTime;
        if ((timerStarted && _time2 >= interval) || (triggerR && savedFirstTrigger && !panel && timerStarted)) {
            _time2 = 0;
            confirmUser();
            //print position to file
            if (gripActive) {
                using (fC = new FileStream(fileControllerPath,FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fC))
                {
                    sw.WriteLine(timeLeft + ",Left,(" + String.Format("{0:0.000000}", devicePosL.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosL.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosL.z).Replace(",", ".") + "),(" 
                    + String.Format("{0:0.000000}", deviceRotL.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotL.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotL.z).Replace(",", ".") + "; " +  String.Format("{0:0.000000}", deviceRotL.w).Replace(",", ".") + ")," 
                    + triggerL + ",Tool,,Resize,,");
                }            
            }
            else {
                using (FileStream fC = new FileStream(fileControllerPath,FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fC))
                {
                    sw.WriteLine(timeLeft + ",Left,(" + String.Format("{0:0.000000}", devicePosL.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosL.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosL.z).Replace(",", ".") + "),(" 
                    + String.Format("{0:0.000000}", deviceRotL.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotL.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotL.z).Replace(",", ".") + "; " +  String.Format("{0:0.000000}", deviceRotL.w).Replace(",", ".") + ")," 
                    + triggerL + ",,,,,");
                }
            }
 
            //right controller position
            if (triggerR && !panel) {
                using (FileStream fC = new FileStream(fileControllerPath,FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fC))
                {
                    if (!isTool) {
                        if (savedFirstTrigger) {
                            CounterStrokes += 1;
                        }
                        Debug.Log(CounterStrokes);
                        sw.WriteLine(timeLeft + ",Right,(" + String.Format("{0:0.000000}", devicePosR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.z).Replace(",", ".") + "),(" 
                        + String.Format("{0:0.000000}", deviceRotR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.z).Replace(",", ".") + "; " +  String.Format("{0:0.000000}", deviceRotR.w).Replace(",", ".") + ")," 
                        + triggerR + ",Stroke," + CounterStrokes + ",," + currentBrush + "," + currentColor.ToString().Replace(",", ";"));
                    }
                    else {
                        sw.WriteLine(timeLeft + ",Right,(" + String.Format("{0:0.000000}", devicePosR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.z).Replace(",", ".") + "),(" 
                        + String.Format("{0:0.000000}", deviceRotR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.z).Replace(",", ".") + "; " +  String.Format("{0:0.000000}", deviceRotR.w).Replace(",", ".") + "),"
                        + triggerR + ",Tool,," + currentTool + ",,");
                    }
                
                }
            }
            else if (gripActive) {
                using (fC = new FileStream(fileControllerPath,FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fC))
                {
                    sw.WriteLine(timeLeft + ",Right,(" + String.Format("{0:0.000000}", devicePosR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.z).Replace(",", ".") + "),(" 
                    + String.Format("{0:0.000000}", deviceRotR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.z).Replace(",", ".") + "; " +  String.Format("{0:0.000000}", deviceRotR.w).Replace(",", ".") + ")," 
                    + triggerR + ",Tool,,Resize,,");
                }   
            }
            else if (!triggerR && !gripActive && isTool && (currentTool == "BrushSize")) {
                using (fC = new FileStream(fileControllerPath,FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fC))
                {
                    sw.WriteLine(timeLeft + ",Right,(" + String.Format("{0:0.000000}", devicePosR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.z).Replace(",", ".") + "),(" 
                    + String.Format("{0:0.000000}", deviceRotR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.z).Replace(",", ".") + "; " +  String.Format("{0:0.000000}", deviceRotR.w).Replace(",", ".") + ")," 
                    + triggerR + ",Tool,,BrushSize,,");
                } 
            } 
            else {
                using (FileStream fC = new FileStream(fileControllerPath,FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fC))
                {
                    sw.WriteLine(timeLeft + ",Right,(" + String.Format("{0:0.000000}", devicePosR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.z).Replace(",", ".") + "),(" 
                    + String.Format("{0:0.000000}", deviceRotR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.z).Replace(",", ".") + "; " +  String.Format("{0:0.000000}", deviceRotR.w).Replace(",", ".") + ")," 
                    + triggerR + ",,,,,");
                }
            }   
        }
        
        if ((gripL || gripR) && gripReady && timerStarted)
        {
            prevTool = currentTool;
            _time2 = 0;
            gripReady = false;
            gripActive = true;
            currentTool = "Resize";
            confirmUser();

            using (fC = new FileStream(fileControllerPath,FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fC))
            {
                sw.WriteLine(timeLeft + ",Left,(" + String.Format("{0:0.000000}", devicePosL.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosL.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosL.z).Replace(",", ".") + "),(" 
                + String.Format("{0:0.000000}", deviceRotL.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotL.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotL.z).Replace(",", ".") + "; " +  String.Format("{0:0.000000}", deviceRotL.w).Replace(",", ".") + ")," 
                + triggerL + ",Tool,,Resize,,");
                
                sw.WriteLine(timeLeft + ",Right,(" + String.Format("{0:0.000000}", devicePosR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePosR.z).Replace(",", ".") + "),(" 
                + String.Format("{0:0.000000}", deviceRotR.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRotR.z).Replace(",", ".") + "; " +  String.Format("{0:0.000000}", deviceRotR.w).Replace(",", ".") + ")," 
                + triggerR + ",Tool,,Resize,,");
            }
        }  
        //ready var for next press
        else if (!(gripL || gripR))
        {
            if (gripActive) {
                currentTool = prevTool;
            }
            gripReady = true;
            gripActive = false;
        }
    }

    public void PrintAction(String currentAction) {
        if (timerStarted) {
            confirmUser();
            using (fC = new FileStream(fileControllerPath,FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fC))
            {
                sw.WriteLine(timeLeft + ",,,,,Action,," + currentAction + ",,");
            }
        }
    }

    void HeadEyePosition(UnityEngine.XR.InputDevice device) 
    {
        float interval = 1.0F;
        Vector3 devicePos;
        Quaternion deviceRot;

        //periodic print
        _time += Time.deltaTime;
        if (change == 0) {
            if (_time >= interval) {        
                //Head position and rotation
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out devicePos) && timerStarted)
                {
                    if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out deviceRot)) {
                        using (FileStream fH = new FileStream(fileHeadEyePath,FileMode.Append, FileAccess.Write))
                        using (StreamWriter ssw = new StreamWriter(fH))
                        {
                            ssw.WriteLine(timeLeft + ",(" + String.Format("{0:0.000000}", devicePos.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePos.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", devicePos.z).Replace(",", ".") + "),(" 
                            + String.Format("{0:0.000000}", deviceRot.x).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRot.y).Replace(",", ".") + "; " + String.Format("{0:0.000000}", deviceRot.z).Replace(",", ".") + "; " +  String.Format("{0:0.000000}", deviceRot.w).Replace(",", ".") + ")");
                        }
                    }
                }
                _time = 0;
            }
        }
    }

    public void newBranch() {
        if (timerStarted) {
            confirmUser();
            using (fC = new FileStream(fileControllerPath,FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fC))
            {
                sw.WriteLine(timeLeft + ",,,,,New,,,,");
            }
        }
    }
}
