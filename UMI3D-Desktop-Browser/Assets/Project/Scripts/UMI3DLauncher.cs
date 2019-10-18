﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using umi3d.cdk;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UMI3DLauncher : MonoBehaviour
{
    [Serializable]
    public class StartData
    {
        public string ip = null;
        public string port = null;
        public int keyboard = 0;
    }
    public string scene;
    public string thisScene;
    public string xmlPath = "start.xml";
    public InputField _ip;
    public InputField _port;
    public Dropdown keyboardMode;
    public Text _version;

    int connected = 0;

    string hub = null;

    // Start is called before the first frame update
    void Start()
    {
        LoadXml();
        _version.text = umi3d.UMI3DVersion.version;
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
            keyboardMode.value = xml.keyboard;
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
            keyboard = keyboardMode.value
        };
        serializer.Serialize(stream, xml);
        stream.Close();
        StartCoroutine(WaitReady());
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
