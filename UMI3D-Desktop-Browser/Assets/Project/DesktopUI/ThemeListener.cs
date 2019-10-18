using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesktopUI
{
    public abstract class ThemeListener : MonoBehaviour
    {
        private void Awake()
        {
            ApplyTheme();
        }

        private void OnValidate()
        {
            ApplyTheme();
        }

        public abstract void ApplyTheme();
    }

}
