using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SetupManager : MonoBehaviour
{
    [Header("Character Database")]
    [SerializeField] private Character[] allAvailableCharacters;

    [SerializeField] private TMP_Text playerNumberText;
    [SerializeField] private GameObject playerNumberScreen;
    [SerializeField] private GameObject continueToCharacterBtn;

    [SerializeField] private GameObject characterSelectScreen;
    [SerializeField] private TMP_Text chooseCharacterText;

    [SerializeField] private GameObject chosenCharactersScreen;

    [SerializeField] private GameObject continueToGameBtn;

    [Header("Chosen characters")]
    public Character player1Char;
    [SerializeField] private PlayerCardUI player1Card;
    public Character player2Char;
    [SerializeField] private PlayerCardUI player2Card;
    public Character player3Char;
    [SerializeField] private PlayerCardUI player3Card;
    public Character player4Char;
    [SerializeField] private PlayerCardUI player4Card;

    [Header("Character cards")]
    [SerializeField] private GameObject bjornCard;
    [SerializeField] private GameObject eydisCard;
    [SerializeField] private GameObject carteiroCard;
    [SerializeField] private GameObject defaultCard;
    [SerializeField] private GameObject default2Card;
    [SerializeField] private GameObject default3Card;

    public bool twoPlayerMode = false;

    public static SetupManager Instance { get; private set; }
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
    }

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; //mantém tela ligada

        continueToCharacterBtn.SetActive(false);
        continueToGameBtn.SetActive(false);
        
        playerNumberScreen.SetActive(true);
        characterSelectScreen.SetActive(false);
        chosenCharactersScreen.SetActive(false);
        playerNumberText.text = "";
    }

    public void SetTwoPlayer(bool twoPlayers)
    {
        twoPlayerMode = twoPlayers;
        playerNumberText.text = twoPlayerMode ? "2 jogadores" : "4 jogadores";
        continueToCharacterBtn.SetActive(true);
    }

    public void SelectCharacters()
    {
        playerNumberScreen.SetActive(false);
        chosenCharactersScreen.SetActive(false);
        characterSelectScreen.SetActive(true);
    }

    public void ChooseCharacter(string chosenCharName)
    {
        //procura na base de dados pelo nome recebido
        Character selectedData = GetCharacterByName(chosenCharName);
        if (selectedData == null) return; //se n achou faz o L

        if (player1Char == null) 
        {
            player1Char = selectedData;
            chooseCharacterText.text = "Jogador 2<br>Escolha seu personagem:";
            return;
        }
        
        if (player2Char == null)
        {
            player2Char = selectedData;
            if (twoPlayerMode) ShowChosenCharacters();
            else chooseCharacterText.text = "Jogador 3<br>Escolha seu personagem:";
            return;
        }

        if (!twoPlayerMode) 
        {
            if (player3Char == null)
            {
                player3Char = selectedData;
                chooseCharacterText.text = "Jogador 4<br>Escolha seu personagem:";
                return;
            }
            
            if (player4Char == null)
            {
                player4Char = selectedData;
                ShowChosenCharacters();
                return;
            }
        }
    }

    private Character GetCharacterByName(string name)
    {
        //procura o personagem pelo nome
        foreach (Character character in allAvailableCharacters)
        {
            if (character.playerName == name)
            {
                return character;
            }
        }
        Debug.LogError("Personagem não encontrado na base de dados: " + name);
        return null; //faz o L
    }

    public void ResetChosenChars()
    {
        player1Char = null; 
        player2Char = null;
        player3Char = null;
        player4Char = null; 
        bjornCard.SetActive(true);
        eydisCard.SetActive(true);
        carteiroCard.SetActive(true);
        defaultCard.SetActive(true);
        default2Card.SetActive(true);
        default3Card.SetActive(true);
        chooseCharacterText.text = "Jogador 1<br>Escolha seu personagem:";
    }

    public void ResetPlayerNumber()
    {
        twoPlayerMode = false;
        characterSelectScreen.SetActive(false);
        playerNumberScreen.SetActive(true);
        ResetChosenChars();
    }

    private void ShowChosenCharacters()
    {
        characterSelectScreen.SetActive(false);
        chosenCharactersScreen.SetActive(true);

        player1Card.gameObject.SetActive(true);
        player1Card.SetupCard(player1Char);

        player2Card.gameObject.SetActive(true);
        player2Card.SetupCard(player2Char);

        if(twoPlayerMode)
        {
            player3Card.gameObject.SetActive(false);
            player4Card.gameObject.SetActive(false);
        }
        else
        {
            player3Card.gameObject.SetActive(true);
            player3Card.SetupCard(player3Char);
            
            player4Card.gameObject.SetActive(true);
            player4Card.SetupCard(player4Char);
        }
        
        continueToGameBtn.SetActive(true);
    }

    public void LoadPlayerScreen()
    {
        SetupSoundManager.Instance.StopSong();
        SceneManager.LoadScene("TelaDeJogo", LoadSceneMode.Single);
    }
}
