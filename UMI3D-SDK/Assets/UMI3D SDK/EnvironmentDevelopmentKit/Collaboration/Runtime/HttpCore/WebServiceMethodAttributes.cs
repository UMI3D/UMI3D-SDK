/*
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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace umi3d.edk.collaboration
{
    public abstract class WebServiceMethodAttribute : System.Attribute
    {
        public enum Security { Public, Private, PrivateAllowOldToken }
        public enum Type { Method, Directory }

        public string path = null;
        public readonly Regex regex;
        public readonly Security security = Security.Private;
        public readonly Type type = Type.Method;

        public WebServiceMethodAttribute(string path, Security security, Type type)
        {
            this.path = path;
            this.security = security;
            this.type = type;
            //compute regex
            string pathFormat = new Regex("(:([a-zA-Z0-9_\\-]+))\\b").Replace(path, "(?<$2>[a-zA-Z0-9_\\-=]+)");
            this.regex = new Regex(pathFormat);
        }

        public bool Match(string uri)
        {
            Match match = regex.Match(uri);
            bool startWith = match.Index == 0 || (match.Index == 1 && uri[0] == '/');
            int d = uri.Length - (match.Index + match.Length);
            bool endWith = d == 0 || (d == 1 && uri[match.Index + match.Length] == '/');
            return match.Success && startWith && (endWith || type == Type.Directory);
        }

        public Dictionary<string, string> GetParametersFrom(string uri)
        {
            Match match = regex.Match(uri);
            if (match.Success)
            {
                return regex.GetGroupNames().Skip(1) // Skip the "0" group
                    .Where(g => match.Groups[g].Success && match.Groups[g].Captures.Count > 0)
                        .ToDictionary(groupName => groupName, groupName => match.Groups[groupName].Value);
            }
            else
            {
                return new Dictionary<string, string>();
            }
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class HttpGet : WebServiceMethodAttribute
    {
        public HttpGet(string path, Security security, Type type) : base(path, security, type) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class HttpPost : WebServiceMethodAttribute
    {
        public HttpPost(string path, Security security, Type type) : base(path, security, type) { }
    }
}