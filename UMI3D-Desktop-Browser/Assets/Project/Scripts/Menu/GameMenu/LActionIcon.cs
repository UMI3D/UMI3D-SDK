using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BrowserDesktop
{
    public class LActionIcon : ActionIcon
    {
        public Text Text;

        public void Set(KeyCode Action)
        {
            Text.text = Action.ToString();
        }
    }
}