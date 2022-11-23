using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameManager()
    {
        // initializarea (dar sa nu ne referim la gamebject'uri ele inca nu exista)
    }

    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null) // instance??= new GameManager();
            {
                instance = new GameManager();
            }

            return instance;
        }
    }

}