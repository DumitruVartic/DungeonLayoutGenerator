using UnityEngine;

namespace UI
{
    public class ToggleData : MonoBehaviour
    {
        [SerializeField] Map.Generator generator;
        [SerializeField] bool currentState;

        public void ToggleExtraEdges()
        {
            currentState ^= true;
            generator.addExtraEdges = currentState;
        }
    }
}