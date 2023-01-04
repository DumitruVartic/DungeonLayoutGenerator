using UnityEngine;

public class ToggleDynamicaly : MonoBehaviour
{
    public GameObject target;
    [SerializeField] bool currentState;

    public void ToggleState()
    {
        currentState ^= true;
        target.SetActive(currentState);
    }
}