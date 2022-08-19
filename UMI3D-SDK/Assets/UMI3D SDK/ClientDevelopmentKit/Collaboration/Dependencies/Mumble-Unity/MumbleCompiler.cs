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
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using Assembly = UnityEditor.Compilation.Assembly;
using System.Linq;

public class MumbleCompiler 
{
    const string plugginfloder = @"Assets\UMI3D SDK\ClientDevelopmentKit\Collaboration\Dependencies\Mumble-Unity/Plugins";

    [MenuItem("Protobuf/Build model")]
    private static void BuildMyProtoModel()
    {

        RuntimeTypeModel typeModel = GetModel();
        typeModel.Compile("MyProtoModel", "MyProtoModel.dll");

        if (!Directory.Exists(plugginfloder))
        {
            Directory.CreateDirectory(plugginfloder);
        }

        File.Move("MyProtoModel.dll", plugginfloder+"/MyProtoModel.dll");

        AssetDatabase.Refresh();
    }

    [MenuItem("Protobuf/Create proto file")]
    private static void CreateProtoFile()
    {
        if (!Directory.Exists(plugginfloder))
        {
            Directory.CreateDirectory(plugginfloder);
        }

        RuntimeTypeModel typeModel = GetModel();
        using (FileStream stream = File.Open(plugginfloder+"/model.proto", FileMode.Create))
        {
            byte[] protoBytes = Encoding.UTF8.GetBytes(typeModel.GetSchema(null));
            stream.Write(protoBytes, 0, protoBytes.Length);
        }
    }

    private static RuntimeTypeModel GetModel()
    {
        RuntimeTypeModel typeModel = TypeModel.Create();

        foreach (var t in GetTypes(CompilationPipeline.GetAssemblies()))
        {
            var contract = t.GetCustomAttributes(typeof(ProtoContractAttribute), false);
            if (contract.Length > 0)
            {
                MetaType metaType = typeModel.Add(t, true);

                // support ISerializationCallbackReceiver
                if (typeof(ISerializationCallbackReceiver).IsAssignableFrom(t))
                {
                    MethodInfo beforeSerializeMethod = t.GetMethod("OnBeforeSerialize");
                    MethodInfo afterDeserializeMethod = t.GetMethod("OnAfterDeserialize");

                    metaType.SetCallbacks(beforeSerializeMethod, null, null, afterDeserializeMethod);
                }

                //add unity types
                typeModel.Add(typeof(Vector2), false).Add("x", "y");
                typeModel.Add(typeof(Vector3), false).Add("x", "y", "z");
            }
        }

        return typeModel;
    }

    private static IEnumerable<Type> GetTypes(IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            foreach (Type type in AppDomain.CurrentDomain.Load(assembly.name).GetTypes())
            {
                yield return type;
            }
        }
    }

}
#endif