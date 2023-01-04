using TMPro;
using UnityEngine;

namespace UI
{
    public class PasteSeedButton : MonoBehaviour
    {
        [SerializeField] TMP_InputField seedData;

        public void PasteData()
        {
            seedData.text = GUIUtility.systemCopyBuffer;
        }
    }
}