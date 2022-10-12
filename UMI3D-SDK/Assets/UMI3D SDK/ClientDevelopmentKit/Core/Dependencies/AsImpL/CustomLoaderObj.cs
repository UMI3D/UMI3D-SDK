using AsImpL;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CustomLoaderObj : LoaderObj
{

    protected override IEnumerator LoadOrDownloadText(string url, bool notifyErrors = true)
    {
        loadedText = null;
#if UNITY_2018_3_OR_NEWER
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            SetCertificate(uwr);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                if (notifyErrors)
                {
                    //Debug.LogError(uwr.error);
                }
                objLoadingProgress.error = true;
            }
            else
            {
                // Get downloaded asset bundle
                loadedText = uwr.downloadHandler.text;
            }
        }

#else
            WWW www = new WWW(url);
            yield return www;
            if (www.error != null)
            {
                if (notifyErrors)
                {
                    Debug.LogError("Error loading " + url + "\n" + www.error);
                }
            }
            else
            {
                loadedText = www.text;
            }
#endif

    }

    protected override IEnumerator LoadMaterialTexture(string basePath, string path)
    {
        loadedTexture = null;
        string texPath = GetTextureUrl(basePath, path);

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(texPath))
        {
            SetCertificate(uwr);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                loadedTexture = DownloadHandlerTexture.GetContent(uwr);
            }
        }
    }


    protected void SetCertificate(UnityWebRequest www)
    {

        if (!String.IsNullOrEmpty(buildOptions.authorization))
        {
            www.certificateHandler = new AcceptAllCertificates();

            www.SetRequestHeader(buildOptions.authorizationName, buildOptions.authorization);

        }
    }


}

