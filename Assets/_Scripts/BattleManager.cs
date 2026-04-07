using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    private bool twoPlayers;
    private int playersInCombat;

    [Header("UI de Combate")]
    [SerializeField] private GameObject battleCanvas;
    [SerializeField] private Image attackingPlayerImg;
    [SerializeField] private TMP_Text attackingPlayerText;
    [SerializeField] private ParticleSystem battleParticle;

    [Header("Resumo da Arena")]
    [SerializeField] private GameObject arenaResumeCanvas;
    [SerializeField] private Image[] attackingPlayerImages;
    [SerializeField] private Image[] defendingPlayerImages;
    [SerializeField] private TMP_Text[] takenDmgText;
    [SerializeField] private GameObject[] summaryRows;

    [Header("Configuração de Alvos")]
    [SerializeField] private TargetButtonUI[] targetButtons;

    [Header("Player cards")]
    [SerializeField] private PlayerCardUI player1Card;
    private int player1Hp;
    [SerializeField] private PlayerCardUI player2Card;
    private int player2Hp;
    [SerializeField] private PlayerCardUI player3Card;
    private int player3Hp;
    [SerializeField] private PlayerCardUI player4Card;
    private int player4Hp;

    public List<PlayerCardUI> alivePlayers = new List<PlayerCardUI>();
    private Dictionary<PlayerCardUI, PlayerCardUI> attackTargets = new Dictionary<PlayerCardUI, PlayerCardUI>();
    
    private bool isWaitingForTargetSelection = false;
    private PlayerCardUI currentAttacker;

    public static BattleManager Instance { get; private set; }
    private void Awake()
    {
        //singleton
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else { Instance = this; DontDestroyOnLoad(this.gameObject); }
    }

    private void Start()
    {
        battleCanvas.SetActive(false);
        arenaResumeCanvas.SetActive(false);
        
        battleParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        twoPlayers = SetupManager.Instance.twoPlayerMode;
        playersInCombat = twoPlayers ? 2 : 4;

        player1Card = GameManager.Instance.player1Card;
        player2Card = GameManager.Instance.player2Card;
        player3Card = GameManager.Instance.player3Card;
        player4Card = GameManager.Instance.player4Card;
        
        foreach (var btn in targetButtons) btn.gameObject.SetActive(false);
    }

    public void BattleLogic()
    {
        battleCanvas.SetActive(true);
        battleParticle.Play();
        SoundManager.Instance.PlaySFX("ArenaStart");
        StartCoroutine(BattleRoutine());
    }

    private IEnumerator BattleRoutine()
    {
        // checa quantos players ainda estão vivos
        alivePlayers.Clear();
        if (player1Card.currentHp > 0) alivePlayers.Add(player1Card);
        if (player2Card.currentHp > 0) alivePlayers.Add(player2Card);
        
        if (!twoPlayers)
        {
            if (player3Card.currentHp > 0) alivePlayers.Add(player3Card);
            if (player4Card.currentHp > 0) alivePlayers.Add(player4Card);
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
            UpdateAttackerUI();
            SetupTargetOptions(attacker);

            isWaitingForTargetSelection = true;
            yield return new WaitWhile(() => isWaitingForTargetSelection);
        }

        ResolveDamage();
    }

    private void UpdateAttackerUI()
    {
        attackingPlayerImg.sprite = currentAttacker.portraitImage.sprite;
        attackingPlayerText.text = "Vez de " + currentAttacker.cardName;
    }

    public void SetTarget(PlayerCardUI targetCard)
    {
        if (!isWaitingForTargetSelection) return;
        Debug.Log(currentAttacker.cardName + " vai atacar: " + targetCard.cardName);

        attackTargets.Add(currentAttacker, targetCard);
        isWaitingForTargetSelection = false;
    }

    private void SetupTargetOptions(PlayerCardUI attacker)
    {
        // desativa todos os botões primeiro 
        foreach (var btn in targetButtons) btn.gameObject.SetActive(false);

        int buttonIndex = 0;
        foreach (PlayerCardUI potentialTarget in alivePlayers)
        {
            // o jogador não pode atacar a si mesmo
            if (potentialTarget == attacker) continue;

            if (buttonIndex < targetButtons.Length)
            {
                targetButtons[buttonIndex].Setup(potentialTarget);
                buttonIndex++;
            }
        }
    }

    private void ResolveDamage()
    {
        //desativa todas as linhas do resumo pra limpar o round anterior
        for (int i = 0; i < attackingPlayerImages.Length; i++)
        {
            summaryRows[i].SetActive(false); //desativa o row inteiro se n tiver a imagem
        }

        int index = 0;

        //resolve o dano e preenche a UI
        foreach (KeyValuePair<PlayerCardUI, PlayerCardUI> combatPair in attackTargets)
        {
            PlayerCardUI attacker = combatPair.Key;
            PlayerCardUI target = combatPair.Value;

            target.TakeDamage(attacker.currentDmg);

            //evita erro OutOfBounds se por algum motivo houver mais ataques que slots de UI
            if (index < attackingPlayerImages.Length) 
            {
                //reativa a UI correspondente a este ataque
                if (index < summaryRows.Length && summaryRows[index] != null)
                    summaryRows[index].SetActive(true);
                else
                {
                    attackingPlayerImages[index].gameObject.SetActive(true);
                    defendingPlayerImages[index].gameObject.SetActive(true);
                }

                //substitui os sprites
                attackingPlayerImages[index].sprite = attacker.portraitImage.sprite;
                defendingPlayerImages[index].sprite = target.portraitImage.sprite;

                //atualiza o texto com o dano tomado
                if (index < takenDmgText.Length && takenDmgText[index] != null)
                {
                    takenDmgText[index].text = $"- {attacker.currentDmg}";
                }
                
                index++;
            }
        }

        battleCanvas.SetActive(false);
        battleParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        arenaResumeCanvas.SetActive(true);
    }

    public int GetAlivePlayersCount()
    {
        int count = 0;
        if (player1Card.currentHp > 0) count++;
        if (player2Card.currentHp > 0) count++;
        
        if (!twoPlayers)
        {
            if (player3Card.currentHp > 0) count++;
            if (player4Card.currentHp > 0) count++;
        }
        
        return count;
    }
}