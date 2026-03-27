using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Characters/Template", order = 1)]
public class Character : ScriptableObject
{
    public string playerName;
    public string abilityTitle;
    public string abilityDesc;
    public string abilityDesc2;
    public Sprite characterPortrait;
    public Sprite portraitFg;
    public Sprite portraitBg;
    public Sprite characterImage;
    public Sprite characterBanner;
    public Sprite characterCard;
    public int health;
    public int damage;
    public int defense;
}