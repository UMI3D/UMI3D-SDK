/*
Copyright 2019 Gfi Informatique

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrowserDesktop.Theme
{
    public class Theme : umi3d.PersistentSingleton<Theme>
    {
        [SerializeField] ThemeInstance _Theme = null;

        public static Color PrincipalColor { get { return Exist && Instance?._Theme != null && Instance?._Theme?.PrincipalColor != null ? Instance._Theme.PrincipalColor : Color.black; } }
        public static Color SecondaryColor { get { return Exist && Instance?._Theme != null && Instance?._Theme?.SecondaryColor != null ? Instance._Theme.SecondaryColor : Color.white; } }
        public static Color PrincipalTextColor { get { return Exist && Instance?._Theme != null && Instance?._Theme?.PrincipalTextColor != null ? Instance._Theme.PrincipalTextColor : Color.white; } }
        public static Color SecondaryTextColor { get { return Exist && Instance?._Theme != null && Instance?._Theme?.SecondaryTextColor != null ? Instance._Theme.SecondaryTextColor : Color.black; } }
        public static Color SeparatorColor { get { return Exist && Instance?._Theme != null && Instance?._Theme?.SeparatorColor != null ? Instance._Theme.SeparatorColor : Color.grey; } }

        private void Start()
        {
            _ApplyTheme();
        }

        private void OnValidate()
        {
            _ApplyTheme();
        }

        void _ApplyTheme()
        {
            if (!Exist)
            {
                Instance = this;
            }
            if (_Theme != null)
                foreach (var listener in FindObjectsOfType<ThemeListener>())
                    listener.ApplyTheme();
        }

        public static void ApplyTheme()
        {
            if (Exist) Instance._ApplyTheme();
        }
    }
}
