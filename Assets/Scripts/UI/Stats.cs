using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    public Text gameFPS;

    private float fpsCounter = 0;
    private float currentFpsTime = 0;
    private float fpsShowPeriod = 1;

    private void Start()
    {
        if (!GameManager.Instance.enableFPS)
        {
            GameObject parent = gameFPS.gameObject;
            parent.SetActive(false);
        }
    }

    void Update()
    {
        currentFpsTime = currentFpsTime + Time.deltaTime;
        fpsCounter = fpsCounter + 1;
        if (currentFpsTime > fpsShowPeriod)
        {
            gameFPS.text = fpsCounter.ToString();
            currentFpsTime = 0;
            fpsCounter = 0;
        }
    }
}