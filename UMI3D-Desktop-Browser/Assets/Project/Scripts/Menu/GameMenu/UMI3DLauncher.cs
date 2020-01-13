/*
Copyright 2019 Gfi Informatique

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using umi3d.cdk;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BrowserDesktop.Controller;

namespace BrowserDesktop.Menu
{
    public class UMI3DLauncher : MonoBehaviour
    {
        [Serializable]
        public class StartData
        {
            public string ip = null;
            public string port = null;
            public string keyboard = "";
        }
        public string scene;
        public string thisScene;
        public string xmlPath = "start.xml";

        string KeyBoardValue = "";

        public InputField _ip;
        public InputField _port;
        public Dropdown keyboardMode;
        public Text _version;

        // Start is called before the first frame update
        void Start()
        {
            LoadXml();
            _version.text = umi3d.UMI3DVersion.version;
            InputLayoutManager.OnLayoutLoaded.AddListener(layoutChanged);
            if (InputLayoutManager.LayoutLoaded) layoutChanged();
        }

        public void LoadXml()
        {
            var serializer = new XmlSerializer(typeof(StartData));
            if (File.Exists(xmlPath))
            {
                var stream = new FileStream(xmlPath, FileMode.Open);
                var xml = serializer.Deserialize(stream) as StartData;
                _ip.text = xml.ip;
                _port.text = xml.port;
                KeyBoardValue = xml.keyboard;
                stream.Close();
            }
        }

        public void Run()
        {
            var serializer = new XmlSerializer(typeof(StartData));
            var stream = new FileStream(xmlPath, FileMode.Create);
            var xml = new StartData()
            {
                ip = _ip.text,
                port = _port.text,
                keyboard = (keyboardMode) ? keyboardMode.options[keyboardMode.value].text : "",
            };
            serializer.Serialize(stream, xml);
            stream.Close();
            InputLayoutManager.OnLayoutLoaded.RemoveListener(layoutChanged);
            if (keyboardMode)
                InputLayoutManager.SetCurrentInputLayout(keyboardMode.options[keyboardMode.value].text);
            StartCoroutine(WaitReady());
        }

        void layoutChanged()
        {
            string[] names = InputLayoutManager.GetInputsName();
            int i = 0;
            int index = 0;
            keyboardMode.options = new List<Dropdown.OptionData>();
            foreach (var name in names)
            {
                if (name == KeyBoardValue) { index = i; }
                i++;
                keyboardMode.options.Add(new Dropdown.OptionData(name));
            }
            keyboardMode.value = index;
        }

        IEnumerator WaitReady()
        {
            SceneManager.LoadScene(scene, LoadSceneMode.Additive);
            while (!UMI3DBrowser.Exist)
                yield return new WaitForEndOfFrame();
            UMI3DBrowser.ChangeEnvironment(_ip.text + ":" + _port.text);
            UMI3DBrowser.useQwerty = (keyboardMode.value == 1);
            SceneManager.UnloadSceneAsync(thisScene);

            //initSocket();
        }
    }
}