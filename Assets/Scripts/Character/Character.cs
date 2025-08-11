using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;

public class Character : MonoBehaviour
{

    
    [Header("Character Properties")]
    public string characterName;    
    public int characterLevel;
    public int characterHealth;
    public int characterMaxHealth;
    public int characterMana;
    public int characterMaxMana;
    public int characterExperience;
    public int characterSpeed;
    public float characterPosX; // Posição X do personagem
    public float characterPosY; // Posição Y do personagem
    public int characterGold; // Ouro do personagem

    
    [Header("Character Appearance")]
    public GameObject characterModel; // Referência ao modelo do personagem

    [Header("Character Abilities")]
    public List<string> abilities; // Lista de habilidades do personagem    


    [Header("Character Equipment")]
    public GameObject amulet; // Referência ao amuleto equipado
    public GameObject helmet; // Referência ao capacete equipado
    public GameObject backpack; // Referência à backpack equipada
    public GameObject weapon; // Referência à arma equipada 
    public GameObject armor; // Referência à armadura equipada
    public GameObject shield; // Referência ao escudo equipado
    public GameObject ring; // Referência ao anel equipado
    public GameObject legs; // Referência às calças equipadas
    public GameObject belt; // Referência ao cinto equipado
    public GameObject boots; // Referência às botas equipadas
    public GameObject gloves; // Referência às luvas equipadas

    [Header("Movement")]
    public float moveSpeed = 3f;
    private Rigidbody2D rb;
    private float moveH, moveV;
    
    [Header("Combat")]
    public float attackRange = 2f; // Distância para atacar
    public float attackCooldown = 2f; // Cooldown entre ataques
    public int attackDamage = 10; // Dano do ataque
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    
    // Componentes
    private CharacterMovement characterMovement;
    private CharacterAnim characterAnim;




    [Header("Firebase Sync")]
    public float savePositionInterval = 2f; // Salva posição a cada 2 segundos
    private float lastSaveTime;
    private float lastSavedPosX;
    private float lastSavedPosY;

    [Header("Leveling System")]
    public int baseExperience = 100; // Experiência base para o próximo nível
    public float levelMultiplier = 1.5f; // Multiplicador para calcular a experiência necessária

    // Regeneração de vida e mana
    private float lastHealthRegenTime = 0f;
    private float lastManaRegenTime = 0f;

    // Constantes para intervalos de regeneração
    private const float HEALTH_REGEN_INTERVAL = 2f; // Intervalo de regeneração de vida
    private const float MANA_REGEN_INTERVAL = 1f; // Intervalo de regeneração de mana

    // Start is called before the first frame update
    protected virtual void Start()
    {
   
        // Busca componentes
        characterMovement = GetComponent<CharacterMovement>();
        characterAnim = GetComponent<CharacterAnim>();
        
        // Configura Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
        // Atualiza as posições iniciais
        characterPosX = transform.position.x;
        characterPosY = transform.position.y;
        
        // Inicializa posições salvas
        lastSavedPosX = characterPosX;
        lastSavedPosY = characterPosY;
        lastSaveTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        CheckPositionUpdate();
        
        // Atualiza posição para Firebase
        characterPosX = transform.position.x;
        characterPosY = transform.position.y;

        // Verifica se o jogador deve subir de nível
        CheckLevelUp();

        // Regenera vida e mana
        RegenerateStats();
    }
    
    // Propriedades públicas para outros componentes
    public bool IsAttacking => isAttacking;
    


    
    private void CheckPositionUpdate()
    {
        // Salva posição automaticamente a cada intervalo
        if (Time.time - lastSaveTime >= savePositionInterval)
        {
            SavePositionToFirebase();
        }
    }
    
