﻿/*
Copyright 2019 - 2021 Inetum

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebSocketSharp.Server;

namespace umi3d.edk.collaboration
{
    public class HttpRoutingUtil
    {

        public Type attributeType { get; private set; }

        public List<WebServiceMethod> roots = new List<WebServiceMethod>();

        public HttpRoutingUtil(Type attributeType) : this(null, attributeType) { }

        public HttpRoutingUtil(List<IHttpApi> apis, Type attributeType)
        {
            Clear();
            this.attributeType = attributeType;
            if (apis != null)
                AddRoot(apis);
        }

        public void AddRoot(IHttpApi api) { AddRoot(new List<IHttpApi>() { api }); }

        public void AddRoot(List<IHttpApi> apis)
        {
            foreach (IHttpApi api in apis)
            {
                MethodInfo[] methods = api.GetType().GetMethods();
                foreach (MethodInfo method in methods)
                {
                    object[] attrArray = method.GetCustomAttributes(attributeType, false);
                    WebServiceMethodAttribute attribute = (attrArray == null || attrArray.Length == 0) ? null : (WebServiceMethodAttribute)attrArray[0];
                    if (attribute != null)
                    {
                        //TODO: improve this to handle equals regex
                        if (roots.Count(r => r.attribute.path == attribute.path) > 0)
                        {
                            throw new Exception("Multiple Attribute path [" + attribute.path + "] detected!");
                        }
                        roots.Add(new WebServiceMethod(api, method, attribute));
                    }
                }
            }
        }

        public bool TryProccessRequest(object sender, HttpRequestEventArgs e)
        {
            string path = e.Request.RawUrl;
            foreach (WebServiceMethod r in roots)
            {
                if (r.attribute.Match(path))
                {
                    r.ProcessRequest(path, sender, e);
                    return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            roots.Clear();
        }
    }
}