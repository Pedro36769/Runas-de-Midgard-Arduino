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
    public Character characterData;
    [SerializeField] private PlayerPopUp popUp;

    [Header("Vida")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_InputField inputHp;
    public int currentHp;

    [Header("Dano")]
    [SerializeField] private TMP_Text dmgText;
    [SerializeField] private TMP_InputField inputDmg;
    public int currentDmg;

    [Header("Defesa")]
    [SerializeField] private TMP_Text defText;
    [SerializeField] private TMP_InputField inputDef;
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

        //inicializa os valores
        currentHp = characterData.health;
        currentDmg = characterData.damage;
        currentDef = characterData.defense;

        UpdateValuesUI();

        if(popUp!=null) popUp.SetupPopUp(characterData);
    }

    private void UpdateValuesUI()
    {
        if(hpText!=null) hpText.text = currentHp.ToString();
        if(inputHp!=null) inputHp.text = currentHp.ToString();
   
        if(dmgText!=null) dmgText.text = currentDmg.ToString();
        if(inputDmg!=null) inputDmg.text = currentDmg.ToString();
        
        if(defText!=null) defText.text = currentDef.ToString();
        if(inputDef!=null) inputDef.text = currentDef.ToString();
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

    public void ChangeDamage(string inputtedDmg)
    {
        string cleanInput = inputtedDmg.Trim((char)8203, ' ');

        if (int.TryParse(cleanInput, out int newDmg) && newDmg >= 0 && newDmg <= 25)
        {
            currentDmg = newDmg;
            inputDmg.text = newDmg.ToString();
        }
        else
        {
            inputDmg.text = currentDmg.ToString(); 
            Debug.LogWarning("Input inválido ou fora do limite (0-25)! Valor resetado para: " + currentDmg);
        }
    }

    public void ChangeDefense(string inputtedDef)
    {
        string cleanInput = inputtedDef.Trim((char)8203, ' ');

        if (int.TryParse(cleanInput, out int newDef) && newDef >= 0 && newDef <= 25)
        {
            currentDef = newDef;
            inputDef.text = newDef.ToString();
        }
        else
        {
            inputDef.text = currentDef.ToString(); 
            Debug.LogWarning("Input inválido ou fora do limite (0-25)! Valor resetado para: " + currentDef);
        }
    }

    public void TakeDamage(int incomingDamage)
    {
        int previousHp = currentHp;
        int previousDef = currentDef;

        if (currentDef > 0) // se tiver defesa
        {
            if (incomingDamage >= currentDef) 
            {
                // se o dano ultrapassar a defesa, desconta no hp
                int excessDamage = incomingDamage - currentDef;
                currentDef = 0;
                currentHp -= excessDamage;
            }
            else 
            {
                // a defesa é suficiente para absorver todo o dano
                currentDef -= incomingDamage;
            }
            Debug.Log("Defesa anterior: " + previousDef + ". Defesa atual: " + currentDef);
        }
        else // senão, reduz o HP diretamente
        {
            currentHp -= incomingDamage;
        }

        // garante q o hp n fique negativo
        currentHp = Mathf.Max(0, currentHp);
        
        Debug.Log("Vida anterior: " + previousHp + ". Vida atual: " + currentHp);
        Debug.Log("Player " + cardName + " sofreu um ataque de " + incomingDamage + " de dano.");

        UpdateValuesUI();
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