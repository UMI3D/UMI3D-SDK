using UnityEditor;
using UnityEngine;

namespace umi3d.edk.editor
{
    [CustomPropertyDrawer(typeof(MaterialOverrider.OverridedMaterialList), false)]
    public class MaterialsOverriderDrawer : PropertyDrawer
    {

        SerializedProperty overrideAllMaterial;
        SerializedProperty overrideMaterials;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Create property fields.
            overrideAllMaterial = property.FindPropertyRelative("overrideAllMaterial");
            overrideMaterials = property.FindPropertyRelative("overidedMaterials");

            if (overrideAllMaterial.boolValue)
            {
                EditorGUI.PropertyField(position, overrideAllMaterial);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }

        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount = 1;
            if (!property.isExpanded)
                lineCount = 1;
            else
            {
                //   if(overrideAllMaterial == null)
                overrideAllMaterial = property.FindPropertyRelative("overrideAllMaterial");
                //    if(overrideMaterials == null)
                overrideMaterials = property.FindPropertyRelative("overidedMaterials");
                //     lineCount += 2 + extraline;// + overrideMaterials.arraySize;
                if (overrideAllMaterial.boolValue)
                    lineCount = 1;
                else
                    lineCount = 2 + overrideMaterials.CountInProperty();
            }

            return EditorGUIUtility.singleLineHeight * lineCount + EditorGUIUtility.standardVerticalSpacing * (lineCount - 1);
        }
    }

}
