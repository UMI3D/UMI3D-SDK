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
using System;
using umi3d.common;
using umi3d.edk;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(CVELight), true)]
    [CanEditMultipleObjects]
    public class CVELightEditor : GenericObject3DEditor
    {
        SerializedProperty isStatic;
        SerializedProperty type;
        SerializedProperty color;
        SerializedProperty intensity;
        SerializedProperty bounceIntensity;
        SerializedProperty range;
        SerializedProperty spotAngle;
        SerializedProperty shadowsType;
        SerializedProperty shadowsIntensity;
        SerializedProperty shadowsBiais;
        SerializedProperty shadowsNormalBiais;
        SerializedProperty shadowsNearPlane;

        umi3d.edk.CVELight Target;
        GameObject previewObject;

        SerializedProperty preview;

        bool AddPreview()
        {
            if (preview.objectReferenceValue == null)
            {
                Target.preview = new GameObject();
                Target.preview.name = Target.name + "_preview";
                Target.preview.hideFlags = HideFlags.NotEditable;
                Light l = Target.preview.AddComponent<Light>();
                l.lightmapBakeType = LightmapBakeType.Realtime;
                l.type = ((umi3d.common.LightTypes)type.enumValueIndex).Convert();
                Target.preview.transform.SetParent(Target.transform, false);
                previewObject = Target.preview;
                preview.objectReferenceValue = Target.preview;
            }
            else
            {
                previewObject = (GameObject)preview.objectReferenceValue;
            }
            return previewObject;
        }

        //void UpdateLight()
        //{
        //    if (!previewObject)
        //    {
        //        if (!AddPreview())
        //        {
        //            Debug.LogError("preview is still null");
        //            return;
        //        }
        //        Light light = previewObject.GetComponent<Light>();
        //        if (RenderSettings.ambientMode != AmbientMode.Skybox) RenderSettings.ambientMode = AmbientMode.Skybox;
        //        light.lightmapBakeType = LightmapBakeType.Realtime;
        //        Debug.Log(light.lightmapBakeType);
        //        light.color = color.colorValue;
        //        light.intensity = intensity.floatValue;
        //        light.bounceIntensity = bounceIntensity.floatValue;
        //        light.range = range.floatValue;
        //        light.spotAngle = 360f * 0.5f * spotAngle.floatValue / (float)Math.PI;
        //        light.shadowBias = shadowsBiais.floatValue;
        //        light.shadowNormalBias = shadowsNormalBiais.floatValue;
        //        light.shadowNearPlane = shadowsNearPlane.floatValue;
        //        switch ((ShadowType)shadowsType.enumValueIndex)
        //        {
        //            case ShadowType.Hard:
        //                light.shadows = LightShadows.Hard;
        //                break;
        //            case ShadowType.Soft:
        //                light.shadows = LightShadows.Soft;
        //                break;
        //            case ShadowType.None:
        //            default:
        //                light.shadows = LightShadows.None;
        //                break;
        //        }
        //    }
        //}

        protected override void OnEnable()
        {
            base.OnEnable();
            isStatic = serializedObject.FindProperty("isStatic");
            type = serializedObject.FindProperty("_type");
            color = serializedObject.FindProperty("color");
            intensity = serializedObject.FindProperty("intensity");
            bounceIntensity = serializedObject.FindProperty("bounceIntensity");
            range = serializedObject.FindProperty("range");
            spotAngle = serializedObject.FindProperty("spotAngle");
            shadowsType = serializedObject.FindProperty("shadowsType");
            shadowsBiais = serializedObject.FindProperty("shadowsBiais");
            shadowsIntensity = serializedObject.FindProperty("shadowsIntensity");
            shadowsNormalBiais = serializedObject.FindProperty("shadowsNormalBiais");
            shadowsNearPlane = serializedObject.FindProperty("shadowsNearPlane");
            preview = serializedObject.FindProperty("preview");
            Target = (umi3d.edk.CVELight)target;
            previewObject = (GameObject)preview.objectReferenceValue;
            if (!previewObject) AddPreview();
        }



        /*
        Ambient = 0,
        Directional = 1,
        Point = 2,
        Spot = 3
        */

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isStatic);
            EditorGUILayout.PropertyField(type);
            int vType = type.enumValueIndex;
            EditorGUILayout.PropertyField(color);
            EditorGUILayout.PropertyField(intensity);
            EditorGUILayout.PropertyField(bounceIntensity);

            if (vType == (int)LightTypes.Spot || vType == (int)LightTypes.Point)
                EditorGUILayout.PropertyField(range);
            if (vType == (int)LightTypes.Spot)
                EditorGUILayout.Slider(spotAngle, 0, (float)Math.PI);

            EditorGUILayout.PropertyField(shadowsType);
            if (shadowsType.enumValueIndex != (int)ShadowType.None)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Slider(shadowsIntensity, 0, 1f);
                EditorGUILayout.Slider(shadowsBiais, 0, 2f);
                EditorGUILayout.Slider(shadowsNormalBiais, 0, 3f);
                EditorGUILayout.Slider(shadowsNearPlane, 0, 10f);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                //UpdateLight();
                serializedObject.ApplyModifiedProperties();
            }

        }


        void OnSceneGUI()
        {
            Color c = Color.yellow;
            //c.a = 0.5f;
            Handles.color = c;


            CVELight scr = (CVELight)target;

            Vector3 position = scr.transform.position;
            Vector3 CamPosition = Camera.current.transform.position;
            float tmp = -1;
            switch (scr._type)
            {
                case LightTypes.Directional:
                    Handles.DrawWireDisc(position, scr.transform.forward, 0.20f * HandleUtility.GetHandleSize(position));
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 p1 = position + Quaternion.Euler(0, 0, i * 360 / 8) * scr.transform.up * 0.20f * HandleUtility.GetHandleSize(position);
                        Vector3 p2 = p1 + scr.transform.forward * 1f * HandleUtility.GetHandleSize(position);
                        Handles.DrawLine(p1, p2);
                    }
                    break;
                case LightTypes.Point:
                    tmp = Handles.RadiusHandle(Quaternion.identity, position, scr.range);
                    if (tmp != scr.range && tmp > 0)
                    {
                        scr.range = tmp;
                        scr.OnValidate();
                    }
                    break;
                case LightTypes.Spot:
                    float handleSize = 0.03f;
                    Vector3 pos = position + scr.transform.forward * scr.range;
                    tmp = Vector3.Distance(position,
                        Handles.FreeMoveHandle(pos, Quaternion.identity, handleSize * HandleUtility.GetHandleSize(pos), new Vector3(.005f, .005f, .005f), Handles.DotHandleCap)
                        );
                    float angle = 360f * 0.5f * scr.spotAngle / (float)Math.PI /2;
                    float h = Mathf.Abs( scr.range / Mathf.Cos(scr.spotAngle/2));
                    //Debug.Log(angle + " " + Mathf.Cos(angle) + " " + scr.range + " " + h);
                    Vector3 p = position;
                    if(tmp != scr.range && tmp > 0)
                    {
                        scr.range = tmp;
                        scr.OnValidate();
                    }
                    tmp = -1;

                    for (int i = 0;i < 4; i++)
                    {
                        p = position + scr.transform.rotation * Quaternion.Euler(0, 0, i * 360 / 4) * Quaternion.Euler(angle, 0, 0) * Vector3.forward  *h;
                        Handles.DrawLine(position, p);
                        Vector3 v1 = Handles.FreeMoveHandle(p, Quaternion.identity, handleSize * HandleUtility.GetHandleSize(p), new Vector3(.005f, .005f, .005f), Handles.DotHandleCap);

                        Plane plane = new Plane(pos - position, pos);
                        Vector3 pp = plane.ClosestPointOnPlane(p);
                        Vector3 pv = plane.ClosestPointOnPlane(v1);
                        float dp = Vector3.Distance(pos, pp);
                        float dv = Vector3.Distance(pos, pv);

                        if(dp != dv)
                        {
                            tmp = Mathf.Atan2(dv, scr.range)*2;
                        }

                    }
                    if (tmp > 0)
                    {
                        scr.spotAngle = tmp;
                        scr.OnValidate();
                    }

                    Handles.DrawWireDisc(pos,pos-position,Vector3.Distance(pos,p));


                    // Handles.FreeMoveHandle(position, Quaternion.identity, handleSize, 0.01f * Vector3.one, Handles.DotHandleCap);

                    break;


            }
            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }


    }

    //public class CVELightGizmoDrawer
    //{

    //    [DrawGizmo(GizmoType.Selected)]
    //    static void DrawGizmoForMyScript(CVELight scr, GizmoType gizmoType)
    //    {
            

    //    }

        //public static bool PlanePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, Vector3 plane1Normal, Vector3 plane1Position, Vector3 plane2Normal, Vector3 plane2Position)
        //{

        //    linePoint = Vector3.zero;
        //    lineVec = Vector3.zero;

        //    //We can get the direction of the line of intersection of the two planes by calculating the 
        //    //cross product of the normals of the two planes. Note that this is just a direction and the line
        //    //is not fixed in space yet. We need a point for that to go with the line vector.
        //    lineVec = Vector3.Cross(plane1Normal, plane2Normal);

        //    //Next is to calculate a point on the line to fix it's position in space. This is done by finding a vector from
        //    //the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
        //    //errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
        //    //the cross product of the normal of plane2 and the lineDirection.		
        //    Vector3 ldir = Vector3.Cross(plane2Normal, lineVec);

        //    float denominator = Vector3.Dot(plane1Normal, ldir);

        //    //Prevent divide by zero and rounding errors by requiring about 5 degrees angle between the planes.
        //    if (Mathf.Abs(denominator) > 0.006f)
        //    {

        //        Vector3 plane1ToPlane2 = plane1Position - plane2Position;
        //        float t = Vector3.Dot(plane1Normal, plane1ToPlane2) / denominator;
        //        linePoint = plane2Position + t * ldir;

        //        return true;
        //    }

        //    //output not valid
        //    else
        //    {
        //        return false;
        //    }
        //}

    //}

}
#endif
      