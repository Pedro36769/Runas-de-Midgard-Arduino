using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject eventPopUp;
    [SerializeField] private GameObject openEventBtn;
    [SerializeField] private Image eventBtnImg;
    [SerializeField] private Image eventPopupIcon;

    [Header ("Imagens")]
    [SerializeField] private Sprite blizzardSprite;
    [SerializeField] private Sprite tidesOfMidgardSprite;
    [SerializeField] private Sprite fenrirsHuntSprite;

    [Header("Textos")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text roundCounter;
    [SerializeField] private TMP_Text arenaText;
    [SerializeField] private TMP_Text currentEventText;
    [SerializeField] private TMP_Text eventDurationText;

    [Header("Player cards")]
    public PlayerCardUI player1Card;
    public PlayerCardUI player2Card;
    public PlayerCardUI player3Card;
    public PlayerCardUI player4Card;

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
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        ClearUITexts();
    }

    private void Start()
    {
        if(SetupManager.Instance==null)
        {
            Debug.LogError("Não achei o SetupManager, começa a partir da cena Setup");
        }
        player1Card.SetupCard(SetupManager.Instance.player1Char);
        player2Card.SetupCard(SetupManager.Instance.player2Char);
        if(SetupManager.Instance.twoPlayerMode) //se tiver só dois players, destrói as outras cartas
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
                openEventBtn.SetActive(false);
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
            SoundManager.Instance.PlaySFX("BattleStart");
            roundCounter.color = Color.red;
            SoundManager.Instance.SongControl("battle");
            inBattle = true;
            BattleManager.Instance.BattleLogic();
        }
        else
        {
            ClearUITexts();
            roundCounter.color = Color.white;
            inBattle = false;

            //só escolhe evento se não for rodada múltipla de 3, e se tiver passado da rodada 2
            if (!eventActive && currentRound > 2 && (currentRound - lastEventEndRound) > eventCooldown) //se não tem nenhum evento ativo && o último evento aconteceu a mais de X rodadas
            {
                ChooseGlobalEvent();
            }
        }

        if(ragnarokActive)
        {
            Debug.Log("Ragnarok está ativo.");
        }

        // sound control
        if(currentRound < 3 && !inBattle) SoundManager.Instance.SongControl("earlyGame");
        else if(currentRound < 9 && !inBattle) SoundManager.Instance.SongControl("midGame");
        else if(currentRound < 12 && !inBattle) SoundManager.Instance.SongControl("lateGame");
        else if(currentRound > 12)
        {
            ragnarokActive = true;
            if(!inBattle)
            {
                SoundManager.Instance.SongControl("ragnarok");
            }
        }
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
            eventBtnImg.sprite = blizzardSprite;
            eventPopupIcon.sprite = blizzardSprite;
            SoundManager.Instance.PlaySFX("Blizzard");
        }
        else if(eventN == 1) 
        { 
            chosenEvent = "Maré de Jörmungandr"; 
            currentEventText.color = tidesOfMidgardColor; 
            eventBtnImg.sprite = tidesOfMidgardSprite;
            eventPopupIcon.sprite = tidesOfMidgardSprite;
            SoundManager.Instance.PlaySFX("TidesSound");
        }
        else if(eventN == 2) 
        { 
            chosenEvent = "Caçada de Fenrir"; 
            currentEventText.color = fenrirsHuntColor; 
            eventBtnImg.sprite = fenrirsHuntSprite;
            eventPopupIcon.sprite = fenrirsHuntSprite;
            SoundManager.Instance.PlaySFX("HuntSound");
        }

        ShowEventPopUp(true);
        currentEventText.text = chosenEvent;
        UpdateEventDuration();
    }

    private void UpdateEventDuration()
    {
        string plural = eventDuration > 1 ? "s" : ""; //se for maior que 1, adiciona o 's'
        eventDurationText.text = $"O evento vai durar por: {eventDuration} rodada{plural}.";
    }

    public void ShowEventPopUp(bool show)
    {
        eventPopUp.SetActive(show);
        openEventBtn.SetActive(!show);
    }

    private void ClearUITexts()
    {
        arenaText.text = "";
        if(!eventActive) //só limpa texto do evento se não tiver um evento ativo
        {
            eventDurationText.text = "";
            currentEventText.text = "";
        }
    }
}
