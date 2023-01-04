using UnityEngine;

namespace UI
{
    public class PanelOpener : MonoBehaviour
    {
        public GameObject panel;

        public void OpenPanel()
        {
            if (panel != null)
            {
                Animator animator = panel.GetComponent<Animator>();
                if (animator != null )
                {
                    bool isOpen = animator.GetBool("open");

                    animator.SetBool("open", !isOpen);
                }
            }
        }
    }
}