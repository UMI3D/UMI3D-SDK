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
using MathNet.Numerics.RootFinding;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Runtime.CompilerServices;
using CodiceApp.EventTracking.Plastic;
using MumbleProto;
using static UnityEngine.Rendering.ReloadAttribute;
using System;
#if UNITY_EDITOR

public class BuildEvents : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public static bool IsBuilding { get; private set; }

    public void OnPreprocessBuild(BuildReport report)
    {
        IsBuilding = true;
        Debug.Log("Build started");
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        IsBuilding = false;
        Debug.Log("Build finished");
    }

    public async Task WaitBuildingEnd()
    {
        Debug.Log("Build start");
        await Task.Delay(100000);
        while (IsBuilding)
        {
            await Task.Delay(1000);
        }
        Debug.Log("Build end");
    }
}

[Serializable]
public class PackageData
{
    readonly public string pathRoot ;
    public string relativePath = null;
    public string name = null;
    public string FullPath => (pathRoot ?? string.Empty) + (relativePath ?? string.Empty);
    public List<string> folderTobuild;


    public bool buildState = false;
    public bool isBuildng = false;

    public PackageData(string pathRoot)
    {
        this.pathRoot = pathRoot;
    }

    public bool? build
    {
        get => isBuildng ? buildState : null;
        set
        {
            if(value == null)
            {
                isBuildng = false;
                buildState = false;
            }
            else
            {
                isBuildng = true;
                buildState = value.Value;
            }
        }
    }
}

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

    static string pathCore => packageFolder + "module/" + Core;
    static string pathDependencies => packageFolder + "module/" + Dependencies;
    static string pathInteractionSystem => packageFolder + "module/" + InteractionSystem;
    static string pathUserCapture => packageFolder + "module/" + UserCapture;
    static string pathCollaboration => packageFolder + "module/" + Collaboration;

    const string assetDependencies = "Assets/UMI3D SDK/Dependencies";
    const string assetCommon = "Assets/UMI3D SDK/Common";
    const string assetCDK = "Assets/UMI3D SDK/ClientDevelopmentKit";
    const string assetEDK = "Assets/UMI3D SDK/EnvironmentDevelopmentKit";

    const string assetServerStarterKit = "Assets/Server Starter Kit";

    const string coreFolder = "/Core";
    const string collaborationFolder = "/Collaboration";
    const string interactionSystemFolder = "/InteractionSystem";
    const string userCaptureFolder = "/UserCapture";

    //[MenuItem("UMI3D/Build Dll")]
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

    //[MenuItem("UMI3D/Export Packages")]
    public static async Task ExportPackagesAll()
    {
        var packages = GetExportPackages("../Packages/", true);
        await ExportPackages(packages, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
    }

    //[MenuItem("UMI3D/Export Packages (EDK & CDK only)")]
    static async Task ExportPackagesEDKCDK()
    {
        var packages = GetExportPackages("../Packages/", false);
        await ExportPackages(packages, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
    }

    public static async Task ExportPackages(List<PackageData> packages)
    {
        await ExportPackages(packages, ExportPackageOptions.Recurse);
    }

    public static List<PackageData> GetExportPackages(string path)
    {
        return GetExportPackages(path, true);
    }

    static List<PackageData> GetExportPackages(string path, bool all)
    {
        CleanFolder(path);
        return GetPackageDatas(all);
    }

    public static async Task BuildPackage(PackageData data)
    {
        await ExportPackages(data, ExportPackageOptions.Recurse);
    }
    public static void SyncBuildPackage(PackageData data)
    {
        AssetDatabase.ExportPackage(data.folderTobuild.ToArray(), data.relativePath, ExportPackageOptions.Recurse);
    }



    static List<PackageData> GetPackageDatas(bool all)
    {
        var result = new List<PackageData>();
        string root = Application.dataPath + "/../";

        PackageData cdk = new(root);
        PackageData edk = new(root);

        cdk.folderTobuild = new() {PackagesExporter.assetDependencies,
            assetCommon + coreFolder, assetCommon + interactionSystemFolder, assetCommon + userCaptureFolder, assetCommon + collaborationFolder,
            assetCDK + coreFolder, assetCDK + interactionSystemFolder, assetCDK + userCaptureFolder, assetCDK + collaborationFolder
        };

        edk.folderTobuild = new() {PackagesExporter.assetDependencies,
            assetCommon + coreFolder, assetCommon + interactionSystemFolder, assetCommon + userCaptureFolder, assetCommon + collaborationFolder,
            assetEDK + coreFolder, assetEDK + interactionSystemFolder, assetEDK + userCaptureFolder, assetEDK + collaborationFolder
        };

        cdk.relativePath = pathCdk;
        edk.relativePath = pathEdk;

        cdk.name = Cdk;
        edk.name = Edk;


        if (all)
        {
            PackageData core = new(root);
            PackageData assetDependencies = new(root);
            PackageData interaction = new(root);
            PackageData userCapture = new(root);
            PackageData collaboration = new(root);
            PackageData serverStarterKit = new(root);

            assetDependencies.folderTobuild = new List<string> { PackagesExporter.assetDependencies };

            core.folderTobuild = new List<string> { assetCommon + coreFolder, assetEDK + coreFolder, assetCDK + coreFolder };
            interaction.folderTobuild = new List<string> { assetCommon + interactionSystemFolder, assetEDK + interactionSystemFolder, assetCDK + interactionSystemFolder };
            userCapture.folderTobuild = new List<string> { assetCommon + userCaptureFolder, assetEDK + userCaptureFolder, assetCDK + userCaptureFolder };
            collaboration.folderTobuild = new List<string> { assetCommon + collaborationFolder, assetEDK + collaborationFolder, assetCDK + collaborationFolder };

            core.folderTobuild.AddRange(assetDependencies.folderTobuild);
            interaction.folderTobuild.AddRange(core.folderTobuild);
            collaboration.folderTobuild.AddRange(interaction.folderTobuild);
            collaboration.folderTobuild.AddRange(userCapture.folderTobuild);
            userCapture.folderTobuild.AddRange(core.folderTobuild);

            serverStarterKit.folderTobuild = new List<string>(edk.folderTobuild) { assetServerStarterKit };

            assetDependencies.relativePath = pathDependencies;
            core.relativePath = pathCore;
            interaction.relativePath = pathInteractionSystem;
            userCapture.relativePath = pathUserCapture;
            collaboration.relativePath = pathCollaboration;
            serverStarterKit.relativePath = pathServerStarterKit;

            assetDependencies.name = Dependencies;
            core.name = Core;
            interaction.name = InteractionSystem;
            userCapture.name = UserCapture;
            collaboration.name = Collaboration;
            serverStarterKit.name = StarterKit;

            result.Add(assetDependencies);
            result.Add(core);
            result.Add(interaction);
            result.Add(userCapture);
            result.Add(collaboration);
            result.Add(serverStarterKit);
        }

        result.Add(edk);
        result.Add(cdk);

        return result;
    }

    static void CleanFolder(string path)
    {
        var fullpath = Application.dataPath + "/../" + path;
        if (!Directory.Exists(fullpath))
        {
            Directory.CreateDirectory(fullpath);
        }

        if (!Directory.Exists(fullpath + "/module"))
        {
            Directory.CreateDirectory(fullpath + "/module");
        }
    }
    static async Task<List<PackageData>> ExportPackages(List<PackageData> packages, ExportPackageOptions flags)
    {
        foreach (var package in packages)
        {
            package.build = false;
            await ExportPackages(package, flags);
            package.build = true;
        }

        return packages;
    }

    public static async Task ExportPackages(PackageData data, ExportPackageOptions flags)
    {
        BuildEvents buildEvents = new();
        AssetDatabase.ExportPackage(data.folderTobuild.ToArray(), data.relativePath, flags);
        await buildEvents.WaitBuildingEnd();
    }


}
#endif