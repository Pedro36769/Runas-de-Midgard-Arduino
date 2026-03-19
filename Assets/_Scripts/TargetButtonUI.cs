using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetButtonUI : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private Image banner;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TargetButtonUI selectedTarget;
    private PlayerCardUI targetReference;

    public void Setup(PlayerCardUI target)
    {
        targetReference = target;
        portrait.sprite = target.portraitImage.sprite;
        banner.sprite = target.bannerImage.sprite;
        nameText.text = target.cardName;
        hpText.text = target.currentHp.ToString();
        gameObject.SetActive(true);
    }

    public void OnClickTarget()
    {
        // fala pro BattleManager quem foi o escolhido
        BattleManager.Instance.SetTarget(targetReference);
    }

    public void ShowSelectedTarget()
    {
        selectedTarget.Setup(targetReference);
    }
}