using UnityEngine;

namespace UI
{
    public class GenerateButton : MonoBehaviour
    {
        Map.Generator generatedMap;
        [SerializeField] MenuData menuData;
        [SerializeField] Player.RotatorPlacement cameraPosition;

        private void GetInstance()
        {
            generatedMap = GameObject.Find("Generator").GetComponent<Map.Generator>();
        }

        public void Generate()
        {
            GetInstance();
            generatedMap.Generate(menuData.Seed);
            cameraPosition.PlaceCamera();
        }
    }
}