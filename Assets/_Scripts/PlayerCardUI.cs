using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCardUI : MonoBehaviour
{
    public Image portraitImage;
    public Image portraitFgImg;
    public Image portraitBgImg;
    public Image bannerImage;
    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private GameObject lockObj;
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
        if(lockObj!=null) lockObj.SetActive(false);
    }

    public void SetupCard(Character characterData)
    {
        if (characterData == null) return;

        //imagens
        portraitImage.sprite = characterData.characterPortrait;
        portraitFgImg.sprite = characterData.portraitFg;
        if(portraitBgImg!=null) portraitBgImg.sprite = characterData.portraitBg;
        if(bannerImage!=null) bannerImage.sprite = characterData.characterBanner;
        if(cardImage!=null) cardImage.sprite = characterData.characterCard;

        //textos
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
    public void LockCard()
    {
        lockObj.SetActive(true); 
        if(cardImage!=null) cardImage.sprite = lockedSprite;
        portraitFgImg.color = new Vector4(0.4f, 0.4f, 0.4f, 1);
        if(portraitBgImg!=null) portraitBgImg.color = new Vector4(0.4f, 0.4f, 0.4f, 1);
        portraitImage.color = new Vector4(0.5f, 0.5f, 0.5f, 1);
        nameText.color = new Vector4(0.5f, 0.5f, 0.5f, 1);
        defText.color = new Vector4(0.5f, 0.5f, 0.5f, 1);
        dmgText.color = new Vector4(0.5f, 0.5f, 0.5f, 1);
        hpText.color = new Vector4(0.5f, 0.5f, 0.5f, 1);
    }

    public void UnlockCard()
    {
        lockObj.SetActive(false); 
        if(cardImage!=null) cardImage.sprite = characterData.characterCard;
        portraitFgImg.color = new Vector4(1, 1, 1, 1);
        if(portraitBgImg!=null) portraitBgImg.color = new Vector4(1, 1, 1, 1);
        portraitImage.color = new Vector4(1, 1, 1, 1);
        nameText.color = new Vector4(1, 1, 1, 1);
        defText.color = new Vector4(1, 1, 1, 1);
        dmgText.color = new Vector4(1, 1, 1, 1);
        hpText.color = new Vector4(1, 1, 1, 1);
    }
}