using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Characters/Template", order = 1)]
public class Character : ScriptableObject
{
    public string playerName;
    public Sprite characterPortrait;
    public Sprite characterImage;
    public Sprite characterBanner;
    public int health;
    public int damage;
    public int defense;
}