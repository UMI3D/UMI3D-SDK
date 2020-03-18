using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLoader : MonoBehaviour
{
    [Tooltip("Bundles must be stored under the assets folder")]
    public List<string> bundlesToLoad = new List<string>();


    public void LoadAssets()
    {
        foreach (string bundlePath in bundlesToLoad)
        {
            string completePath = Application.dataPath + "/" + bundlePath;
            AssetBundle bundle = AssetBundle.LoadFromFile(completePath);

            if (bundle != null)
            {
                bundle.LoadAllAssets();                
            }
        }
    }

    private void OnDestroy()
    {
        AssetBundle.UnloadAllAssetBundles(true);
    }
}
