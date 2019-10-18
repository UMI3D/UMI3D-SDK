using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopUI
{

    public class ApplicationBarHeader : ThemeListener {

        public ApplicationBarTab tab;
        public Image background;
        public Text label;

        // Use this for initialization
        void Start()
        {
            var tabs = GetComponentInParent<ApplicationBar>();
            GetComponent<Button>().onClick.AddListener(() => tabs.ShowTab(tab));
        }

        private void Update()
        {
            if(background != null && tab != null)
                background.color = tab.gameObject.activeInHierarchy ? Theme.SecondaryColor : new Color(0,0,0,0);
            if (label != null && tab != null)
                label.color = tab.gameObject.activeInHierarchy ? Theme.SecondaryTextColor : Theme.PrincipalTextColor;
        }

        public override void ApplyTheme()
        {
            Update();
        }

    }

}