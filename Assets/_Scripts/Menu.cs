using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadSetupScreen()
    {
        SceneManager.LoadScene("TelaSetup", LoadSceneMode.Single);
    }
}
