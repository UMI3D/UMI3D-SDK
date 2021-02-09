using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class PackagesExporter 
{
    const string folder = "../Packages/";
    const string pathCdk = folder+ "cdk.unitypackage";
    const string pathEdk = folder + "edk.unitypackage";
    const string pathCore = folder + "module/core.unitypackage";
    const string pathDependencies = folder + "module/dependencies.unitypackage";
    const string pathInteractionSystem = folder + "module/interaction-system.unitypackage";
    const string pathUserCapture = folder + "module/user-capture.unitypackage";
    const string pathCollaboration = folder + "module/collaboration.unitypackage";

    const string assetDependencies = "Assets/Dependencies";
    const string assetCommon = "Assets/Common";
    const string assetCDK = "Assets/ClientDevelopmentKit";
    const string assetEDK = "Assets/EnvironmentDevelopmentKit";

    const string coreFolder = "/Core";
    const string collaborationFolder = "/Collaboration";
    const string interactionSystemFolder = "/InteractionSystem";
    const string userCaptureFolder = "/UserCapture";




    [MenuItem("UMI3D/Export Packages")]
    static void ExportPackages()
    {
        var core = new List<string>{ assetCommon + coreFolder, assetEDK + coreFolder, assetCDK + coreFolder };
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

        if(! Directory.Exists(Application.dataPath + "/../" + folder))
        {
            Directory.CreateDirectory(Application.dataPath + "/../" + folder);
        }

        if (!Directory.Exists(Application.dataPath + "/../" + folder +"/module"))
        {
            Directory.CreateDirectory(Application.dataPath + "/../" + folder + "/module");
        }
        Debug.Log(Application.dataPath + folder);
        AssetDatabase.ExportPackage( assetDependencies, pathDependencies, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
        AssetDatabase.ExportPackage( core.ToArray() , pathCore, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
        AssetDatabase.ExportPackage(interaction.ToArray(), pathInteractionSystem, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
        AssetDatabase.ExportPackage(userCapture.ToArray(), pathUserCapture, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
        AssetDatabase.ExportPackage(collaboration.ToArray(), pathCollaboration, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
        AssetDatabase.ExportPackage(cdk, pathCdk, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
        AssetDatabase.ExportPackage(edk, pathEdk, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
    }
}
