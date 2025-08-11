using UnityEngine;

public enum ItemType
{
    Amulet,
    Helmet,
    Armor,
    Weapon,
    Shield,
    Leg,
    Boot,
    Ring
}

[System.Serializable]
public class Item
{
    public string itemName; // Nome do item
    public Sprite icon; // Ícone do item
    public ItemType itemType; // Tipo do item
    public int arm; // Defesa do item
    public int price; // Preço do item
    public int attack; // Ataque do item
}
