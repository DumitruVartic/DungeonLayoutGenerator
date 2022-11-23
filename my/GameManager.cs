using UnityEngine;
namespace Manager;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private GameManager() { }
    public static GameManager GetInstance()
    {
        if (instance == null) // instance??= new GameManager();
        {
            instance = new GameManager();
        }

        return instance;
    }

    private void Update()
    {

    }
}
