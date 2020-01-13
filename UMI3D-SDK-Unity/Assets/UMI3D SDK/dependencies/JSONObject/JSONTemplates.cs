using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

/*
 * http://www.opensource.org/licenses/lgpl-2.1.php
 * JSONTemplates class
 * for use with Unity
 * Copyright Matt Schoen 2010
 */

public static partial class JSONTemplates {
	static readonly HashSet<object> touched = new HashSet<object>();

	public static umi3d.common.JSONObject TOJSON(object obj) {		//For a generic guess
		if(touched.Add(obj)) {
			umi3d.common.JSONObject result = umi3d.common.JSONObject.obj;
			//Fields
			FieldInfo[] fieldinfo = obj.GetType().GetFields();
			foreach(FieldInfo fi in fieldinfo) {
				umi3d.common.JSONObject val = umi3d.common.JSONObject.nullJO;
				if(!fi.GetValue(obj).Equals(null)) {
					MethodInfo info = typeof(JSONTemplates).GetMethod("From" + fi.FieldType.Name);
					if(info != null) {
						object[] parms = new object[1];
						parms[0] = fi.GetValue(obj);
						val = (umi3d.common.JSONObject)info.Invoke(null, parms);
					} else if(fi.FieldType == typeof(string))
						val = umi3d.common.JSONObject.CreateStringObject(fi.GetValue(obj).ToString());
					else
						val = umi3d.common.JSONObject.Create(fi.GetValue(obj).ToString());
				}
				if(val) {
					if(val.type != umi3d.common.JSONObject.Type.NULL)
						result.AddField(fi.Name, val);
					else Debug.LogWarning("Null for this non-null object, property " + fi.Name + " of class " + obj.GetType().Name + ". Object type is " + fi.FieldType.Name);
				}
			}
			//Properties
			PropertyInfo[] propertyInfo = obj.GetType().GetProperties();
			foreach(PropertyInfo pi in propertyInfo) {
				//This section should mirror part of AssetFactory.AddScripts()
				umi3d.common.JSONObject val = umi3d.common.JSONObject.nullJO;
				if(!pi.GetValue(obj, null).Equals(null)) {
					MethodInfo info = typeof(JSONTemplates).GetMethod("From" + pi.PropertyType.Name);
					if(info != null) {
						object[] parms = new object[1];
						parms[0] = pi.GetValue(obj, null);
						val = (umi3d.common.JSONObject)info.Invoke(null, parms);
					} else if(pi.PropertyType == typeof(string))
						val = umi3d.common.JSONObject.CreateStringObject(pi.GetValue(obj, null).ToString());
					else
						val = umi3d.common.JSONObject.Create(pi.GetValue(obj, null).ToString());
				}
				if(val) {
					if(val.type != umi3d.common.JSONObject.Type.NULL)
						result.AddField(pi.Name, val);
					else Debug.LogWarning("Null for this non-null object, property " + pi.Name + " of class " + obj.GetType().Name + ". Object type is " + pi.PropertyType.Name);
				}
			}
			return result;
		} 
		Debug.LogWarning("trying to save the same data twice");
		return umi3d.common.JSONObject.nullJO;
	}
}

/*
 * Some helpful code templates for the JSON class
 * 
 * LOOP THROUGH OBJECT
for(int i = 0; i < obj.Count; i++){
	if(obj.keys[i] != null){
		switch((string)obj.keys[i]){
			case "key1":
				do stuff with (umi3d.common.JSONObject)obj.list[i];
				break;
			case "key2":
				do stuff with (umi3d.common.JSONObject)obj.list[i];
				break;		
		}
	}
}
 *
 * LOOP THROUGH ARRAY
foreach(umi3d.common.JSONObject ob in obj.list)
	do stuff with ob;
 */
