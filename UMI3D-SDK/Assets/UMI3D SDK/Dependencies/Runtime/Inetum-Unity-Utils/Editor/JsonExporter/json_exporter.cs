using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using umi3d.common.userCapture.pose;
using System.IO;

public class json_exporter : EditorWindow
{
    [SerializeField] VisualTreeAsset treeAsset;
    ObjectField of_objectToSerialize = null;
    ObjectField of_objectToDeserialize = null;
    TextField tf_path = null;
    Button b_Serialize = null;
    Button b_Deserialize = null;

    [MenuItem("Inetum/Utils/json_exporter")]
    public static void ShowExample()
    {
        json_exporter wnd = GetWindow<json_exporter>();
        wnd.titleContent = new GUIContent("json_exporter");
    }

    public void CreateGUI()
    {
        treeAsset.CloneTree(rootVisualElement);

        GetRefs();
        SetRefs();
    }

    private void GetRefs()
    {
        of_objectToSerialize = rootVisualElement.Q<ObjectField>("serializable");
        of_objectToDeserialize = rootVisualElement.Q<ObjectField>("deserializable");
        tf_path = rootVisualElement.Q<TextField>();
        b_Serialize = rootVisualElement.Q<Button>("serialize");
        b_Deserialize = rootVisualElement.Q<Button>("deserialize");
    }

    private void SetRefs()
    {
        of_objectToSerialize.objectType = typeof(ScriptableObject);
        of_objectToSerialize.RegisterValueChangedCallback((obj) =>
        {
            if (obj.newValue is not IJsonSerializer) 
            {
                Debug.Log("<color=red> your objecct dosent implements IJsonSerializer");
            }
        });

        of_objectToDeserialize.objectType = typeof(TextAsset);

        b_Serialize.clicked += () =>
        {
            Serialize();
        };

        b_Deserialize.clicked += () =>
        {
            Deserialize();
        };
    }

    private void Serialize()
    { 
        string jsonString = (of_objectToSerialize.value as IJsonSerializer)?.JsonSerialize();
        string savePath = Path.Combine(Application.dataPath, tf_path.text, of_objectToSerialize.value.name + ".json");
        using StreamWriter sw = new StreamWriter(savePath);
        sw.Write(jsonString);
    }
        
    private void Deserialize()
    {
        string savePath = Path.Combine(Application.dataPath, tf_path.text);
        if (of_objectToSerialize.value == null)
        {
            Debug.Log("please add a file as a reference in the Object to serialize of the jeson exporter");
        }

        ScriptableObject so = (of_objectToSerialize.value as IJsonSerializer)?.JsonDeserializeScriptableObject((of_objectToDeserialize.value as TextAsset).text);
        Debug.Log(Path.Combine(savePath, of_objectToDeserialize.value.name + ".asset"));
        Debug.Log("error when creating the assets");
        //AssetDatabase.CreateAsset(, Path.Combine(savePath, of_objectToDeserialize.value.name + ".asset"));
        //AssetDatabase.SaveAssets();
        //EditorUtility.SetDirty(so);
    }
}