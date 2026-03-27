using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerPopUp : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image portraitFgImg;
    [SerializeField] private Image portraitBgImg;

    [Header("Texts")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text abilityTitleText;
    [SerializeField] private TMP_Text abilityDescText;
    [SerializeField] private TMP_Text abilityDesc2Text;

    private void Awake()
    {
        this.gameObject.SetActive(false);
    }

    public void SetupPopUp(Character charData)
    {
        portraitImage.sprite = charData.characterPortrait;
        portraitFgImg.sprite = charData.portraitFg;
        portraitBgImg.sprite = charData.portraitBg;

        nameText.text = charData.playerName;
        abilityTitleText.text = "Habilidade — " + charData.abilityTitle + ":";
        abilityDescText.text = charData.abilityDesc;
        abilityDesc2Text.text = charData.abilityDesc2;
    }
}
