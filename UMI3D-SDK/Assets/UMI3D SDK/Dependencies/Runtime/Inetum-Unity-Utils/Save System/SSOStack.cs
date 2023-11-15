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

namespace inetum.unityUtils.saveSystem
{
    /// <summary>
    /// <see cref="SerializableScriptableObject"/>([SSO] in the context) stack.<br/>
    /// Use to save SSO with the SSOSaveSystem.
    /// </summary>
    [System.Serializable]
    public struct SSOStack
    {
        public string guid;
        public int amount;

        public SSOStack(string guid, int amount)
        {
            this.guid = guid;
            this.amount = amount;
        }

        public SSOStack(SerializableScriptableObject sso, int amount)
        {
            this.guid = sso.Guid;
            this.amount = amount;
        }
    }
}
