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
using inetum.unityUtils.editor;
using System.Threading.Tasks;
#if UNITY_EDITOR
public class PackagesExporter
{
    static string packageFolder = "../Packages/";
    const string dllFolder = "../Dll/";

    const string Cdk = "cdk.unitypackage";
    const string Edk = "edk.unitypackage";
    const string StarterKit = "server-starter-kit.unitypackage";

    const string Core = "core.unitypackage";
    const string Dependencies = "dependencies.unitypackage";
    const string InteractionSystem = "interaction-system.unitypackage";
    const string UserCapture = "user-capture.unitypackage";
    const string Collaboration = "collaboration.unitypackage";


    static string pathCdk => packageFolder + Cdk;
    static string pathEdk => packageFolder + Edk;
    static string pathServerStarterKit => packageFolder + StarterKit;

    static string pathCore => packageFolder + "module/" +Core;
    static string pathDependencies => packageFolder + "module/"+Dependencies;
    static string pathInteractionSystem => packageFolder + "module/"+ InteractionSystem;
    static string pathUserCapture => packageFolder + "module/" + UserCapture;
    static string pathCollaboration => packageFolder + "module/"+ Collaboration;

    const string assetDependencies = "Assets/UMI3D SDK/Dependencies";
    const string assetCommon = "Assets/UMI3D SDK/Common";
    const string assetCDK = "Assets/UMI3D SDK/ClientDevelopmentKit";
    const string assetEDK = "Assets/UMI3D SDK/EnvironmentDevelopmentKit";

    const string assetServerStarterKit = "Assets/Server Starter Kit";

    const string coreFolder = "/Core";
    const string collaborationFolder = "/Collaboration";
    const string interactionSystemFolder = "/InteractionSystem";
    const string userCaptureFolder = "/UserCapture";

    [MenuItem("UMI3D/Build Dll")]
    public static void BuildDll()
    {
        ClearOrCreateDirectory(dllFolder);
        IEnumerable<string> assemblies = Directory.EnumerateFiles("./Assets/", "*.asmdef", SearchOption.AllDirectories).Select(s => Path.GetFileNameWithoutExtension(s));
        IEnumerable<string> dlls = GetFiles("./Assets/", new string[] { "*.dll", "*.pdb", "*.dll.meta" }, SearchOption.AllDirectories);
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
    public static void ExportPackagesAll()
    {
        ExportPackages(true, "../Packages/", ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
    }

    public static List<(string, string)> ExportPackages(string path)
    {
        return ExportPackages(true, path, ExportPackageOptions.Recurse);
    }


    [MenuItem("UMI3D/Export Packages (EDK & CDK only)")]
    static void ExportPackagesEDKCDK()
    {
        ExportPackages(false,"../Packages/", ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
    }

    static List<(string, string)> ExportPackages(bool all, string path, ExportPackageOptions flags)
    {
        packageFolder = path;
        var fullpath = Application.dataPath + "/../" + packageFolder;
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

        var ServerStarterKit = new List<string>(edk) { assetServerStarterKit };

        if (!Directory.Exists(fullpath))
        {
            Directory.CreateDirectory(fullpath);
        }

        if (!Directory.Exists(fullpath + "/module"))
        {
            Directory.CreateDirectory(fullpath + "/module");
        }
        Debug.Log($"Export package at {fullpath}");

        List<(string,string)> list = new List<(string, string)>();

        if (all)
        {
            list.Add((Application.dataPath + "/../" + pathDependencies, Dependencies));
            list.Add((Application.dataPath + "/../" + pathCore, Core));
            list.Add((Application.dataPath + "/../" + pathInteractionSystem, InteractionSystem));
            list.Add((Application.dataPath + "/../" + pathUserCapture, UserCapture));
            list.Add((Application.dataPath + "/../" + pathCollaboration, Collaboration));
            list.Add((Application.dataPath + "/../" + pathServerStarterKit, StarterKit));

            AssetDatabase.ExportPackage(assetDependencies, pathDependencies, flags );
            AssetDatabase.ExportPackage(core.ToArray(), pathCore, flags);
            AssetDatabase.ExportPackage(interaction.ToArray(), pathInteractionSystem, flags);
            AssetDatabase.ExportPackage(userCapture.ToArray(), pathUserCapture, flags);
            AssetDatabase.ExportPackage(collaboration.ToArray(), pathCollaboration, flags);

            AssetDatabase.ExportPackage(ServerStarterKit.ToArray(), pathServerStarterKit, flags);
        }

        list.Add((Application.dataPath + "/../" + pathCdk, Cdk));
        list.Add((Application.dataPath + "/../" + pathEdk, Edk));

        AssetDatabase.ExportPackage(cdk, pathCdk, flags);
        AssetDatabase.ExportPackage(edk, pathEdk, flags);

        return list;
    }
}
#endif