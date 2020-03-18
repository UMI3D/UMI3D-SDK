using UnityEngine;
using UnityEngine.UI;

namespace BrowserDesktop
{
    public class ActionIcon : MonoBehaviour
    {
        [SerializeField]
        Image ActiveBackground = null;
        [SerializeField]
        Image NotActiveBackground = null;

        public void State(bool active)
        {
            ActiveBackground.enabled = active;
            NotActiveBackground.enabled = !active;
        }
    }
}