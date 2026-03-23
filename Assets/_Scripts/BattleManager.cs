using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    private bool twoPlayers;
    private int playersInCombat;
    [SerializeField] private GameObject confirmAttackObj;

    [Header("UI de Combate")]
    [SerializeField] private GameObject battleCanvas;
    [SerializeField] private Image attackingPlayerImg;
    [SerializeField] private TMP_Text attackingPlayerText;
    [SerializeField] private ParticleSystem battleParticle;
    [SerializeField] private GameObject mainElements;

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

    private List<PlayerCardUI> alivePlayers = new List<PlayerCardUI>();
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
        mainElements.SetActive(true);
        battleCanvas.SetActive(false);
        confirmAttackObj.SetActive(false);
        
        battleParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

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
        
        foreach (var btn in targetButtons) btn.gameObject.SetActive(false);
    }

    public void BattleLogic()
    {
        mainElements.SetActive(false);
        battleCanvas.SetActive(true);
        battleParticle.Play();
        SoundManager.Instance.PlaySFX("ArenaStart");
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
        foreach (KeyValuePair<PlayerCardUI, PlayerCardUI> combatPair in attackTargets)
        {
            PlayerCardUI attacker = combatPair.Key;
            PlayerCardUI target = combatPair.Value;

            int finalDamage = Mathf.Max(0, attacker.currentDmg - target.currentDef);

            target.TakeDamage(finalDamage);
            target.ChangeHealth(target.currentHp.ToString());
        }

        battleCanvas.SetActive(false);
        battleParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        mainElements.SetActive(true);
        Debug.Log("Combate resolvido e UI atualizada.");
    }
}