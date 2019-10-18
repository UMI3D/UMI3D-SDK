using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesktopUI
{

    public class ApplicationBar : MonoBehaviour
    {
        public ApplicationBarTab defaultTab = null;

        public void ShowTab(ApplicationBarTab tab, bool toggle = true)
        {
            var tabs = GetComponentsInChildren<ApplicationBarTab>(true);
            foreach (ApplicationBarTab _tab in tabs)
                _tab.gameObject.SetActive(tab == _tab && (!_tab.gameObject.activeInHierarchy || !toggle));
        }

        // Use this for initialization
        void Awake()
        {
            ShowTab(defaultTab);
        }
    }

}