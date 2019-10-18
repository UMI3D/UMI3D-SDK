using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;


namespace DesktopUI
{

    public class BasicList : MonoBehaviour
    {

        public GameObject prefab;
        public GameObject inputPrefab;
        public GameObject rangePrefab;
        public GameObject checkPrefab;
        public GameObject selectPrefab;
        public string xmlPath = null;

        [SerializeField] GameObject content = null;
        RectTransform _content_rect;

        public int Count { get { return content.transform.childCount; } }

        public void ScrollTop()
        {
            if (_content_rect == null)
                _content_rect = content.GetComponent<RectTransform>();
            _content_rect.localPosition += new Vector3(0, -_content_rect.localPosition.y, 0);
        }

        public virtual void AddItem(GameObject go)
        {
            go.transform.SetParent(content.transform, false);
        }

        public GameObject CreateItem(string title, bool write = true)
        {
            return Create(prefab, title, write);
        }

        public GameObject CreateInput(string title, bool write = true)
        {
            return Create(inputPrefab, title, write);
        }

        public GameObject CreateRange(string title, bool write = true)
        {
            return Create(rangePrefab, title, write);
        }

        public GameObject CreateCheckbox(string title, bool write = true)
        {
            return Create(checkPrefab, title, write);
        }

        public GameObject CreateSelect(string title, bool write = true)
        {
            return Create(selectPrefab, title, write);
        }

        GameObject Create(GameObject prefab, string title, bool write = true)
        {
            //if (title == null || title.Length == 0)
            //    return null;
            var go = Instantiate(prefab);
            var item = go.GetComponent<ListItem>();
            AddItem(go);
            if (item != null)
            {
                item.label.text = title;
                if (write)
                    WriteXml();
            }
            return go;
        }

        public virtual void RemoveItem(GameObject go)
        {
            DestroyImmediate(go);
            WriteXml();
        }

        public virtual void ClearItems()
        {
            foreach (Transform t in content.transform)
                Destroy(t.gameObject);
        }

        public class BasicXmlList
        {
            [XmlArray(ElementName = "values")]
            [XmlArrayItem(ElementName = "url")]
            public List<string> values = new List<string>();
        }

        protected virtual void WriteXml()
        {
            if (!HasXml())
                return;
            var xml = new BasicXmlList();
            xml.values.AddRange(GetComponentsInChildren<ListItem>(true).Where(item => item != null).Select((item)=>item.label.text));
            var serializer = new XmlSerializer(typeof(BasicXmlList));
            var stream = new FileStream(xmlPath, FileMode.Create);
            serializer.Serialize(stream, xml);
            stream.Close();
        }

        protected virtual void LoadXml()
        {
            if (!HasXml())
                return;
            ClearItems();
            var serializer = new XmlSerializer(typeof(BasicXmlList));
            if (File.Exists(xmlPath))
            {
                var stream = new FileStream(xmlPath, FileMode.Open);
                var xml = serializer.Deserialize(stream) as BasicXmlList;
                foreach (var l in xml.values)
                    CreateItem(l,false);
                stream.Close();
            }
        }

        public bool HasXml()
        {
            return xmlPath != null && xmlPath.IndexOf(".xml") > 0;
        }

        public void Awake()
        {
            if(HasXml())
                LoadXml();
        }

    }

}