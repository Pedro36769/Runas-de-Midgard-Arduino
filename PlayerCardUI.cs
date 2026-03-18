using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCardUI : MonoBehaviour
{
    public Image portraitImage;
    [SerializeField] private Image bannerImage;
    [SerializeField] private TMP_Text nameText;
    public string cardName;
    [SerializeField] private bool setupOnStart;
    [SerializeField] private Character characterData;

    [Header("Valores")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_InputField inputHp;
    public int currentHp;
    [SerializeField] private TMP_Text dmgText;
    public int currentDmg;
    [SerializeField] private TMP_Text defText;
    public int currentDef;

    private void Start()
    {
        if(setupOnStart && characterData!=null) SetupCard(characterData);
    }

    public void SetupCard(Character characterData)
    {
        if (characterData == null) return;

        portraitImage.sprite = characterData.characterPortrait;
        if(bannerImage!=null) bannerImage.sprite = characterData.characterBanner;
        cardName = characterData.playerName;
        nameText.text = cardName;

        currentHp = characterData.health;
        if(hpText!=null) hpText.text = currentHp.ToString();
        if(inputHp!=null) inputHp.text = currentHp.ToString();
        currentDmg = characterData.damage;
        if(dmgText!=null) dmgText.text = currentDmg.ToString();
        currentDef = characterData.defense;
        if(defText!=null) defText.text = currentDef.ToString();
    }

    public void ChangeHealth(string inputtedHp)
    {
        string cleanInput = inputtedHp.Trim((char)8203, ' '); //tira um caractere de controle do TMP
        //teoricamente nem precisa disso pq o TMP força o usuário a só colocar int, mas eh bom checar

        if (int.TryParse(cleanInput, out int newHp) && newHp >= 0 && newHp <= 99)
        {
            currentHp = newHp;
            inputHp.text = newHp.ToString();
        }
        else
        {
            inputHp.text = currentHp.ToString(); 
            Debug.LogWarning("Input inválido ou fora do limite (0-99)! Valor resetado para: " + currentHp);
        }
    }

    public void TakeDamage(int takenDmg)
    {
        int previousHp = currentHp;
        currentHp = currentHp - takenDmg;
        Debug.Log("Player " + cardName + " tomou " + takenDmg + " de dano.");
        Debug.Log("Vida anterior: " + previousHp + ". Vida atual: " + currentHp);
    }
    public void DestroyCard()
    {
        Destroy(this.gameObject);
    }
}