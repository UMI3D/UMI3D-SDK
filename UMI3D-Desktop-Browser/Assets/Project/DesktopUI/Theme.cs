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

namespace DesktopUI
{
    public class Theme : MonoBehaviour
    {
        [SerializeField] protected Color _PrincipalColor = Color.black;
        [SerializeField] protected Color _PrincipalTextColor = Color.white;
        [Space]
        [SerializeField] protected Color _SecondaryColor = Color.white;
        [SerializeField] protected Color _SecondaryTextColor = Color.black;
        [Space]
        [SerializeField] protected Color _SeparatorColor = Color.grey;

        static Theme instance;

        public static Color PrincipalColor { get { return instance == null ? Color.black : instance._PrincipalColor; } }
        public static Color SecondaryColor { get { return instance == null ? Color.white : instance._SecondaryColor; } }
        public static Color PrincipalTextColor { get { return instance == null ? Color.white : instance._PrincipalTextColor; } }
        public static Color SecondaryTextColor { get { return instance == null ? Color.black : instance._SecondaryTextColor; } }
        public static Color SeparatorColor { get { return instance == null ? Color.grey : instance._SeparatorColor; } }

        // Use this for initialization
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
                Destroy(this);
        }

        private void Start()
        {
            ApplyTheme();
        }

        private void OnValidate()
        {
            ApplyTheme();
        }

        void ApplyTheme()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
                Destroy(this);
            foreach (var listener in FindObjectsOfType<ThemeListener>())
                listener.ApplyTheme();

        }

    }

}
