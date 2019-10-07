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

using UnityEditor;
using UnityEngine;

namespace umi3d.edk.editor
{
    public class Menu : Editor
    {

        #region MenuButton

        [MenuItem("GameObject/Umi3D/Umi3D Node", false, -1000)]
        static void CreateUmi3D_Node()
        {
            UMI3D_Wizard.Display(
                (GameObject g) => { Selection.objects = new GameObject[1] { g }; }
                );
        }

        [MenuItem("GameObject/Umi3D/Umi3D Node", true)]
        static bool Validate_CreateUmi3D_Node()
        {
            return !edk.UMI3D.Instance || !edk.UMI3D.Instance.isRunning;
        }

        [MenuItem("GameObject/Umi3D/Empty", false, 0)]
        public static void CreateEmpty(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
    (GameObject g) => { Selection.objects = new GameObject[1] { CreateEmpty(g.transform) }; }
    );
            else
                Selection.objects = new GameObject[1] { CreateEmpty(parent.transform) };
        }

        /// <summary>
        /// Create an empty UMI3D Node
        /// To be use only in editor script.
        /// </summary>
        /// <param name="parent">Under which node this node should be instanciate</param>
        public static GameObject CreateEmpty(Transform parent)
        {
            GameObject node = new GameObject();
            node.name = "Empty";
            if (parent)
                node.transform.SetParent(parent, false);
            node.AddComponent<edk.EmptyObject3D>();
            return node;
        }


        [MenuItem("GameObject/Umi3D/Mesh Object", false, 1)]
        static void CreateMesh(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreateMesh(g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreateMesh(parent.transform) };
        }

        static GameObject CreateMesh(Transform parent)
        {
            GameObject node = new GameObject();
            node.name = "Mesh";
            if (parent)
                node.transform.SetParent(parent, false);
            node.AddComponent<edk.CVEModel>();
            return node;
        }

        static GameObject CreatePrimitive(common.MeshPrimitive primitive, Transform parent)
        {
            GameObject node = new GameObject();
            node.name = primitive.ToString();
            if (parent)
                node.transform.SetParent(parent, false);
            node.AddComponent<edk.CVEPrimitive>().primitive = primitive;
            return node;
        }

