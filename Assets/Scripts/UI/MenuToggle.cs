using UnityEngine;

namespace UI
{
    public class MenuToggle : MonoBehaviour
    {
        public GameObject panel;

        public void OpenPanel()
        {
            if (panel != null)
            {
                bool isActive = panel.activeSelf;
                panel.SetActive(!isActive);
            }
        }
    }
}