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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Compilation;
using UnityEngine;

public class AssemblyUtils : MonoBehaviour
{
    public static void ExportAssembliesTo(IEnumerable<string> assemblyNames, string ouputDirectory)
    {
        Debug.Log("====== Exporting Aseemblies to [" + ouputDirectory + "]:" + assemblyNames);

        Assembly[] playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);

        //TODO: Check if any requested assembly exists
        foreach(var s in assemblyNames)
        {
            Debug.Log(s);
        }

        foreach (var assembly in playerAssemblies)
        {
            if (assemblyNames.Contains(assembly.name))
            {
                if (File.Exists(assembly.outputPath))
                {
                    string filename = Path.GetFileName(assembly.outputPath);
                    string outputfile = Path.Combine(ouputDirectory, filename);
                    if (File.Exists(outputfile))
                        File.Delete(outputfile);
                    File.Copy(assembly.outputPath, outputfile);
                }
                else
                {
                    Debug.LogError("dll not found: " + assembly.outputPath);
                }
            }
            else
            {
                Debug.Log($"not found {assembly.name}");
            }
        }

        Debug.Log("====== Done");
    }
}
#endif