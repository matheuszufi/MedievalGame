using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnim : MonoBehaviour
{
    [Header("Sprite Animation")]
    public SpriteRenderer spriteRenderer;
    
    // Sprites de movimento
    [Header("Movement Sprites")]
    public List<Sprite> nSprites;
    public List<Sprite> neSprites;
    public List<Sprite> eSprites;
    public List<Sprite> seSprites;      
    public List<Sprite> sSprites;
    public List<Sprite> swSprites;
    public List<Sprite> wSprites;
    public List<Sprite> nwSprites;
    
    // Sprites de idle
    [Header("Idle Sprites")]
    public List<Sprite> idleNSprites;
    public List<Sprite> idleNeSprites;
    public List<Sprite> idleESprites;
    public List<Sprite> idleSeSprites;
    public List<Sprite> idleSSprites;
    public List<Sprite> idleSwSprites;
    public List<Sprite> idleWSprites;
    public List<Sprite> idleNwSprites;
    
    // Sprites de ataque
    [Header("Attack Sprites")]
    public List<Sprite> attackNSprites;
    public List<Sprite> attackNeSprites;
    public List<Sprite> attackESprites;
    public List<Sprite> attackSeSprites;
    public List<Sprite> attackSSprites;
    public List<Sprite> attackSwSprites;
    public List<Sprite> attackWSprites;
    public List<Sprite> attackNwSprites;

    [Header("Animation Settings")]
    public int frameRate = 18;
    
    // Variáveis para controle de animação
    private List<Sprite> currentSpriteList;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    private bool isAttacking = false;
    private bool attackAnimationComplete = false;
    
    // Referências para outros componentes
    private CharacterMovement characterMovement;
    private Character character;
    
    void Start()
    {
        // Busca o SpriteRenderer se não foi configurado
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Busca componentes
        characterMovement = GetComponent<CharacterMovement>();
        character = GetComponent<Character>();
        
        // Configura sprite inicial (idle norte)
        currentSpriteList = idleNSprites;
        if (currentSpriteList != null && currentSpriteList.Count > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = currentSpriteList[0];
        }
    }
    
    void Update()
    {
        UpdateSpriteAnimation();
    }
    
    public void SetDirection(int direction, string animationType)
    {
        // Seleciona a lista de sprites correta baseada na direção e tipo de animação
        switch (animationType)
        {
            case "movement":
                switch (direction)
                {
                    case 0: currentSpriteList = nSprites; break;   // Norte
                    case 1: currentSpriteList = neSprites; break;  // Nordeste
                    case 2: currentSpriteList = eSprites; break;   // Leste
                    case 3: currentSpriteList = seSprites; break;  // Sudeste
                    case 4: currentSpriteList = sSprites; break;   // Sul
                    case 5: currentSpriteList = swSprites; break;  // Sudoeste
                    case 6: currentSpriteList = wSprites; break;   // Oeste
                    case 7: currentSpriteList = nwSprites; break;  // Noroeste
                    default: currentSpriteList = nSprites; break;
                }
                break;
                
            case "idle":
                switch (direction)
                {
                    case 0: currentSpriteList = idleNSprites; break;   // Idle Norte
                    case 1: currentSpriteList = idleNeSprites; break;  // Idle Nordeste
                    case 2: currentSpriteList = idleESprites; break;   // Idle Leste
                    case 3: currentSpriteList = idleSeSprites; break;  // Idle Sudeste
                    case 4: currentSpriteList = idleSSprites; break;   // Idle Sul
                    case 5: currentSpriteList = idleSwSprites; break;  // Idle Sudoeste
                    case 6: currentSpriteList = idleWSprites; break;   // Idle Oeste
                    case 7: currentSpriteList = idleNwSprites; break;  // Idle Noroeste
                    default: currentSpriteList = idleNSprites; break;
                }
                // Quando para de se mover, reinicia a animação idle
                currentFrame = 0;
                frameTimer = 0f;
                if (currentSpriteList != null && currentSpriteList.Count > 0)
                {
                    spriteRenderer.sprite = currentSpriteList[0];
                }
                break;
                
            case "attack":
                switch (direction)
                {
                    case 0: currentSpriteList = attackNSprites; break;   // Ataque Norte
                    case 1: currentSpriteList = attackNeSprites; break;  // Ataque Nordeste
                    case 2: currentSpriteList = attackESprites; break;   // Ataque Leste
                    case 3: currentSpriteList = attackSeSprites; break;  // Ataque Sudeste
                    case 4: currentSpriteList = attackSSprites; break;   // Ataque Sul
                    case 5: currentSpriteList = attackSwSprites; break;  // Ataque Sudoeste
                    case 6: currentSpriteList = attackWSprites; break;   // Ataque Oeste
                    case 7: currentSpriteList = attackNwSprites; break;  // Ataque Noroeste
                    default: currentSpriteList = attackNSprites; break;
                }
                // Inicia a animação de ataque do primeiro frame
                isAttacking = true;
                attackAnimationComplete = false;
                currentFrame = 0;
                frameTimer = 0f;
                if (currentSpriteList != null && currentSpriteList.Count > 0)
                {
                    spriteRenderer.sprite = currentSpriteList[0];
                }
                break;
        }
    }
    
    void UpdateSpriteAnimation()
    {
        if (currentSpriteList != null && currentSpriteList.Count > 0)
        {
            frameTimer += Time.deltaTime;
            
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer = 0f;
                
                if (isAttacking)
                {
                    // Durante ataque, avança frame por frame sem loop
                    currentFrame++;
                    
                    if (currentFrame >= currentSpriteList.Count)
                    {
                        // Animação de ataque terminou
                        CompleteAttack();
                        return;
                    }
                    
                    spriteRenderer.sprite = currentSpriteList[currentFrame];
                }
                else
                {
                    // Para movimento e idle, faz loop normal
                    currentFrame = (currentFrame + 1) % currentSpriteList.Count;
                    spriteRenderer.sprite = currentSpriteList[currentFrame];
                }
            }
        }
    }
    
    void CompleteAttack()
    {
        isAttacking = false;
        attackAnimationComplete = false;
        
        // Volta para idle ou movimento dependendo do input atual
        if (characterMovement != null)
        {
            if (characterMovement.IsMoving)
            {
                SetDirection(characterMovement.LastDirection, "movement");
            }
            else
            {
                SetDirection(characterMovement.LastDirection, "idle");
            }
        }
    }
    
    public void PlayAttackAnimation(Vector3 direction)
    {
        // Calcula a direção baseada nas 8 direções possíveis
        int attackDirection = GetDirectionFromVector(direction);
        
        Debug.Log($"Iniciando ataque na direção: {attackDirection} ({GetDirectionName(attackDirection)})");
        
        // Inicia a animação de ataque
        SetDirection(attackDirection, "attack");
    }
    
    private int GetDirectionFromVector(Vector3 direction)
    {
        // Calcula o ângulo em graus
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Normaliza o ângulo para 0-360
        if (angle < 0) angle += 360;
        
        // Divide em 8 seções de 45 graus cada
        angle = (angle + 22.5f) % 360f;
        int index = Mathf.FloorToInt(angle / 45f);
        
        // Mapeia para: 0=E, 1=NE, 2=N, 3=NW, 4=W, 5=SW, 6=S, 7=SE
        int[] mapping = { 2, 1, 0, 7, 6, 5, 4, 3 };
        return mapping[index];
    }
    
    private string GetDirectionName(int direction)
    {
        string[] names = { "Norte", "Nordeste", "Leste", "Sudeste", "Sul", "Sudoeste", "Oeste", "Noroeste" };
        return names[direction];
    }
    
    public float GetAttackAnimationDuration()
    {
        if (currentSpriteList != null && currentSpriteList.Count > 0)
        {
            return currentSpriteList.Count / (float)frameRate;
        }
        return 0.5f; // Duração padrão se não houver sprites
    }
    
    // Propriedades públicas
    public bool IsAttacking => isAttacking;
    public bool AttackAnimationComplete => attackAnimationComplete;
}
