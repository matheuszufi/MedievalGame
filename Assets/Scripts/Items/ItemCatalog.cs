using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemCatalog", menuName = "Catalogs/ItemCatalog")]
public class ItemCatalog : ScriptableObject
{
    public List<Item> items = new List<Item>(); // Lista de itens disponíveis no catálogo
}
