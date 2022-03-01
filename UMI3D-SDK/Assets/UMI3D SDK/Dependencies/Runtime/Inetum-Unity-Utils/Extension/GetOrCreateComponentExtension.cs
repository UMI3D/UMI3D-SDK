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

using UnityEngine;

namespace inetum.unityUtils
{

    public static class GetOrCreateComponentExtension
    {

        public static A GetOrAddComponent<A>(this GameObject gameObject) where A : Component
        {
            System.Type type = typeof(A);
            Component _comp = gameObject.GetComponent(type);
            if (_comp == null)
                _comp = gameObject.AddComponent(type);
            return _comp as A;
        }

    }

}