        [MenuItem("GameObject/Umi3D/3D Object/Cube", false, 2)]
        static void CreateCube(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) =>
                    {
                        Selection.objects = new GameObject[1] { CreatePrimitive(common.MeshPrimitive.Cube, g.transform) };
                    }
                    );
            else
                Selection.objects = new GameObject[1] { CreatePrimitive(common.MeshPrimitive.Cube, parent.transform) };
        }

        [MenuItem("GameObject/Umi3D/3D Object/Sphere", false, 3)]
        static void CreateSphere(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreatePrimitive(common.MeshPrimitive.Sphere, g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreatePrimitive(common.MeshPrimitive.Sphere, parent.transform) };
        }

        [MenuItem("GameObject/Umi3D/3D Object/Capsule", false, 4)]
        static void CreateCapsule(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreatePrimitive(common.MeshPrimitive.Capsule, g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreatePrimitive(common.MeshPrimitive.Capsule, parent.transform) };
        }

        [MenuItem("GameObject/Umi3D/3D Object/Cylinder", false, 5)]
        static void CreateCylinder(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreatePrimitive(common.MeshPrimitive.Cylinder, g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreatePrimitive(common.MeshPrimitive.Cylinder, parent.transform) };
        }

        [MenuItem("GameObject/Umi3D/3D Object/Plane", false, 6)]
        static void CreatePlane(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreatePrimitive(common.MeshPrimitive.Plane, g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreatePrimitive(common.MeshPrimitive.Plane, parent.transform) };
        }

        //[MenuItem("GameObject/Umi3D/3D Object/Quad", false, 7)]
        //static void CreateQuad()
        //{
        //    Debug.Log("Create Quad");
        //}

        [MenuItem("GameObject/Umi3D/Effects/Line", false, 8)]
        static void CreateLine(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreateLine(g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreateLine(parent.transform) };
        }
        static GameObject CreateLine(Transform parent)
        {
            GameObject node = new GameObject();
            node.name = "Line";
            if (parent)
                node.transform.SetParent(parent, false);
            node.AddComponent<edk.CVELine>();
            return node;
        }

        #region Light

        static GameObject CreateLight(common.LightTypes light, Transform parent)
        {
            GameObject node = new GameObject();
            node.name = light.ToString() + " Light";
            if (parent)
                node.transform.SetParent(parent, false);
            var component = node.AddComponent<edk.CVELight>();
            component._type = light;
            component.SyncEnvironmentLight();
            return node;
        }

        [MenuItem("GameObject/Umi3D/Light/Directional Light", false, 9)]
        static void CreateLight_Directional(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreateLight(common.LightTypes.Directional, g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreateLight(common.LightTypes.Directional, parent.transform) };
        }


        [MenuItem("GameObject/Umi3D/Light/Point Light", false, 10)]
        static void CreateLight_Point(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreateLight(common.LightTypes.Point, g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreateLight(common.LightTypes.Point, parent.transform) };
        }

        [MenuItem("GameObject/Umi3D/Light/Spotlight", false, 11)]
        static void CreateLight_Spot(MenuCommand command)
        {

            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreateLight(common.LightTypes.Spot, g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreateLight(common.LightTypes.Spot, parent.transform) };
        }

        //[MenuItem("GameObject/Umi3D/Light/Area Light", false, 12)]
        //static void CreateLight_Area(MenuCommand command)
        //{

        //    GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
        //    if (!parent)
        //        UMI3D_Wizard.Display(
        //            (GameObject g) => { Selection.objects = new GameObject[1] { CreateLight(common.LightTypes.Ambient, g.transform) }; }
        //            );
        //    else
        //        Selection.objects = new GameObject[1] { CreateLight(common.LightTypes.Ambient, parent.transform) };
        //}

        #endregion

        [MenuItem("GameObject/Umi3D/Audio/Audio Source", false, 13)]
        static void CreateAudio_Source(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreateAudio_Source(g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreateAudio_Source(parent.transform) };
        }
        static GameObject CreateAudio_Source(Transform parent)
        {
            GameObject node = new GameObject();
            node.name = "Audio Source";
            if (parent)
                node.transform.SetParent(parent, false);
            node.AddComponent<edk.CVEAudioSource>();
            return node;
        }

        [MenuItem("GameObject/Umi3D/Video/Video Player", false, 14)]
        static void CreateVideo_Player(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreateVideo_Player(g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreateVideo_Player(parent.transform) };
        }
        static GameObject CreateVideo_Player(Transform parent)
        {
            GameObject node = new GameObject();
            node.name = "Video Player";
            if (parent)
                node.transform.SetParent(parent, false);
            node.AddComponent<edk.CVEVideo>();
            return node;
        }

        static Transform ForceCanvasInParent(Transform parent)
        {
            if (parent != null)
            {
                if (parent.GetComponentInParent<Canvas>()) return parent;
            }
            return CreateCanvas(parent).transform;
        }

        [MenuItem("GameObject/Umi3D/UI/Text", false, 15)]
        static void CreateUI_Text(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreateUI_Text(g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreateUI_Text(parent.transform) };
        }

        static GameObject CreateUI_Text(Transform parent)
        {
            parent = ForceCanvasInParent(parent);
            GameObject node = new GameObject();
            node.name = "Text";
            if (parent)
                node.transform.SetParent(parent, false);
            node.AddComponent<edk.UIText>();
            return node;
        }

        [MenuItem("GameObject/Umi3D/UI/Image", false, 15)]
        static void CreateUI_Image(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreateImage(g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreateImage(parent.transform) };
        }
        static GameObject CreateImage(Transform parent)
        {
            parent = ForceCanvasInParent(parent);
            GameObject node = new GameObject();
            node.name = "Image";
            if (parent)
                node.transform.SetParent(parent, false);
            node.AddComponent<edk.UIImage>();
            return node;
        }


        [MenuItem("GameObject/Umi3D/UI/Canvas", false, 50)]
        static void CreateUI_Canvas(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreateCanvas(g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreateCanvas(parent.transform) };
        }
        static GameObject CreateCanvas(Transform parent)
        {
            GameObject node = new GameObject();
            node.name = "Canvas";
            if (parent)
                node.transform.SetParent(parent.transform);
            node.AddComponent<edk.UICanvas>();
            return node;
        }

        [MenuItem("GameObject/Umi3D/UI/Panel", false, 51)]
        static void CreateUI_Panel(MenuCommand command)
        {
            GameObject parent = (GameObject)command.context ?? (edk.UMI3D.Instance ? edk.UMI3D.Instance.gameObject : GameObject.Find("UMI3D"));
            if (!parent)
                UMI3D_Wizard.Display(
                    (GameObject g) => { Selection.objects = new GameObject[1] { CreatePanel(g.transform) }; }
                    );
            else
                Selection.objects = new GameObject[1] { CreatePanel(parent.transform) };
        }
        static GameObject CreatePanel(Transform parent)
        {
            parent = ForceCanvasInParent(parent);
            GameObject node = CreateImage(parent);
            node.name = "Panel";
            return node;
        }
        #endregion

        #region Context menu

        [MenuItem("CONTEXT/CVEPrimitive/Switch to Model")]
        private static void SwitchToModel(MenuCommand menuCommand)
        {
            var Primitive = menuCommand.context as edk.CVEPrimitive;
            edk.CVEMaterial material = Primitive.Material;
            GameObject parent = Primitive.gameObject;
            GameObject preview = Primitive.preview;
            DestroyImmediate(Primitive);
            DestroyImmediate(preview);
            edk.CVEModel model = parent.AddComponent<edk.CVEModel>();
            model.Material = material;
        }

        [MenuItem("CONTEXT/CVEModel/Switch to Primitive")]
        private static void SwitchToPrimitive(MenuCommand menuCommand)
        {
            var Model = menuCommand.context as edk.CVEModel;
            edk.CVEMaterial material = Model.Material;
            GameObject parent = Model.gameObject;
            DestroyImmediate(Model);
            edk.CVEPrimitive primitive = parent.AddComponent<edk.CVEPrimitive>();
            primitive.Material = material;
        }


        [MenuItem("CONTEXT/CVEPrimitive/Add or Switch Collider")]
        private static void primitive_AddCollider(MenuCommand menuCommand)
        {
            var Primitive = menuCommand.context as edk.CVEPrimitive;
            Collider collider = Primitive.gameObject.GetComponent<Collider>();
            if (collider) DestroyImmediate(collider);
            Primitive.gameObject.AddComponent<MeshCollider>().sharedMesh = Primitive.preview.GetComponent<MeshFilter>().sharedMesh;
        }

        #endregion
    }
}
#endif