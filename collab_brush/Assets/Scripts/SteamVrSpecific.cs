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

using UnityEngine;
using Valve.VR;

namespace TiltBrush
{
    public class SteamOverlay : OverlayImplementation
    {
        private SteamVR_Overlay m_SteamVROverlay;

        [SerializeField] private float m_OverlayMaxAlpha = 1.0f;
        [SerializeField] private float m_OverlayMaxSize = 8;

        public override bool Enabled
        {
            get
            {
                return m_SteamVROverlay.gameObject.activeSelf;
            }
            set
            {
                m_SteamVROverlay.gameObject.SetActive(value);
            }
        }

        public SteamOverlay(SteamVR_Overlay so)
        {
            m_SteamVROverlay = so;
        }

        public override void Initialise()
        { }

        public override void SetTexture(Texture tex)
        {
            m_SteamVROverlay.texture = tex;
            m_SteamVROverlay.UpdateOverlay();
        }

        public override void SetAlpha(float ratio)
        {
            m_SteamVROverlay.alpha = ratio * m_OverlayMaxAlpha;
            Enabled = ratio > 0.0f;
        }

        public override void SetPosition(float distance, float height)
        {
            // place overlay in front of the player a distance out
            Vector3 vOverlayPosition = ViewpointScript.Head.position;
            Vector3 vOverlayDirection = ViewpointScript.Head.forward;
            vOverlayDirection.y = 0.0f;
            vOverlayDirection.Normalize();

            vOverlayPosition += (vOverlayDirection * distance);
            vOverlayPosition.y = height;
            m_SteamVROverlay.transform.position = vOverlayPosition;
            m_SteamVROverlay.transform.forward = vOverlayDirection;
        }

        public override void PauseRendering(bool pause)
        {
            SteamVR_Render.pauseRendering = pause;
        }

        protected override void FadeToCompositor(float fadeTime, bool fadeToCompositor)
        {
            SteamVR rVR = SteamVR.instance;
            if (rVR != null && rVR.compositor != null)
            {
                rVR.compositor.FadeGrid(fadeTime, fadeToCompositor);
            }
        }

        protected override void FadeBlack(float fadeTime, bool fadeToBlack)
        {
            SteamVR_Fade.Start(fadeToBlack ? Color.black : Color.clear, fadeTime);
        }
    }

}
