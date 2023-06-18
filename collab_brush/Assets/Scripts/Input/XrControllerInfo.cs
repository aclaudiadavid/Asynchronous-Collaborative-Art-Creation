// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using UnityEngine;
using UnityEngine.XR;

namespace TiltBrush
{

    // Unity XR Controller.
    // - https://docs.unity3d.com/2019.4/Documentation/ScriptReference/XR.CommonUsages.html
    public class XrControllerInfo : ControllerInfo
    {
        private InputDeviceCharacteristics m_characteristics;
        private InputDevice m_device;
        private uint m_currInputState = 0u;
        private uint m_prevInputState = 0u;

        private const float kDeadzoneTolerance = 0.01f;

        public override bool IsTrackedObjectValid { get; set; }

        public XrControllerInfo(BaseControllerBehavior behavior, bool isLeftHand) : base(behavior)
        {
            InputDevices.deviceConnected += OnDeviceConnected;
            InputDevices.deviceDisconnected += OnDeviceDisconnected;

            m_characteristics = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice;

            m_characteristics |= isLeftHand ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right;
        }

        ~XrControllerInfo()
        {
            InputDevices.deviceConnected -= OnDeviceConnected;
            InputDevices.deviceDisconnected -= OnDeviceDisconnected;
        }

        private void OnDeviceConnected(InputDevice device)
        {
            if (!m_device.isValid && (device.characteristics & m_characteristics) == m_characteristics)
            {
                m_device = device;
                App.Log($"XrController connected: {device.manufacturer} - {device.name} - {device.serialNumber}");
            }
        }

        private void OnDeviceDisconnected(InputDevice device)
        {
            // If this is the same device we are tracking, reset it to make it invalid.
            if (m_device.isValid && (device.characteristics & m_characteristics) == m_characteristics
                && device.name == m_device.name && device.serialNumber == m_device.serialNumber)
            {
                m_device = new InputDevice(); // reset
                IsTrackedObjectValid = false;
                App.Log($"XrController disconnected: {device.manufacturer} - {device.name} - {device.serialNumber}");
            }
        }

        // Update device 
        public void UpdatePoses()
        {
            IsTrackedObjectValid = m_device.isValid;

            if (m_device.isValid)
            {
                Transform t = Behavior.transform;

                Vector3 position;
                if (m_device.TryGetFeatureValue(CommonUsages.devicePosition, out position))
                {
                    t.localPosition = position;
                }

                Quaternion rotation;
                if (m_device.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation))
                {
                    t.localRotation = rotation;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            UpdatePoses();

            m_prevInputState = m_currInputState;
            m_currInputState = 0u;
        }

        // TODO: Do we need this, and GetTriggerValue() ?
        public override float GetTriggerRatio()
        {
            return GetTriggerValue();
        }

        public override Vector2 GetPadValue()
        {
            return GetThumbStickValue();

            // Vector2 value;
            // if (m_device.isValid && m_device.TryGetFeatureValue(CommonUsages.secondary2DAxis, out value))
            // {
            //     return value;
            // }
            // return Vector2.zero;
        }

        public override Vector2 GetThumbStickValue()
        {
            Vector2 value;
            if (m_device.isValid && m_device.TryGetFeatureValue(CommonUsages.primary2DAxis, out value))
            {
                return value;
            }

            return Vector2.zero;
        }

        public override Vector2 GetPadValueDelta()
        {
            return new Vector2(GetScrollXDelta(), GetScrollYDelta());

            // Vector2 value;
            // if (m_device.isValid && m_device.TryGetFeatureValue(CommonUsages.secondary2DAxis, out value))
            // {
            //     return value;
            // }
            //
            // return Vector2.zero;
        }

        public override float GetGripValue()
        {
            float value;
            if (m_device.isValid && m_device.TryGetFeatureValue(CommonUsages.grip, out value))
            {
                return value;
            }

            return 0.0f;
        }

        public override float GetTriggerValue()
        {
            float value;
            if (m_device.isValid && m_device.TryGetFeatureValue(CommonUsages.trigger, out value))
            {
                // If we don't have a deadzone the trigger can return very small values that leave the tools latched on.
                return value <= kDeadzoneTolerance ? 0.0f : value;
            }

            return 0.0f;
        }

        public override float GetScrollXDelta()
        {
            if (IsTrackedObjectValid)
            {
                return GetThumbStickValue().x;
            }

            return 0.0f;
        }

        public override float GetScrollYDelta()
        {
            if (IsTrackedObjectValid)
            {
                return GetThumbStickValue().y;
            }

            return 0.0f;
        }

        public override bool GetVrInputTouch(VrInput input)
        {
            bool value = false;
            if (m_device.isValid)
            {
                switch (input)
                {
                    case VrInput.Button01:
                    case VrInput.Button04:
                    case VrInput.Button06:
                        if (m_device.TryGetFeatureValue(CommonUsages.primaryTouch, out value))
                        {
                            return value;
                        }
                        break;
                    case VrInput.Button02:
                    case VrInput.Button03:
                    case VrInput.Button05:
                        if (m_device.TryGetFeatureValue(CommonUsages.secondaryTouch, out value))
                        {
                            return value;
                        }
                        break;
                    case VrInput.Directional:
                    case VrInput.Thumbstick:
                    case VrInput.Touchpad:
                        if (m_device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out value))
                        {
                            return value;
                        }
                        break;
                    case VrInput.Any:
                        if (m_device.TryGetFeatureValue(CommonUsages.primaryTouch, out value) && value)
                            return true;
                        if (m_device.TryGetFeatureValue(CommonUsages.secondaryTouch, out value) && value)
                            return true;
                        if (m_device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out value) && value)
                            return true;
                        break;
                    default:
                        throw new NotImplementedException($"Invalid touch button enum: {input.ToString()}");
                }

            }

