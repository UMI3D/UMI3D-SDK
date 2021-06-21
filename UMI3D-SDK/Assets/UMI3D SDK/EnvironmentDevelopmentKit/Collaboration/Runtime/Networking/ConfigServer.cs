/*
Copyright 2019 - 2021 Inetum

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
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class ConfigServer 
{
    
    /// <summary>
    /// Set the name of the environment.
    /// </summary>
    public string nameParam = "";
    /// <summary>
    /// Set the public ip of the server.
    /// </summary>
    public string ipParam = "";
    /// <summary>
    /// Set the Authentification type. 
    /// <see cref="common.AuthenticationType"/>
    /// </summary>
    public string authParam = "";
    /// <summary>
    /// Set the token life span.
    /// </summary>
    public float tokenLifeParam = -1;
    /// <summary>
    /// Set the http port.
    /// </summary>
    public ushort httpPortParam = 0;
    /// <summary>
    /// Set the websocket port.
    /// </summary>
    public ushort udpportParam = 0;
    /// <summary>
    /// Set the public ip of the master server.
    /// </summary>
    public string masterIpParam = "";
    /// <summary>
    /// Set the port of the master server.
    /// </summary>
    public ushort masterPortParam = 0;
    /// <summary>
    /// Set the public ip of the master server.
    /// </summary>
    public string natIpParam = "";
    /// <summary>
    /// Set the port of the master server.
    /// </summary>
    public ushort natPortParam = 0;
    /// <summary>
    /// Set the max number of player.
    /// </summary>
    public int playersParam = 0;
    /// <summary>
    /// Set the id of the session.
    /// </summary>
    public string sessionIdParam = "";
    /// <summary>
    /// Set the comment of the session.
    /// </summary>
    public string sessionCommentParam = "";
    /// <summary>
    /// Set the icon url of the server.
    /// </summary>
    public string iconUrlParam = "";

    /// <summary>
    /// Read config server file.
    /// </summary>
    /// <param name="path"> must be a valid path.</param>
    /// <returns></returns>
    public static ConfigServer ReadXml(string path)
    {
        ConfigServer res = new ConfigServer();
        var ser = new XmlSerializer(typeof(ConfigServer));
        using (FileStream myFileStream = new FileStream(path, FileMode.Open))
        {
            res = (ConfigServer)ser.Deserialize(myFileStream);
        }
        return res;
    }


    // use to generate empty xml config
    public static void WriteXml(ConfigServer conf)
    {
        XmlSerializer ser = new XmlSerializer(typeof(ConfigServer));

        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.NewLineOnAttributes = true;
        XmlWriter writer = XmlWriter.Create("config.xml", settings);
        ser.Serialize(writer, conf);
        writer.Close();
    }


}
