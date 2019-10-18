using System.Collections;
using System.Text;
using umi3d.cdk;
using umi3d.common;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;

public class PostToServer : MonoBehaviour
{
}

#if UNITY_EDITOR

[CustomEditor(typeof(PostToServer))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PostToServer myScript = (PostToServer)target;
        if (UMI3DBrowser.Media != null && GUILayout.Button("interact"))
        {
            UMI3DHttpClient.Interact("fakeId");
        }
    }
}

#endif
