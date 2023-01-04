using TMPro;
using UnityEngine;

namespace UI
{
    public class OutputSeedButton : MonoBehaviour
    {
        [SerializeField] MenuData menuData;
        [SerializeField] TMP_InputField seedData;

        public void OutputData()
        {
            seedData.text = menuData.Seed;
        }
    }
}