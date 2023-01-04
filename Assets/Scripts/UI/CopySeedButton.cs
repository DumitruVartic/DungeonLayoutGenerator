using TMPro;
using UnityEngine;

namespace UI
{
    public class CopySeedButton : MonoBehaviour
    {
        [SerializeField] TMP_InputField seedData;

        public void CopyData()
        {
            GUIUtility.systemCopyBuffer = seedData.text;
        }
    }
}