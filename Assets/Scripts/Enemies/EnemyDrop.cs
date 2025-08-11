using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    // Este script não é mais necessário - a lógica de drop foi movida para Enemy.cs
    // Mantido apenas para compatibilidade, caso seja referenciado em algum lugar
    
    [Header("Gold Drop Settings")]
    public int minGoldDrop = 10; // Mínimo de ouro que pode ser dropado
    public int maxGoldDrop = 50; // Máximo de ouro que pode ser dropado

    public void DropGold(Character character)
    {
        // Método deprecado - usar Enemy.DropGold() diretamente
        Debug.LogWarning("EnemyDrop.DropGold() está deprecado. Use Enemy.DropGold() diretamente.");
    }
}
