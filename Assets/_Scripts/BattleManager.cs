using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    private bool twoPlayers;
    private int playersInCombat;
    [SerializeField] private GameObject battleCanvas;
    [SerializeField] private Image attackingPlayerImg;
    [SerializeField] private TMP_Text attackingPlayerText;

    [Header("Player cards")]
    [SerializeField] private PlayerCardUI player1Card;
    private int player1Hp;
    [SerializeField] private PlayerCardUI player2Card;
    private int player2Hp;
    [SerializeField] private PlayerCardUI player3Card;
    private int player3Hp;
    [SerializeField] private PlayerCardUI player4Card;
    private int player4Hp;

    private List<PlayerCardUI> alivePlayers = new List<PlayerCardUI>();
    private Dictionary<PlayerCardUI, PlayerCardUI> attackTargets = new Dictionary<PlayerCardUI, PlayerCardUI>();
    
    private bool isWaitingForTargetSelection = false;
    private PlayerCardUI currentAttacker;

    public static BattleManager Instance { get; private set; }
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
        battleCanvas.SetActive(false);

        twoPlayers = SetupManager.Instance.twoPlayerMode;
        playersInCombat = twoPlayers ? 2 : 4;

        player1Card = GameManager.Instance.player1Card;
        player2Card = GameManager.Instance.player2Card;
        player3Card = GameManager.Instance.player3Card;
        player4Card = GameManager.Instance.player4Card;

        player1Hp = player1Card.currentHp;
        player2Hp = player2Card.currentHp; 
        
        if(!twoPlayers) 
        {
            player3Hp = player3Card.currentHp; 
            player4Hp = player4Card.currentHp; 
        }
        
        battleCanvas.SetActive(false);
    }

    public void BattleLogic()
    {
        battleCanvas.SetActive(true);
        StartCoroutine(BattleRoutine());
    }

    private IEnumerator BattleRoutine()
    {
        // checa quantos players ainda estão vivos
        alivePlayers.Clear();
        if (player1Hp > 0) alivePlayers.Add(player1Card);
        if (player2Hp > 0) alivePlayers.Add(player2Card);
        if (!twoPlayers)
        {
            if (player3Hp > 0) alivePlayers.Add(player3Card);
            if (player4Hp > 0) alivePlayers.Add(player4Card);
        }

        // decide quem vai atacar primeiro
        for (int i = 0; i < alivePlayers.Count; i++)
        {
            PlayerCardUI temp = alivePlayers[i];
            int randomIndex = Random.Range(i, alivePlayers.Count);
            alivePlayers[i] = alivePlayers[randomIndex];
            alivePlayers[randomIndex] = temp;
        }

        attackTargets.Clear();

        // passa por cada jogador vivo para que ele escolha seu alvo
        foreach (PlayerCardUI attacker in alivePlayers)
        {
            currentAttacker = attacker;

            // atualiza a UI baseada em quem está atacando
            attackingPlayerImg.sprite = currentAttacker.portraitImage.sprite;
            attackingPlayerText.text = "Mostre a tela para o jogador: " + currentAttacker.cardName;

            isWaitingForTargetSelection = true;

            // pausa a execução deste loop ATÉ que a variável isWaitingForTargetSelection vire false
            yield return new WaitWhile(() => isWaitingForTargetSelection);
        }

        ResolveDamage();
    }

    public void SetTarget(PlayerCardUI targetCard)
    {
        if (!isWaitingForTargetSelection) return;

        attackTargets.Add(currentAttacker, targetCard);
        isWaitingForTargetSelection = false;
    }

    private void ResolveDamage()
    {
        foreach (KeyValuePair<PlayerCardUI, PlayerCardUI> combatPair in attackTargets)
        {
            PlayerCardUI attacker = combatPair.Key;
            PlayerCardUI target = combatPair.Value;

            int attackPower = attacker.currentDmg;
            int defensePower = target.currentDef;

            // Mathf.Max impede que o dano seja negativo se a defesa for maior que o ataque
            int finalDamage = Mathf.Max(0, attackPower - defensePower);

            // aplica o dano na vida do alvo correspondente
            if (target == player1Card) player1Hp -= finalDamage;
            else if (target == player2Card) player2Hp -= finalDamage;
            else if (target == player3Card) player3Hp -= finalDamage;
            else if (target == player4Card) player4Hp -= finalDamage;
            
            Debug.Log($"{attacker.cardName} atacou {target.cardName}! (Ataque: {attackPower} - Defesa: {defensePower} = {finalDamage} de dano causado)");
        }

        attackTargets.Clear();
        battleCanvas.SetActive(false);
        Debug.Log("Combate acabou.");
    }
}