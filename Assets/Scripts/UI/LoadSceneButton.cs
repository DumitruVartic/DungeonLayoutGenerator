using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class LoadSceneButton : MonoBehaviour
    {
        public string SceneName = "";

        private void Start()
        {
            gameObject.GetComponent<Button>().onClick.AddListener(LoadTargetScene);
        }

        public void LoadTargetScene()
        {
            SceneManager.LoadScene(SceneName);
        }
    }
}