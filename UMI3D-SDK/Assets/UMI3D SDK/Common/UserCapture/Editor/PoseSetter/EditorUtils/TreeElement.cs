/*
Copy 2019 - 2023 Inetum

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
using System.Collections.Generic;
using UnityEngine;


namespace UnityEditor.TreeViewExamples
{
	[Serializable]
	public class TreeElement
	{
		[SerializeField] int m_ID;
		[SerializeField] string m_Name;
		[SerializeField] int m_Depth;
		[NonSerialized] TreeElement m_Parent;
		[NonSerialized] List<TreeElement> m_Children;

		public int depth
		{
			get { return m_Depth; }
			set { m_Depth = value; }
		}

		public TreeElement parent
		{
			get { return m_Parent; }
			set { m_Parent = value; }
		}

		public List<TreeElement> children
		{
			get { return m_Children; }
			set { m_Children = value; }
		}

		public bool hasChildren
		{
			get { return children != null && children.Count > 0; }
		}

		public string name
		{
			get { return m_Name; } set { m_Name = value; }
		}

		public int id
		{
			get { return m_ID; } set { m_ID = value; }
		}

		public TreeElement ()
		{
		}

		public TreeElement (string name, int depth, int id)
		{
			m_Name = name;
			m_ID = id;
			m_Depth = depth;
		}
	}
}


#endif