/*
Copyright 2019 Gfi Informatique

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
using umi3d.edk;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(CVEMaterial))]
    [CanEditMultipleObjects]
    public class CVEMaterialEditor : Editor
    {

        PreviewRenderUtility previewRenderUtility;
        Light previewLight;
        GameObject previewObject;
        Vector2 previewDir;
        Texture2D texture;
        umi3d.edk.CVEMaterial Target;


        static void SyncMeshRenderer(CVEMaterial material)
        {
            UMI3D root = UMI3D.Instance;
            if (root)
            {
                foreach (CVEPrimitive primitive in root.GetComponentsInChildren<CVEPrimitive>())
                {
                    if (primitive.Material == material)
                        primitive.SyncMeshRenderer();
                }
                foreach (CVEModel model in root.GetComponentsInChildren<CVEModel>())
                {
                    if (model.Material == material)
                        model.SyncMeshRenderer();
                }
            }
        }



        void OnEnable()
        {


            //previewRenderUtility = new PreviewRenderUtility(true);
            //previewRenderUtility.cameraFieldOfView = 30f;

            //previewRenderUtility.camera.farClipPlane = 1000;
            //previewRenderUtility.camera.nearClipPlane = 0.3f;

            //PreviewLight = previewRenderUtility.camera.gameObject.AddComponent<Light>();
            //PreviewLight.type = LightType.Directional;

            //Target = (umi3d.edk.CVEMaterial)target;
            //previewObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //previewObject.hideFlags = HideFlags.HideAndDontSave;
            //previewDir = Vector2.zero;
        }



        #region Preview

        //public override bool HasPreviewGUI()
        //{
        //    return false;
        //}

        //public override void OnPreviewGUI(Rect r, GUIStyle background)
        //{
        //    //if (previewRenderUtility == null) previewRenderUtility = new PreviewRenderUtility(true);

        //    //previewDir = Drag2D(previewDir, r);
        //    //previewRenderUtility.BeginPreview(r, background);

        //    //var previewCamera = previewRenderUtility.camera;
        //    //Vector3 rot = new Vector3(-previewDir.y, -previewDir.x, 0);

        //    //previewCamera.transform.position = previewObject.transform.position + Quaternion.Euler(rot) * Vector3.forward * 5;
        //    //previewCamera.transform.LookAt(previewObject.transform);
        //    //previewCamera.Render();
        //    //previewRenderUtility.DrawMesh(previewObject.GetComponent<MeshFilter>().sharedMesh, previewObject.transform.position, previewObject.transform.rotation, previewObject.GetComponent<Renderer>().sharedMaterial, 0);
        //    //previewRenderUtility.EndAndDrawPreview(r);
        //}

        //public override GUIContent GetPreviewTitle()
        //{
        //    return new GUIContent("preview");
        //}

        //public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        //{
        //    base.OnInteractivePreviewGUI(r, background);
        //}

        //public override string GetInfoString()
        //{
        //    return previewDir.ToString();
        //    // return "www.url.todo";
        //}


        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && position.width > 50f)
                    {
                        GUIUtility.hotControl = controlID;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                    }
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        scrollPosition -= current.delta / Mathf.Min(position.width, position.height) * 140f;
                        scrollPosition.y = scrollPosition.y % 360;
                        if (scrollPosition.y < 0) scrollPosition.y += 360;
                        scrollPosition.x = scrollPosition.x % 180;
                        if (scrollPosition.x < 0) scrollPosition.x += 360;
                        current.Use();
                        GUI.changed = true;
                    }
                    break;
            }
            return scrollPosition;
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            CVEMaterial material = target as CVEMaterial;

            var Icon = Resources.Load("CVEMaterial/CVEMaterial Icon") as Texture2D;
            if (Icon != null)
            {
                Color[] colors = Icon.GetPixels();
                var texture = new Texture2D(Icon.width, Icon.height);
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i].r *= material.AlbedoColor.r;
                    colors[i].g *= material.AlbedoColor.g;
                    colors[i].b *= material.AlbedoColor.b;
                }
                texture.SetPixels(colors);
                texture.Apply();
                return texture;
            }
            return null;
            //Debug.Log(texture);
            //if (texture == null) {


            //return base.RenderStaticPreview(assetPath,subAssets,width,height);
            //}
            //else
            //{
            //    Color[] colors = texture.GetPixels();
            //    for (int i = 0; i < colors.Length; i++)
            //    {
            //        colors[i] *= Color.blue;
            //    }
            //    texture.SetPixels(colors);
            //    texture.Apply();
            //    return texture;
            //}
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                var material = target as CVEMaterial;
                SyncMeshRenderer(material);
            }
        }

        #endregion

    }
}
#endif