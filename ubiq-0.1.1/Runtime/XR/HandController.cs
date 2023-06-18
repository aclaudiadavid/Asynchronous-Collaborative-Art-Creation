﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SpatialTracking;
using UnityEngine.Events;
using UnityEngine.XR.Management;
using static UnityEngine.SpatialTracking.TrackedPoseDriver;

namespace Ubiq.XR
{
    public class HandController : Hand, IPrimaryButtonProvider, IMenuButtonProvider
    {
        private TrackedPoseDriver poseDriver;
        private List<InputDevice> controllers;
        private List<XRNodeState> nodes;
        private XRNode node;

        public ButtonEvent GripPress;
        public ButtonEvent TriggerPress;
        public SwipeEvent JoystickSwipe;

        [SerializeField]
        private ButtonEvent _PrimaryButtonPress;
        public ButtonEvent PrimaryButtonPress { get { return _PrimaryButtonPress; } }

        [SerializeField]
        private ButtonEvent _MenuButtonPress;
        public ButtonEvent MenuButtonPress { get { return _MenuButtonPress; } }

        public Vector2 Joystick;

        public bool GripState;
        public bool TriggerState;
        public bool PrimaryButtonState;

        private bool initialised;

        private void Awake()
        {
            poseDriver = GetComponent<TrackedPoseDriver>();
            controllers = new List<InputDevice>();
            nodes = new List<XRNodeState>();

            if (Right)
            {
                node = XRNode.RightHand;
            }
            if(Left)
            {
                node = XRNode.LeftHand;
            }

            initialised = false;
        }

        private InputDeviceCharacteristics GetSideCharacteristic(TrackedPose type)
        {
            switch (type)
            {
                case TrackedPose.LeftPose:
                    return InputDeviceCharacteristics.Left;
                case TrackedPose.RightPose:
                    return InputDeviceCharacteristics.Right;
                case TrackedPose.RemotePose:
                    return 0;
                default:
                    return 0;
            }
        }

        private void InitialiseHandDevices()
        {
            controllers.Clear();
            var collection = new List<InputDevice>();
            InputDevices.GetDevices(collection);
            foreach (var item in collection)
            {
                InputDevices_deviceConnected(item);

            }
            InputDevices.deviceConnected += InputDevices_deviceConnected;
            initialised = true;
        }

        private void InputDevices_deviceConnected(InputDevice device)
        {
            if ((device.characteristics & InputDeviceCharacteristics.Controller) == 0)
            {
                return;
            }
            if ((device.characteristics & InputDeviceCharacteristics.HeldInHand) == 0)
            {
                return;
            }
            if ((device.characteristics & GetSideCharacteristic(poseDriver.poseSource)) == 0)
            {
                return;
            }
            controllers.Add(device);
        }

        // Update is called once per frame
        void Update()
        {
            if (poseDriver.enabled)
            {
                if (!initialised)
                {
                    if (XRGeneralSettings.Instance && XRGeneralSettings.Instance.Manager.isInitializationComplete)
                    {
                        InitialiseHandDevices();
                    }
                }

                foreach (var item in controllers)
                {
                    item.TryGetFeatureValue(CommonUsages.triggerButton, out TriggerState);
                }

                foreach (var item in controllers)
                {
                    item.TryGetFeatureValue(CommonUsages.gripButton, out GripState);
                }

                foreach (var item in controllers)
                {
                    item.TryGetFeatureValue(CommonUsages.primaryButton, out PrimaryButtonState);
                }

                foreach (var item in controllers)
                {
                    item.TryGetFeatureValue(CommonUsages.primary2DAxis, out Joystick);
                }
            }

            TriggerPress.Update(TriggerState);
            GripPress.Update(GripState);
            PrimaryButtonPress.Update(PrimaryButtonState);
            JoystickSwipe.Update(Joystick.x);
        }

        public bool Left
        {
            get
            {
                return poseDriver.poseSource == TrackedPose.LeftPose;
            }
        }

        public bool Right
        {
            get
            {
                return poseDriver.poseSource == TrackedPose.RightPose;
            }
        }

        private void FixedUpdate()
        {
            InputTracking.GetNodeStates(nodes);
            foreach (var item in nodes)
            {
                if(item.nodeType == node)
                {
                    item.TryGetVelocity(out velocity);
                }
            }
        }


    }
}