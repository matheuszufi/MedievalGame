using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayUIManager : MonoBehaviour
{
    [Header("Character Info")]
    public TMP_Text characterNameText;

    [Header("Health Bar")]
    public Image healthBarFill;
    public TMP_Text healthText; // Opcional: para mostrar números

    [Header("Mana Bar")]
    public Image manaBarFill; // Opcional: para mana também
    public TMP_Text manaText;

    [Header("Experience Bar")]
    public Image experienceBarFill; // Opcional: para experiência
    public TMP_Text experienceText; // Opcional: para mostrar experiência atual

    [Header("Experience Info")]
    public TMP_Text experienceNeededText; // Texto para mostrar a experiência necessária para o próximo nível

    [Header("Character Level")]
    public TMP_Text characterLevelText; // Texto para mostrar o nível do personagem

    [Header("Character Gold")]
    public TMP_Text characterGold; // Texto para mostrar o ouro do personagem

    [Header("Character")]
    private Character currentCharacter;

    public static PlayUIManager Instance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        // Procura pelo personagem na cena
        FindCharacter();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple instances of PlayUIManager detected. There should only be one instance.");
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Se não há personagem, tenta encontrar
        if (currentCharacter == null)
        {
            FindCharacter();
        }
        else
        {
            // Atualiza as barras de vida, mana e experiência
            UpdateCharacterInfo();
            UpdateHealthBar();
            UpdateManaBar();
            UpdateExperienceBar();
            UpdateCharacterGold(currentCharacter.characterGold);
        }
    }

    private void FindCharacter()
    {
        currentCharacter = FindObjectOfType<Character>();

        if (currentCharacter != null)
        {
            Debug.Log($"PlayUIManager encontrou personagem: {currentCharacter.characterName}");
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null && currentCharacter != null)
        {
            // Calcula o fill da barra de vida (0 a 1)
            float healthPercentage = (float)currentCharacter.characterHealth / (float)currentCharacter.characterMaxHealth;
            healthBarFill.fillAmount = Mathf.Clamp01(healthPercentage);

            // Atualiza o texto se configurado
            if (healthText != null)
            {
                healthText.text = $"{currentCharacter.characterHealth}/{currentCharacter.characterMaxHealth}";
            }
        }
    }

    private void UpdateManaBar()
    {
        if (manaBarFill != null && currentCharacter != null)
        {
            // Calcula o fill da barra de mana (0 a 1)
            float manaPercentage = (float)currentCharacter.characterMana / (float)currentCharacter.characterMaxMana;
            manaBarFill.fillAmount = Mathf.Clamp01(manaPercentage);

            // Atualiza o texto se configurado
            if (manaText != null)
            {
                manaText.text = $"{currentCharacter.characterMana}/{currentCharacter.characterMaxMana}";
            }
        }
    }

    private void UpdateCharacterInfo()
    {
        if (characterNameText != null && currentCharacter != null)
        {
            characterNameText.text = currentCharacter.characterName;
        }

        // Atualiza o texto do nível do personagem
        if (characterLevelText != null && currentCharacter != null)
        {
            characterLevelText.text = $"{currentCharacter.characterLevel}";
        }
    }

    private void UpdateExperienceBar()
    {
        if (experienceBarFill != null && currentCharacter != null)
        {
            // Calcula o preenchimento da barra de experiência (0 a 1)
            int experienceNeeded = Mathf.FloorToInt(currentCharacter.baseExperience * Mathf.Pow(currentCharacter.characterLevel, currentCharacter.levelMultiplier));
            float experiencePercentage = (float)currentCharacter.characterExperience / experienceNeeded;
            experienceBarFill.fillAmount = Mathf.Clamp01(experiencePercentage);

            // Atualiza o texto da experiência atual, se configurado
            if (experienceText != null)
            {
                experienceText.text = $"{currentCharacter.characterExperience}/{experienceNeeded}";
            }
        }
    }

    // Método público para definir manualmente o personagem
    public void SetCharacter(Character character)
    {
        currentCharacter = character;

        if (currentCharacter != null)
        {
            Debug.Log($"PlayUIManager agora monitora: {currentCharacter.characterName}");
        }
    }

    public void UpdateCharacterGold(int goldAmount)
    {
    
        if (characterGold != null && currentCharacter != null)
        {
      
            currentCharacter.characterGold = goldAmount;
            characterGold.text = $"{currentCharacter.characterGold}";
        }
        else
        {
            Debug.LogWarning("characterGold TMP_Text or currentCharacter is null.");
        }
    }   





}
