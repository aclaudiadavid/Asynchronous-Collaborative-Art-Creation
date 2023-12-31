// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// This script only works in Unity 5.6 or newer since older versions of Unity
// don't have IPreprocessBuild and IPostprocessBuild.
#if UNITY_5_6_OR_NEWER && (UNITY_ANDROID || UNITY_IOS)
using UnityEngine;
using UnityEngine.VR;
using UnityEditor;
using UnityEditor.Build;
using System.Linq;

// Notifes users if they build for Android or iOS without Cardboard or Daydream enabled.
// class GvrBuildProcessor : IPreprocessBuild {
//   private const string VR_SDK_DAYDREAM = "daydream";
//   private const string VR_SDK_CARDBOARD = "cardboard";
//   private const string VR_SDK_OCULUS = "Oculus";
//   private const string VR_SETTINGS_NOT_ENABLED_ERROR_MESSAGE_FORMAT =
//     "On {0} 'Player Settings > Virtual Reality Supported' setting must be checked.\n" +
//     "Please fix this setting and rebuild your app.";
//   private const string IOS_MISSING_GVR_SDK_ERROR_MESSAGE =
//     "On iOS 'Player Settings > Virtual Reality SDKs' must include 'Cardboard'.\n" +
//     "Please fix this setting and rebuild your app.";
//   private const string ANDROID_MISSING_GVR_SDK_ERROR_MESSAGE =
//     "On Android 'Player Settings > Virtual Reality SDKs' must include" +
//     "'Daydream', 'Cardboard', or 'Oculus'.\n" +
//     "Please fix this setting and rebuild your app.";
//
//   public int callbackOrder {
//     get { return 0; }
//   }
//
//   public void OnPreprocessBuild (BuildTarget target, string path)
//   {
//     if (target != BuildTarget.Android && target != BuildTarget.iOS) {
//       // Do nothing when not building for Android or iOS.
//       return;
//     }
//
//     // 'Player Settings > Virtual Reality Supported' must be enabled.
//     if (!IsVRSupportEnabled()) {
//       Debug.LogErrorFormat(VR_SETTINGS_NOT_ENABLED_ERROR_MESSAGE_FORMAT, target);
//     }
//
//     if (target == BuildTarget.Android) {
//       // On Android VR SDKs must include 'Daydream' and/or 'Cardboard' and/or 'Oculus'.
//       if (!IsDaydreamSDKIncluded() && !IsCardboardSDKIncluded() && !IsOculusSDKIncluded()) {
//         Debug.LogError(ANDROID_MISSING_GVR_SDK_ERROR_MESSAGE);
//       }
//     }
//
//     if (target == BuildTarget.iOS) {
//       // On iOS VR SDKs must include 'Cardboard'.
//       if (!IsCardboardSDKIncluded()) {
//         Debug.LogError(IOS_MISSING_GVR_SDK_ERROR_MESSAGE);
//       }
//     }
//   }
//
//   // 'Player Settings > Virtual Reality Supported' enabled?
//   private bool IsVRSupportEnabled() {
//     return PlayerSettings.virtualRealitySupported;
//   }
//
//   // 'Player Settings > Virtual Reality SDKs' includes 'Daydream'?
//   private bool IsDaydreamSDKIncluded() {
//     return UnityEngine.XR.XRSettings.supportedDevices.Contains(VR_SDK_DAYDREAM);
//   }
//
//   // 'Player Settings > Virtual Reality SDKs' includes 'Cardboard'?
//   private bool IsCardboardSDKIncluded() {
//     return UnityEngine.XR.XRSettings.supportedDevices.Contains(VR_SDK_CARDBOARD);
//   }
//
//   // 'Player Settings > Virtual Reality SDKs' includes 'Oculus'?
//   private bool IsOculusSDKIncluded() {
//     return UnityEngine.XR.XRSettings.supportedDevices.Contains(VR_SDK_OCULUS);
//   }
// }
#endif  // UNITY_5_6_OR_NEWER && (UNITY_ANDROID || UNITY_IOS)