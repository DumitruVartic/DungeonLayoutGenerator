using UI;
using UnityEngine;

public class MapLoader : LoadSceneButton
{
    Map.Generator generatedMap;

    private void GetInstance()
    {
        generatedMap = GameObject.Find("Generator").GetComponent<Map.Generator>();
    }

    public void LoadInterior()
    {
        GetInstance();
        generatedMap.InteriorLoading();
        LoadTargetScene();
    }

    public void LoadExterior()
    {
        GetInstance();
        generatedMap.ExteriorLoading();
        LoadTargetScene();
    }
}