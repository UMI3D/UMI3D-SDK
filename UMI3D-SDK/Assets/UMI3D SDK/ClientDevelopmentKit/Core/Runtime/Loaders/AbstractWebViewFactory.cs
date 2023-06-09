/*
Copyright 2019 - 2023 Inetum

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

using inetum.unityUtils;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Creates <see cref="AbstractUMI3DWebView"/>.
    /// </summary>
    public abstract class AbstractWebViewFactory : SingleBehaviour<AbstractWebViewFactory>
    {
        Queue<AbstractUMI3DWebView> webViews = new Queue<AbstractUMI3DWebView>();
        
        public AbstractUMI3DWebView GetWebView()
        {
            if (webViews.Count > 0)
            {
                return webViews.Dequeue();
            }
            else
            {
                return CreateWebView();
            }
        }

        protected abstract AbstractUMI3DWebView CreateWebView();
    }

}

