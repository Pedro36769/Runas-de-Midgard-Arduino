using UnityEngine;
using System.Collections;
using TMPro;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private SoundManager soundManager;

    // textos
    [Header("Textos")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text roundCounter;
    [SerializeField] private TMP_Text getRuneText;
    [SerializeField] private TMP_Text runeText;
    [SerializeField] private TMP_Text arenaText;
    [SerializeField] private TMP_Text eventText;
    [SerializeField] private TMP_Text currentEventText;
    [SerializeField] private TMP_Text eventDurationText;
    [SerializeField] private TMP_Text riskyModeText;

    [Header("Animators")]
    [SerializeField] private Animator riskyBtnAnimator;
    [SerializeField] private Animator runeBtnAnimator;

    //cores
        //runas
    private Color goldenRuneColor = new Vector4(0.86f, 0.73f, 0.23f, 1);
    private Color curseRuneColor = new Vector4(0.37f, 0.05f, 0.58f, 1);
    private Color goodRuneColor = new Vector4(0.09f, 0.63f, 0.2f, 1);
    private Color badRuneColor = new Vector4(0.65f, 0.09f, 0.09f, 1);
        //eventos
    private Color blizzardColor = new Vector4(0.29f, 0.81f, 0.93f, 1);
    private Color tidesOfMidgardColor = new Vector4(0.26f, 0.21f, 0.6f, 1);
    private Color fenrirsHuntColor = new Vector4(0.59f, 0.19f, 0.23f, 1);
        //personagens
    private Color bjornColor = new Vector4(0.03f, 0.09f, 0.11f, 1);

    private int currentRound = 1;
    private Coroutine holdCoroutine;
    private bool riskyMode = false;
    private bool eventActive = false;
    private int eventDuration;
    private int lastEventEndRound = -4;
    private int eventCooldown = 1;
    private bool inBattle = false;

    private void Awake()
    {
        ClearUITexts();
    }

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; //mantém tela ligada
        roundCounter.text = currentRound.ToString();
    }

    private void Update()
    {
        runeBtnAnimator.SetBool("IsRisky", riskyMode);
    }

    public void NextRound()
    {
        currentRound++;
        roundCounter.text = currentRound.ToString();

        if (eventActive)
        {
            eventDuration--;
            if (eventDuration <= 0)
            {
                eventActive = false;
                lastEventEndRound = currentRound; //o evento acabou nessa rodada
                ClearUITexts();
            }
            else
            {
                UpdateEventDuration();
            }
        }

        if (currentRound % 3 == 0 && currentRound != 0) //se divisível por 3, começa batalha e escolhe evento
        {
            arenaText.text = "É hora da batalha!";
            soundManager.PlaySFX("BattleStart");
            roundCounter.color = Color.red;
            soundManager.SongControl("battle");
            inBattle = true;

            if (!eventActive && (currentRound - lastEventEndRound) > eventCooldown) //se não tem nenhum evento ativo && o último evento aconteceu a mais de X rodadas
            {
                ChooseGlobalEvent();
            }
        }
        else
        {
            ClearUITexts();
            roundCounter.color = Color.white;
            inBattle = false;
        }

        // sound control
        if(currentRound < 3 && !inBattle) soundManager.SongControl("earlyGame");
        else if(currentRound < 9 && !inBattle) soundManager.SongControl("midGame");
        else if(currentRound < 12 && !inBattle) soundManager.SongControl("lateGame");
        else if(currentRound > 12)
        {
            if(!inBattle)
            {
                soundManager.SongControl("ragnarok");
            }
        }
    }

    private void ChooseGlobalEvent()
    {
        string chosenEvent = "";
        int eventN = Random.Range(0, 3); 
        
        eventDuration = Random.Range(1, 4); //duração de no máximo 3 rodadas 
        eventActive = true; 

        if(eventN == 0) 
        { 
            chosenEvent = "Nevasca"; currentEventText.color = blizzardColor; 
            soundManager.PlaySFX("Blizzard");
        }
        else if(eventN == 1) 
        { 
            chosenEvent = "Maré de Midgard"; currentEventText.color = tidesOfMidgardColor; 
            soundManager.PlaySFX("TidesSound");
        }
        else if(eventN == 2) 
        { 
            chosenEvent = "Caçada de Fenrir"; currentEventText.color = fenrirsHuntColor; 
            soundManager.PlaySFX("HuntSound");
        }

        eventText.text = "Evento: ";
        currentEventText.text = chosenEvent;

        UpdateEventDuration();
    }

    private void UpdateEventDuration()
    {
        string plural = eventDuration > 1 ? "s" : ""; //se for maior que 1, adiciona o 's'
        eventDurationText.text = $"O evento vai durar por: {eventDuration} rodada{plural}.";
    }

    public void GetRune()
    {
        ClearUITexts();
        string chosenRune = "";
        int rune;
        rune = Random.Range(0, 10);

        if(!riskyMode)
        {
            Debug.Log("Escolhendo runa com chances normais");
            if(rune == 0) //10%
            {
                chosenRune = "Antiga";
                soundManager.PlaySFX("RuneAncient");
                runeText.color = goldenRuneColor;
            }
            else if(rune == 1) //10%
            {
                chosenRune = "Maldição";
                soundManager.PlaySFX("RuneCurse");
                runeText.color = curseRuneColor;
            }
            else if(rune < 6) //40%
            {
                chosenRune = "Benéfica";
                soundManager.PlaySFX("RuneGood");
                runeText.color = goodRuneColor;
            }
            else if(rune > 5) //40%
            {
                chosenRune = "Maléfica";
                soundManager.PlaySFX("RuneBad");
                runeText.color = badRuneColor;
            }
        }
        else //risky mode
        {
            Debug.Log("Escolhendo runa com chances arriscadas");
            if(rune == 0) //10%
            {
                chosenRune = "Benéfica";
                soundManager.PlaySFX("RuneGood");
                runeText.color = goodRuneColor;
            }
            else if(rune == 1) //10%
            {
                chosenRune = "Maléfica";
                soundManager.PlaySFX("RuneBad");
                runeText.color = badRuneColor;
            }
            else if(rune < 6) //40%
            {
                chosenRune = "Antiga";
                soundManager.PlaySFX("RuneAncient");
                runeText.color = goldenRuneColor;
            }
            else if(rune > 5) //40%
            {
                chosenRune = "Maldição";
                soundManager.PlaySFX("RuneCurse");
                runeText.color = curseRuneColor;
            }

            riskyMode = false;
        }
        
        Debug.Log("Ganhou a runa: " + chosenRune);
        getRuneText.text = "Runa recebida: ";
        runeText.text = chosenRune;
    }

    public void OnButtonRiskyPress()
    {
        if (holdCoroutine != null) //garante que não existam duas contagens ao mesmo tempo
        {
            StopCoroutine(holdCoroutine);
        }
        holdCoroutine = StartCoroutine(TrackHoldTime());
    }

    public void OnButtonRiskyRelease()
    {
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
        
        if (!riskyMode)
        {
            riskyModeText.text = ""; // player soltou antes, cancelou
        }
        riskyBtnAnimator.SetBool("Pressing", false);
    }

    private IEnumerator TrackHoldTime()
    {
        ClearUITexts();
        float t = 0;
        riskyMode = false;

        while (t < 2)
        {
            t += Time.deltaTime;

            riskyBtnAnimator.SetBool("Pressing", true);
            if (t < 0.66f) riskyModeText.text = "Segure para apostar com os deuses.";
            else if (t < 1.2f) riskyModeText.text = "Segure para apostar com os deuses..";
            else if (t < 2f) riskyModeText.text = "Segure para apostar com os deuses...";

            yield return null;
        }

        riskyMode = true;
        riskyBtnAnimator.SetBool("Pressing", false);
        riskyModeText.text = "Os deuses aceitam sua oferta. Suas chances mudaram.";
    }

    private void ClearUITexts()
    {
        arenaText.text = "";
        getRuneText.text = "";
        runeText.text = "";
        if(!eventActive) //só limpa texto do evento se não tiver um evento ativo
        {
            eventDurationText.text = "";
            currentEventText.text = "";
            eventText.text = "";
        }
        riskyModeText.text = "";
    }
}
