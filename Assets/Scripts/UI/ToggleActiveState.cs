using UnityEngine;
using UnityEngine.UI;

public class ToggleActiveState : MonoBehaviour
{
    public Button button;

    private void Start()
    {
        if (!GameManager.Instance.enableFPS)
        {
            GameObject parent = button.gameObject;
            parent.SetActive(false);
        }
    }
}
