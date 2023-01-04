using UnityEngine;

namespace Player
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] float rotateSpeed;

        void Update()
        {
            transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        }
    }
}