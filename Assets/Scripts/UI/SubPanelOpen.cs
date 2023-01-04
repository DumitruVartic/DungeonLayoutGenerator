using UnityEngine;

namespace UI
{
    public class SubPanelOpen : MonoBehaviour
    {
        public GameObject panel;

        public void OpenPanel()
        {
            if (panel != null)
            {
                Animator animator = panel.GetComponent<Animator>();
                if (animator != null)
                {
                    bool isOpen = animator.GetBool("toggle");

                    animator.SetBool("toggle", !isOpen);
                }
            }
        }

        [SerializeField] bool currentState;
        public void ToggleSubPanel()
        {
            currentState ^= true;
            OpenPanel();
        }
    }
}