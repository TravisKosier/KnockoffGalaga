using UnityEngine;

using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public string sceneToLoad;

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
