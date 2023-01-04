using TMPro;
using UnityEngine;

namespace UI
{
    public class InputSeedButton : MonoBehaviour
    {
        [SerializeField] MenuData menuData;
        [SerializeField] TMP_InputField seedData;

        public void InputData()
        {
            menuData.Seed = seedData.text;
        }
    }
}