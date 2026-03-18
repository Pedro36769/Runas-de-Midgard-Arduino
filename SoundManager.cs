using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundManager : MonoBehaviour
{
    private FMOD.Studio.EventInstance songInstance;

    public static SoundManager Instance { get; private set; }

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
        songInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Songs/GameSong");
        songInstance.start();
    }

    public void PlaySFX(string sfxPath)
    {
        RuntimeManager.PlayOneShot("event:/SFXs/" + sfxPath);
    }

    public void SongControl(string mode)
    {
        if(mode == "earlyGame") // round 1-2
        {
            //guitar
            songInstance.setParameterByName("driveGuitar", 0);
            songInstance.setParameterByName("mandolin", 0);
            //bass
            songInstance.setParameterByName("bassGroove", 0);
            songInstance.setParameterByName("bassStraight", 0);
            //pads
            songInstance.setParameterByName("grandfatherPads", 0);
            songInstance.setParameterByName("icePads", 1);
            //drums
            songInstance.setParameterByName("kick", 0);
            songInstance.setParameterByName("snare", 0);
            songInstance.setParameterByName("tomLow", 0);
        }
        else if(mode == "midGame") // round 4-5 && 7-8
        {
            //guitar
            songInstance.setParameterByName("driveGuitar", 0);
            songInstance.setParameterByName("mandolin", 1);
            //bass
            songInstance.setParameterByName("bassGroove", 1);
            songInstance.setParameterByName("bassStraight", 0);
            //pads
            songInstance.setParameterByName("grandfatherPads", 0);
            songInstance.setParameterByName("icePads", 1);
            //drums
            songInstance.setParameterByName("kick", 0);
            songInstance.setParameterByName("snare", 0);
            songInstance.setParameterByName("tomLow", 0);
        }
        else if(mode == "lateGame") // round 10-11
        {
            //guitar
            songInstance.setParameterByName("driveGuitar", 0);
            songInstance.setParameterByName("mandolin", 1);
            //bass
            songInstance.setParameterByName("bassGroove", 1);
            songInstance.setParameterByName("bassStraight", 0);
            //pads
            songInstance.setParameterByName("grandfatherPads", 0);
            songInstance.setParameterByName("icePads", 1);
            //drums
            songInstance.setParameterByName("kick", 0);
            songInstance.setParameterByName("snare", 0);
            songInstance.setParameterByName("tomLow", 1);
        }
        else if(mode == "ragnarok")
        {
            //guitar
            songInstance.setParameterByName("driveGuitar", 0);
            songInstance.setParameterByName("mandolin", 0);
            //bass
            songInstance.setParameterByName("bassGroove", 1);
            songInstance.setParameterByName("bassStraight", 0);
            //pads
            songInstance.setParameterByName("grandfatherPads", 1);
            songInstance.setParameterByName("icePads", 0);
            //drums
            songInstance.setParameterByName("kick", 0);
            songInstance.setParameterByName("snare", 0);
            songInstance.setParameterByName("tomLow", 1);
        }
        else if(mode == "battle")
        {
            //guitar
            songInstance.setParameterByName("driveGuitar", 1);
            //bass
            songInstance.setParameterByName("bassGroove", 0);
            songInstance.setParameterByName("bassStraight", 1);
            //pads
            songInstance.setParameterByName("grandfatherPads", 1);
            songInstance.setParameterByName("icePads", 0);
            //drums
            songInstance.setParameterByName("kick", 1);
            songInstance.setParameterByName("snare", 1);
            songInstance.setParameterByName("tomLow", 1);
        }
    }
}
