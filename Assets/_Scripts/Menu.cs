using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject configObj;
    [SerializeField] private GameObject infosObj;

    private void Start()
    {
        SoundManager.Instance.SongControl("earlyGame");
        configObj.SetActive(false);
        infosObj.SetActive(false);
    }

    public void LoadSetupScreen()
    {
        SceneManager.LoadScene("TelaSetup", LoadSceneMode.Single);
    }
    
    public void LoadBluetoothDebug()
    {
        SoundManager.Instance.StopSong();
        SceneManager.LoadScene("BluetoothManagerExample", LoadSceneMode.Single);
    }

    public void OpenLink(string link)
    {
        Application.OpenURL(link);
    }
}
