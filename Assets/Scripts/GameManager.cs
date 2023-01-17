using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool enableFPS;
    public bool enableInterior;

    private GameManager()
    {
        enableFPS = false;
        enableInterior = false;
    }

    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameManager();
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ToggleFpsButton(bool tog)
    {
        enableFPS = tog;
    }

    public void ToggleInteriorButton(bool tog)
    {
        enableInterior = tog;
    }

    bool wasTp = false;
    private void FixedUpdate()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        string sceneName = currentScene.name;

        if (sceneName == "Dungeon3D" && !wasTp)
        {
            Map.Generator generatedMap = GameObject.Find("Generator").GetComponent<Map.Generator>();
            if (generatedMap)
            {
                wasTp = true;

                generatedMap.TeleportPlayerInside();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else if (sceneName == "Playground" && wasTp)
        {
            wasTp = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Cursor.visible == false)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}