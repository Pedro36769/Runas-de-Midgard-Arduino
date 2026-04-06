using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private void Start()
    {
        SoundManager.Instance.SongControl("earlyGame");
    }

    public void LoadSetupScreen()
    {
        SceneManager.LoadScene("TelaSetup", LoadSceneMode.Single);
    }
    
    public void LoadBluetoothDebug()
    {
        SceneManager.LoadScene("BluetoothManagerExample", LoadSceneMode.Single);
    }

    public void OpenLink(string link)
    {
        Application.OpenURL(link);
    }
}
