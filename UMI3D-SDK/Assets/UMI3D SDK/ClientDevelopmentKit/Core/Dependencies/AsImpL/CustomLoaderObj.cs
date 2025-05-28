using AsImpL;
using System.Collections;
using umi3d.common;
using UnityEngine;
using UnityEngine.Networking;

public class CustomLoaderObj : LoaderObj
{
    protected override IEnumerator LoadOrDownloadText(string url, bool notifyErrors = true)
    {
        loadedText = null;
        using UnityWebRequest uwr = UnityWebRequest.Get(url);
        SetCertificate(uwr);

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            if (notifyErrors)
            {
                //Debug.LogError(uwr.error);
            }

            objLoadingProgress.error = true;

            if (url != uwr.url)
            {
                NotifyModelError(uwr.url);
            }
        }
        else
        {
            // Get downloaded asset bundle
            loadedText = uwr.downloadHandler.text;
        }
    }

    protected override IEnumerator LoadMaterialTexture(string basePath, string path)
    {
        loadedTexture = null;
        string texPath = GetTextureUrl(basePath, path);

        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(texPath);
        SetCertificate(uwr);

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(uwr.error + " " + texPath);
        }
        else
        {
            // Get downloaded asset bundle
            loadedTexture = DownloadHandlerTexture.GetContent(uwr);
        }
    }

    protected void SetCertificate(UnityWebRequest www)
    {
        if (!string.IsNullOrEmpty(buildOptions.authorization))
        {
            www.certificateHandler = new AcceptAllCertificates();

            www.SetRequestHeader(buildOptions.authorizationName, buildOptions.authorization);
        }
    }
}

