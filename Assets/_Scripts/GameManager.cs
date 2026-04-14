using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;

    [Header("Game End")]
    [SerializeField] private GameObject gameEndCanvas;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private Image winnerImg;
    [SerializeField] private Image loserImg1;
    [SerializeField] private Image loserImg2; //imagem central pro modo de dois jogadores
    [SerializeField] private Image loserImg3;

    [Header("Eventos")]
    [SerializeField] private GameObject eventPopUp;
    [SerializeField] private GameObject openEventBtn;
    [SerializeField] private Image eventBtnImg;
    [SerializeField] private Image eventPopupIcon;

    //sprites
    [SerializeField] private Sprite blizzardSprite;
    [SerializeField] private Sprite fenrirsHuntSprite;
    [SerializeField] private Sprite tidesOfMidgardSprite;

    //effects
    [SerializeField] private ParticleSystem snowParticle;
    [SerializeField] private GameObject wavesObj;
    [SerializeField] private GameObject fenrirObj;

    //texts
    [SerializeField] private TMP_Text currentEventText;
    [SerializeField] private TMP_Text eventDurationText;
    [SerializeField] private TMP_Text eventRoundCounter;
    [SerializeField] private TMP_Text eventDescriptionText;

    //colors
    private Color blizzardColor = new Vector4(0.29f, 0.81f, 0.93f, 1);
    private Color tidesOfMidgardColor = new Vector4(0.26f, 0.21f, 0.6f, 1);
    private Color fenrirsHuntColor = new Vector4(0.59f, 0.19f, 0.23f, 1);

    [Header("Rodadas")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text roundCounter;
    [SerializeField] private TMP_Text arenaText;

    [Header("Player cards")]
    public PlayerCardUI player1Card;
    public PlayerCardUI player2Card;
    public PlayerCardUI player3Card;
    public PlayerCardUI player4Card;

    private int currentRound = 1;
    private bool twoPlayers = false;
    private bool gameEnded = false;
    private bool eventActive = false;
    private int eventDuration;
    private int lastEventEndRound = -4;
    private int lastEventIndex = -1;
    private int eventCount = 0; //conta eventos consecutivos
    private int eventCooldown = 1;
    private bool inBattle = false;
    private bool ragnarokActive = false;

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        //singleton
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;

        ClearUITexts();
    }

    private void Start()
    {
        player1Card.SetupCard(SetupManager.Instance.player1Char);
        player2Card.SetupCard(SetupManager.Instance.player2Char);

        twoPlayers = SetupManager.Instance.twoPlayerMode;
        if(twoPlayers) //se tiver só dois players, destrói as outras cartas
        {
            player3Card.DestroyCard();
            player4Card.DestroyCard();
        }
        else
        {
            player3Card.SetupCard(SetupManager.Instance.player3Char);
            player4Card.SetupCard(SetupManager.Instance.player4Char);
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep; //mantém tela ligada
        roundCounter.text = currentRound.ToString();

        eventPopUp.SetActive(false);
        openEventBtn.SetActive(false);
        pauseCanvas.SetActive(false);

        snowParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        wavesObj.SetActive(false);
        fenrirObj.SetActive(false);
    }

    public void NextRound()
    {
        currentRound++;
        roundCounter.text = currentRound.ToString();

        if (currentRound % 3 == 0 && currentRound != 0) //se divisível por 3, começa arena
        {
            arenaText.text = "É hora da batalha!";
            roundCounter.color = Color.red;
            SoundManager.Instance.SongControl("battle");
            inBattle = true;
            BattleManager.Instance.BattleLogic();
        }
        else //se não for arena
        {
            ClearUITexts();
            roundCounter.color = Color.white;
            inBattle = false;

            if (eventActive)
            {
                eventDuration--;
                if (eventDuration <= 0)
                {
                    eventActive = false;
                    openEventBtn.SetActive(false);

                    lastEventEndRound = currentRound; //evento acabou nessa rodada
                    ClearUITexts();
                }
                else
                {
                    UpdateEventDuration();
                }
            }

            //só tenta criar novo evento se não for arena e não houver um ativo
            if (!eventActive && currentRound > 2 && (currentRound - lastEventEndRound) > eventCooldown)
            {
                if (!gameEnded) ChooseGlobalEvent();
            }
        }

        if(ragnarokActive)
        {
            Debug.Log("Ragnarok está ativo.");
        }

        // sound control
        if(currentRound < 3 && !inBattle) SoundManager.Instance.SongControl("earlyGame");
        if(currentRound < 9 && !inBattle) SoundManager.Instance.SongControl("midGame");
        if(currentRound < 12 && !inBattle) SoundManager.Instance.SongControl("lateGame");
        if(currentRound > 12)
        {
            ragnarokActive = true;
            if(!inBattle)
            {
                SoundManager.Instance.SongControl("ragnarok");
            }
        }

        if (BattleManager.Instance.GetAlivePlayersCount() == 1)
        {
            SoundManager.Instance.SongControl("endGame");
            gameEnded = true;
            SetupEndGameScreen(); 
        }
    }

    private void SetupEndGameScreen()
    {
        PlayerCardUI winner = null;
        List<PlayerCardUI> losers = new List<PlayerCardUI>();

        //separa o ganhador dos perdedores
        CheckPlayerResult(player1Card, ref winner, losers);
        CheckPlayerResult(player2Card, ref winner, losers);
        if (!twoPlayers) 
        {
            CheckPlayerResult(player3Card, ref winner, losers);
            CheckPlayerResult(player4Card, ref winner, losers);
        }

        if (winner != null)
        {
            winnerImg.sprite = winner.portraitImage.sprite;
            winnerText.text = winner.cardName + " perseverou";
        }

        if (twoPlayers)
        {
            //modo 2 Players, desativa 1 e 3
            loserImg1.gameObject.SetActive(false);
            loserImg3.gameObject.SetActive(false);
            
            loserImg2.gameObject.SetActive(true);
            if (losers.Count > 0)
            {
                loserImg2.sprite = losers[0].portraitImage.sprite;
            }
        }
        else
        {
            //modo 4 Players, ativa todos
            loserImg1.gameObject.SetActive(true);
            loserImg2.gameObject.SetActive(true);
            loserImg3.gameObject.SetActive(true);

            if (losers.Count > 0) loserImg1.sprite = losers[0].portraitImage.sprite;
            if (losers.Count > 1) loserImg2.sprite = losers[1].portraitImage.sprite;
            if (losers.Count > 2) loserImg3.sprite = losers[2].portraitImage.sprite;
        }

        // Ativa o canvas no final
        gameEndCanvas.SetActive(true);
    }

    private void CheckPlayerResult(PlayerCardUI player, ref PlayerCardUI winner, List<PlayerCardUI> losers)
    {
        if (player.currentHp > 0) winner = player;
        else losers.Add(player);
    }

    private void ChooseGlobalEvent()
    {
        int eventN;

        do //roda até achar um evento permitido
        {
            eventN = Random.Range(0, 3);
        } 
        while (eventN == lastEventIndex && eventCount >= 2);

        //conta repetições do evento
        if (eventN == lastEventIndex)
        {
            eventCount++;
        }
        else
        {
            eventCount = 1; 
            lastEventIndex = eventN; 
        }

        string chosenEvent = "";
        eventDuration = Random.Range(1, 4); 
        eventActive = true; 

        if(eventN == 0) 
        { 
            chosenEvent = "Nevasca de Jotunheim"; 
            currentEventText.color = blizzardColor;
            eventRoundCounter.color = blizzardColor;
            eventDescriptionText.text = "• Deslocamento reduzido pela metade. <br>• <color=#FFD900><b>-2 de Ataque</b></color> global (<color=#CCC>mínimo 1</color>).";
            
            eventBtnImg.sprite = blizzardSprite;
            eventPopupIcon.sprite = blizzardSprite;

            SoundManager.Instance.PlaySFX("Blizzard");
            snowParticle.Play();

            ControlesCenaDois.Instance.Enviar("nevasca#");
        }
        else if(eventN == 1) 
        { 
            chosenEvent = "Maré de Jörmungandr"; 
            currentEventText.color = tidesOfMidgardColor; 
            eventRoundCounter.color = tidesOfMidgardColor;
            eventDescriptionText.text = "• Todas as <color=#FF3432><b>curas</b></color> recebem +5.<br>• <color=#00C8FF><b>Defesa</b></color> de todos reduzida em -3.";

            eventBtnImg.sprite = tidesOfMidgardSprite;
            eventPopupIcon.sprite = tidesOfMidgardSprite;

            SoundManager.Instance.PlaySFX("TidesSound");
            wavesObj.SetActive(true);

            ControlesCenaDois.Instance.Enviar("mare#");
        }
        else if(eventN == 2) 
        { 
            chosenEvent = "Caçada de Fenrir"; 
            currentEventText.color = fenrirsHuntColor; 
            eventRoundCounter.color = fenrirsHuntColor;
            eventDescriptionText.text = "•  O jogador com maior <color=#FF3432><b>Vida</b></color> recebe <color=#FFD900><b>+2 de Ataque</b></color>.<br>• Esse mesmo jogador sofre <color=#00C8FF><b>-4 de Defesa</b></color>.";

            eventBtnImg.sprite = fenrirsHuntSprite;
            eventPopupIcon.sprite = fenrirsHuntSprite;

            SoundManager.Instance.PlaySFX("HuntSound");
            fenrirObj.SetActive(true);

            ControlesCenaDois.Instance.Enviar("hunt#");
        }

        ShowEventPopUp(true);
        currentEventText.text = chosenEvent;
        UpdateEventDuration();
    }

    private void UpdateEventDuration()
    {
        string plural = eventDuration > 1 ? "s" : ""; //se for maior que 1, adiciona o 's'
        eventDurationText.text = $"O evento vai durar por: {eventDuration} rodada{plural}.";
        eventRoundCounter.text = $"( {eventDuration} )";
    }

    public void ShowEventPopUp(bool show)
    {
        eventPopUp.SetActive(show);
        openEventBtn.SetActive(!show);
        eventRoundCounter.text = !show ? $"( {eventDuration} )" : "";
    }

    private void ClearUITexts()
    {
        arenaText.text = "";
        if(!eventActive) //só limpa texto do evento se não tiver um evento ativo
        {
            eventDurationText.text = "";
            eventRoundCounter.text = "";
            currentEventText.text = "";

            snowParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            wavesObj.SetActive(false);
            fenrirObj.SetActive(false);
        }
    }

    public void LoadMenu()
    {
        SetupManager.Instance.LoadMenuScreen();
    }
}
