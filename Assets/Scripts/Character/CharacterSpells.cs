
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellType
{
    Heal,
    Attack,
    Support
}

[System.Serializable]
public class Spell
{
    public string spellName;
    public SpellType spellType;
    public int minValue;
    public int maxValue;
    public int manaCost;
}

public class CharacterSpells : MonoBehaviour
{
    public List<Spell> spells = new List<Spell>(); // Lista de feitiços

    private Character character; // Referência ao script Character

    private void Start()
    {
        character = GetComponent<Character>();

        // Adiciona o feitiço de cura no F1
        spells.Add(new Spell
        {
            spellName = "Heal",
            spellType = SpellType.Heal,
            minValue = 10,
            maxValue = 40,
            manaCost = 20
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            UseSpell(0); // Usa o feitiço no índice 0 (F1)
        }
    }

    private void UseSpell(int spellIndex)
    {
        if (spellIndex < 0 || spellIndex >= spells.Count) return;

        Spell spell = spells[spellIndex];

        if (spell.spellType == SpellType.Heal && character.characterMana >= spell.manaCost)
        {
            int healAmount = Random.Range(spell.minValue, spell.maxValue + 1);
            character.characterHealth = Mathf.Min(character.characterHealth + healAmount, character.characterMaxHealth);
            character.characterMana -= spell.manaCost;

            Debug.Log($"Usou {spell.spellName}: Curou {healAmount} de vida. Mana restante: {character.characterMana}");

            // Atualiza no Firebase
            character.UpdateFirebaseProperty("currentHealth", character.characterHealth);
            character.UpdateFirebaseProperty("currentMana", character.characterMana);
        }
        else
        {
            Debug.Log("Mana insuficiente ou feitiço inválido.");
        }
    }
}