    private void SavePositionToFirebase()
    {
        // Verifica se o usuário está logado
        if (FirebaseAuth.DefaultInstance?.CurrentUser == null)
        {
            return;
        }

        // Verifica se o nome do personagem está definido
        if (string.IsNullOrEmpty(characterName))
        {
            return;
        }

        // Verifica se a posição realmente mudou
        float tolerance = 0.1f; // Tolerância para movimento livre
        if (Mathf.Abs(characterPosX - lastSavedPosX) < tolerance && Mathf.Abs(characterPosY - lastSavedPosY) < tolerance)
        {
            return; // Não precisa salvar se a posição não mudou significativamente
        }

        Dictionary<string, object> positionData = new Dictionary<string, object>
        {
            {"posX", characterPosX},
            {"posY", characterPosY}
        };

        if (FirebaseAuth.DefaultInstance?.CurrentUser != null)
        {
            string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            string characterPath = $"users/{userId}/characters/{characterName}";

            DatabaseReference databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
            databaseRef.Child(characterPath).UpdateChildrenAsync(positionData);
        }

        lastSavedPosX = characterPosX;
        lastSavedPosY = characterPosY;
        lastSaveTime = Time.time;
    }
    
    // Função para receber dano
    public void TakeDamage(int damage)
    {
        // Reduz a saúde do jogador
        characterHealth -= damage;

        // Garante que a saúde não fique abaixo de zero
        if (characterHealth < 0)
        {
            characterHealth = 0;
        }


        // Atualiza a saúde no Firebase
        UpdateHealthInFirebase();

        // Verifica se o jogador morreu
        if (characterHealth == 0)
        {
            Die();
        }
    }

    // Função chamada quando o jogador morre
    private void Die()
    {
        // Aqui você pode adicionar lógica para respawn ou fim de jogo
    }

    private void CheckLevelUp()
    {
        int experienceNeeded = Mathf.FloorToInt(baseExperience * Mathf.Pow(characterLevel, levelMultiplier));

        while (characterExperience >= experienceNeeded)
        {
            characterExperience -= experienceNeeded;
            characterLevel++;


            // Atualiza os atributos baseados no nível
            characterMaxHealth = 175 + (5 * characterLevel);
            characterMaxMana = 175 + (25 * characterLevel);
            characterHealth = characterMaxHealth; // Restaura a saúde máxima ao subir de nível
            characterMana = characterMaxMana; // Restaura a mana máxima ao subir de nível
     

            // Recalcula a experiência necessária para o próximo nível
            experienceNeeded = Mathf.FloorToInt(baseExperience * Mathf.Pow(characterLevel, levelMultiplier));

            // Atualiza o nível e atributos no Firebase
            if (FirebaseAuth.DefaultInstance?.CurrentUser != null)
            {
                string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                string characterPath = $"users/{userId}/characters/{characterName}";

                DatabaseReference databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    {"level", characterLevel},
                    {"maxHealth", characterMaxHealth},
                    {"maxMana", characterMaxMana}
                };

                databaseRef.Child(characterPath).UpdateChildrenAsync(updates);
            }
          
        }
    }

    private void RegenerateStats()
    {
        RegenerateStat(ref characterHealth, characterMaxHealth, HEALTH_REGEN_INTERVAL, "currentHealth", ref lastHealthRegenTime);
        RegenerateStat(ref characterMana, characterMaxMana, MANA_REGEN_INTERVAL, "currentMana", ref lastManaRegenTime);
    }

    private void RegenerateStat(ref int currentStat, int maxStat, float interval, string firebaseProperty, ref float lastRegenTime)
    {
        if (Time.time - lastRegenTime >= interval)
        {
            if (currentStat < maxStat)
            {
                currentStat = Mathf.Min(currentStat + 2, maxStat);
                UpdateFirebaseProperty(firebaseProperty, currentStat);
            }
            lastRegenTime = Time.time;
        }
    }

    public void UpdateFirebaseProperty(string propertyName, object value)
    {
        // Verifica se o usuário está autenticado
        if (FirebaseAuth.DefaultInstance?.CurrentUser == null)
        {
            return;
        }

        // Verifica se o nome do personagem está definido
        if (string.IsNullOrEmpty(characterName))
        {
            return;
        }

        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        string characterPath = $"users/{userId}/characters/{characterName}";

        DatabaseReference databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        databaseRef.Child(characterPath).Child(propertyName).SetValueAsync(value);
    }

    private void UpdateHealthInFirebase()
    {
        UpdateFirebaseProperty("currentHealth", characterHealth);
    }

    private void UpdateManaInFirebase()
    {
        UpdateFirebaseProperty("currentMana", characterMana);
    }
}
