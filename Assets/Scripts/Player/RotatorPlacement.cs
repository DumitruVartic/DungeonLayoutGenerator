using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class RotatorPlacement : MonoBehaviour
    {
        [SerializeField] UI.MenuData menuData;
        [SerializeField] Transform cameraHolder;
        [SerializeField] Transform mainCamera;
        [SerializeField] Rotator rotator;
        Vector3Int size;

        public void PlaceCamera()
        {
            rotator.enabled = false;
            cameraHolder.position = Vector3.zero;
            cameraHolder.rotation = Quaternion.identity;
            mainCamera.position = Vector3.zero;
            size = menuData.Size;
            int max = GetMax(size.x / 2, size.y / 2, size.z / 2);
            int medie = (size.x + size.y + size.z) / 3;
            Vector3Int newPosition = new Vector3Int(max * -1, medie, max * -1);
            mainCamera.position = newPosition;
            cameraHolder.position = new Vector3Int(size.x / 2, size.y / 2, size.z / 2);
            rotator.enabled = true;
        }

        int GetMax(int x, int y, int z)
        {
            List<int> list = new List<int>{ x, y, z };  
            int max = x;
            foreach (var item in list)
            {
                if (max < item)
                    max = item;
            }
            return max;
        }
    }
}