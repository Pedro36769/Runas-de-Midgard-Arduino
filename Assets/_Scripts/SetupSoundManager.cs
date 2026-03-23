using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SetupSoundManager : MonoBehaviour
{
    private FMOD.Studio.EventInstance setupSongInstance;

    public static SetupSoundManager Instance { get; private set; }

    private void Awake()
    {
        //singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        setupSongInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Songs/SetupSong");
        setupSongInstance.start();

        setupSongInstance.setParameterByName("mobiusPlucks", 0);
        setupSongInstance.setParameterByName("drone", 0);
    }

    public void PlaySFX(string sfxPath)
    {
        RuntimeManager.PlayOneShot("event:/SFXs/" + sfxPath);
    }

    public void StopSong()
    {
        setupSongInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        setupSongInstance.release();
    }

    public void SetupSongControl(int step)
    { 
        //selecionando personagens
        if(step == 1) setupSongInstance.setParameterByName("mobiusPlucks", 1);
        //personagens selecionados
        if(step == 2) setupSongInstance.setParameterByName("drone", 1);
    }
}
