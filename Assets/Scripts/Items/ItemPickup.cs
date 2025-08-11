using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private Item item; // Referência ao item associado

    public void SetItem(Item newItem)
    {
        item = newItem;
        // Aqui você pode configurar a aparência do item no mundo, como o ícone
    }

    public Item GetItem()
    {
        return item;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Lógica para quando o jogador coleta o item
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Item coletado: {item.itemName}");
            Destroy(gameObject); // Destroi o item após ser coletado
        }
    }
}
