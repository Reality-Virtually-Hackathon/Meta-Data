﻿// Copyright © 2017, Meta Company.  All rights reserved.
// 
// Redistribution and use of this software (the "Software") in binary form, without modification, is 
// permitted provided that the following conditions are met:
// 
// 1.      Redistributions of the unmodified Software in binary form must reproduce the above 
//         copyright notice, this list of conditions and the following disclaimer in the 
//         documentation and/or other materials provided with the distribution.
// 2.      The name of Meta Company (“Meta”) may not be used to endorse or promote products derived 
//         from this Software without specific prior written permission from Meta.
// 3.      LIMITATION TO META PLATFORM: Use of the Software is limited to use on or in connection 
//         with Meta-branded devices or Meta-branded software development kits.  For example, a bona 
//         fide recipient of the Software may incorporate an unmodified binary version of the 
//         Software into an application limited to use on or in connection with a Meta-branded 
//         device, while he or she may not incorporate an unmodified binary version of the Software 
//         into an application designed or offered for use on a non-Meta-branded device.
// 
// For the sake of clarity, the Software may not be redistributed under any circumstances in source 
// code form, or in the form of modified binary code – and nothing in this License shall be construed 
// to permit such redistribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL META COMPANY BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System.Threading;
using Meta.MetaAnalytics;
using Meta.Mouse;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meta
{
    /// <summary>
    /// Reports analytics events for the SDK.
    /// </summary>
    internal class MetaSdkAnalytics : IEventReceiver
    {
#if !NET_2_0_SUBSET
        private bool? _webcamEnabled = null;

        private IMetaAnalytics _metaAnalytics;

        public MetaSdkAnalytics()
        {
            _metaAnalytics = new MetaAnalytics.MetaAnalytics();
        }
#endif

        public void Init(IEventHandlers eventHandlers)
        {
#if !NET_2_0_SUBSET
            eventHandlers.SubscribeOnAwake(SceneStartAnalytics);
            eventHandlers.SubscribeOnApplicationQuit(SceneStopAnalytics);
            eventHandlers.SubscribeOnUpdate(OnUpdate);
#endif
        }

#if !NET_2_0_SUBSET
        private void OnUpdate()
        {
            //Prevents generating analytics when the scene is started
            if (_webcamEnabled == null)
            {
                _webcamEnabled = Meta.Plugin.Webcam.IsWebcamOn();
                return;
            }

            WebcamToggleAnalytics();
        }

        private void WebcamToggleAnalytics()
        {
            bool webcamEnabled = Meta.Plugin.Webcam.IsWebcamOn();
            if (webcamEnabled != _webcamEnabled)
            {
                _webcamEnabled = webcamEnabled;
                JObject o = new JObject();
                o["webcam_enabled"] = webcamEnabled;

                SendAsyncAnalytics("unity_webcamToggle", o);
            }
        }

        private void SceneStartAnalytics()
        {
            Scene s = SceneManager.GetActiveScene();
            string sceneName = s.name;

            bool handsInScene = GameObject.FindObjectOfType(typeof(HandsProvider)) != null;
            bool mouseInScene = GameObject.FindObjectOfType(typeof(MetaMouse)) != null;

            JObject o = new JObject();
            o["scene_identifier"] = sceneName;
            o["hands_present"] = handsInScene;
            o["mouse_present"] = mouseInScene;

            SendAsyncAnalytics("unity_sceneStarted", o);
        }

        private void SendAsyncAnalytics(string eventName, JObject o)
        {
            Thread t = new Thread(() => { _metaAnalytics.SendAnalytics(eventName, o.ToString()); });
            t.Start();
        }

        private void SceneStopAnalytics()
        {
            Scene s = SceneManager.GetActiveScene();
            string sceneName = s.name;
            JObject o = new JObject();
            o["scene_identifier"] = sceneName;
            _metaAnalytics.SendAnalytics("unity_sceneEnded", o.ToString());
        }
#endif
    }
}