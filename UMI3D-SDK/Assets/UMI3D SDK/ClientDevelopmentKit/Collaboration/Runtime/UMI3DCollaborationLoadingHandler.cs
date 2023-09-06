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

using umi3d.cdk.collaboration.emotes;
using umi3d.cdk.collaboration.userCapture;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// <inheritdoc/>
    /// Specialized for collaborative browsers.
    /// </summary>
    public class UMI3DCollaborationLoadingHandler : UMI3DLoadingHandler
    {
        #region Emotes

        private IEmoteService emoteService;

        #endregion Emotes

        public AbstractNavigation navigation;

        protected override void Awake()
        {
            // LOADING SERVICE
            environmentLoaderService = UMI3DCollaborationEnvironmentLoader.Instance;

            // EMOTES SERVICE
            if ((parameters as UMI3DCollabLoadingParameters).areEmotesSupported)
            {
                emoteService = EmoteManager.Instance;
                emoteService.DefaultIcon = (parameters as UMI3DCollabLoadingParameters).defaultEmoteIcon;
            }

            // SKELETON SERVICE
            CollaborationSkeletonsManager.Instance.navigation = navigation; //also use to init manager via Instance call

            base.Awake();
        }
    }
}