            return false;
        }

        public override bool GetVrInput(VrInput input)
        {
            bool value = false;

            if (m_device.isValid)
            {
                switch (input)
                {
                    case VrInput.Directional:
                    case VrInput.Thumbstick:
                    case VrInput.Touchpad:
                        if (m_device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out value))
                        {
                            return value;
                        }
                        break;

                    case VrInput.Trigger:
                        if (m_device.TryGetFeatureValue(CommonUsages.triggerButton, out value))
                        {
                            return value;
                        }
                        break;

                    case VrInput.Grip:
                        if (m_device.TryGetFeatureValue(CommonUsages.gripButton, out value))
                        {
                            return value;
                        }
                        break;

                    case VrInput.Button01:
                    case VrInput.Button04:
                    case VrInput.Button06:
                        if (m_device.TryGetFeatureValue(CommonUsages.primaryButton, out value))
                        {
                            return value;
                        }
                        break;

                    case VrInput.Button02:
                    case VrInput.Button03:
                    case VrInput.Button05:
                        if (m_device.TryGetFeatureValue(CommonUsages.secondaryButton, out value))
                        {
                            return value;
                        }
                        break;

                    case VrInput.Any:
                        if (m_device.TryGetFeatureValue(CommonUsages.primaryButton, out value) && value)
                            return true;
                        if (m_device.TryGetFeatureValue(CommonUsages.secondaryButton, out value) && value)
                            return true;
                        if (m_device.TryGetFeatureValue(CommonUsages.triggerButton, out value) && value)
                            return true;
                        if (m_device.TryGetFeatureValue(CommonUsages.gripButton, out value) && value)
                            return true;
                        if (m_device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out value) && value)
                            return true;
                        break;
                    default:
                        throw new NotImplementedException($"Invalid button enum: {input.ToString()}");
                }
            }

            return false;
        }

        public override bool GetVrInputDown(VrInput input)
        {
            uint flag = 1u << (int)input;

            if (GetVrInput(input))
            {
                m_currInputState |= flag;
                return true; // (m_prevInputState & flag) == 0;
            }

            return false;
        }

        public override void TriggerControllerHaptics(float seconds)
        {
            // TODO-XR - Do controller haptics!
        }
    }
}
