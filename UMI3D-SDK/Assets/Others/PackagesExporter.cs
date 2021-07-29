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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
public class PackagesExporter 
{
    const string packageFolder = "../Packages/";
    const string dllFolder = "../Dll/";
    const string pathCdk = packageFolder+ "cdk.unitypackage";
    const string pathEdk = packageFolder + "edk.unitypackage";
    const string pathCore = packageFolder + "module/core.unitypackage";
    const string pathDependencies = packageFolder + "module/dependencies.unitypackage";
    const string pathInteractionSystem = packageFolder + "module/interaction-system.unitypackage";
    const string pathUserCapture = packageFolder + "module/user-capture.unitypackage";
    const string pathCollaboration = packageFolder + "module/collaboration.unitypackage";

    const string assetDependencies = "Assets/UMI3D SDK/Dependencies";
    const string assetCommon = "Assets/UMI3D SDK/Common";
    const string assetCDK = "Assets/UMI3D SDK/ClientDevelopmentKit";
    const string assetEDK = "Assets/UMI3D SDK/EnvironmentDevelopmentKit";

    const string coreFolder = "/Core";
    const string collaborationFolder = "/Collaboration";
    const string interactionSystemFolder = "/InteractionSystem";
    const string userCaptureFolder = "/UserCapture";

    [MenuItem("UMI3D/Build Dll")]
    static void BuildDll()
    {
        ClearOrCreateDirectory(dllFolder);
        IEnumerable<string> assemblies = Directory.EnumerateFiles("./Assets/", "*.asmdef", SearchOption.AllDirectories).Select(s=> Path.GetFileNameWithoutExtension(s));
        IEnumerable<string> dlls = GetFiles("./Assets/", new string[] { "*.dll" , "*.pdb" ,"*.dll.meta"}, SearchOption.AllDirectories);
        foreach (var dll in dlls)
        {
            var path = $"{dllFolder}{dll.Split(new string[] { "/Assets/" }, System.StringSplitOptions.RemoveEmptyEntries)[1]}";

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            System.IO.File.Copy(dll, $"{dllFolder}{path}");
        }

        AssemblyUtils.ExportAssembliesTo(assemblies, dllFolder);
    }

    public static IEnumerable<string> GetFiles(string path,
                    string[] searchPatterns,
                    SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return searchPatterns.AsParallel()
               .SelectMany(searchPattern =>
                      Directory.EnumerateFiles(path, searchPattern, searchOption));
    }

    public static void ClearOrCreateDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(directory);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
        else
        {
            Directory.CreateDirectory(directory);
        }
    }

    [MenuItem("UMI3D/Export Packages")]
    static void ExportPackagesAll()
    {
        ExportPackages(true);
    }

    [MenuItem("UMI3D/Export Packages (EDK & CDK only)")]
    static void ExportPackagesEDKCDK()
    {
        ExportPackages(false);
    }

    static void ExportPackages(bool all)
    {
        var core = new List<string> { assetCommon + coreFolder, assetEDK + coreFolder, assetCDK + coreFolder };
        var interaction = new List<string> { assetCommon + interactionSystemFolder, assetEDK + interactionSystemFolder, assetCDK + interactionSystemFolder };
        var userCapture = new List<string> { assetCommon + userCaptureFolder, assetEDK + userCaptureFolder, assetCDK + userCaptureFolder };
        var collaboration = new List<string> { assetCommon + collaborationFolder, assetEDK + collaborationFolder, assetCDK + collaborationFolder };
        core.Add(assetDependencies);
        interaction.AddRange(core);
        collaboration.AddRange(interaction);
        collaboration.AddRange(userCapture);
        userCapture.AddRange(core);

        var cdk = new string[] {assetDependencies,
            assetCommon + coreFolder, assetCommon + interactionSystemFolder, assetCommon + userCaptureFolder, assetCommon + collaborationFolder,
            assetCDK + coreFolder, assetCDK + interactionSystemFolder, assetCDK + userCaptureFolder, assetCDK + collaborationFolder
        };

        var edk = new string[] {assetDependencies,
            assetCommon + coreFolder, assetCommon + interactionSystemFolder, assetCommon + userCaptureFolder, assetCommon + collaborationFolder,
            assetEDK + coreFolder, assetEDK + interactionSystemFolder, assetEDK + userCaptureFolder, assetEDK + collaborationFolder
        };

        if (!Directory.Exists(Application.dataPath + "/../" + packageFolder))
        {
            Directory.CreateDirectory(Application.dataPath + "/../" + packageFolder);
        }

        if (!Directory.Exists(Application.dataPath + "/../" + packageFolder + "/module"))
        {
            Directory.CreateDirectory(Application.dataPath + "/../" + packageFolder + "/module");
        }
        Debug.Log(Application.dataPath + packageFolder);
        if (all)
        {
            AssetDatabase.ExportPackage(assetDependencies, pathDependencies, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
            AssetDatabase.ExportPackage(core.ToArray(), pathCore, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
            AssetDatabase.ExportPackage(interaction.ToArray(), pathInteractionSystem, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
            AssetDatabase.ExportPackage(userCapture.ToArray(), pathUserCapture, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
            AssetDatabase.ExportPackage(collaboration.ToArray(), pathCollaboration, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
        }
        AssetDatabase.ExportPackage(cdk, pathCdk, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
        AssetDatabase.ExportPackage(edk, pathEdk, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
    }
}
#endif