using UnityEngine;
public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Unique
}
public enum ItemCategory
{
    AdventuringGear,
    AlchemicalItems,
    Armour,
    Consumables,
    Shields,
    Staves,
    TradeGoods,
    Wands,
    Weapons,
    WornItems
}

[CreateAssetMenu(menuName = "ScriptableObjects/PF2eItem")]
public class Item : ScriptableObject
{
    public string Name;
    public Rarity Rarity;
    public ItemCategory Category;
    public int Level;
    public int Price;
    /// <summary>
    /// -1 is Negligible, 0 is light, positive is normal bulk.
    /// </summary>
    public int Bulk;
}