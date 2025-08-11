using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemySystem
{
    public class EnemyAnim : MonoBehaviour
    {
        [Header("Sprite Animation")]
        public SpriteRenderer spriteRenderer;
        
        // Sprites de movimento (4 direções isométricas)
        [Header("Movement Sprites")]
        public List<Sprite> moveNorthEastSprites;
        public List<Sprite> moveNorthWestSprites;
        public List<Sprite> moveSouthEastSprites;
        public List<Sprite> moveSouthWestSprites;
        
        // Sprites de idle (4 direções isométricas)
        [Header("Idle Sprites")]
        public List<Sprite> idleNorthEastSprites;
        public List<Sprite> idleNorthWestSprites;
        public List<Sprite> idleSouthEastSprites;
        public List<Sprite> idleSouthWestSprites;


        [Header("Animation Settings")]
        public int frameRate = 18;
        
        // Variáveis para controle de animação
        private List<Sprite> currentSpriteList;
        private int currentFrame = 0;
        private float frameTimer = 0f;
        private bool isAttacking = false;
        private bool attackAnimationComplete = false;
        
        // Referências para outros componentes
        private EnemyMovement enemyMovement;
        private Enemy enemy;
        
        void Start()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            
            enemyMovement = GetComponent<EnemyMovement>();
            enemy = GetComponent<Enemy>();
            
            
            // Configura sprite inicial (idle south east)
            currentSpriteList = idleSouthEastSprites;
            if (currentSpriteList != null && currentSpriteList.Count > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = currentSpriteList[0];
            }
        }
        
        void Update()
        {
            UpdateSpriteAnimation();
        }
        
        public void SetDirection(Vector2 direction, string animationType)
        {
            // Determina qual direção isométrica baseada no Vector2
            int directionIndex = GetIsometricDirectionIndex(direction);

            // Seleciona a lista de sprites correta baseada na direção e tipo de animação
            switch (animationType)
            {
                case "movement":
                    switch (directionIndex)
                    {
                        case 0: currentSpriteList = moveNorthEastSprites; break;  // North East
                        case 1: currentSpriteList = moveNorthWestSprites; break;  // North West
                        case 2: currentSpriteList = moveSouthEastSprites; break;  // South East
                        case 3: currentSpriteList = moveSouthWestSprites; break;  // South West
                        default: currentSpriteList = moveSouthEastSprites; break;
                    }
                    break;

                case "idle":
                    switch (directionIndex)
                    {
                        case 0: currentSpriteList = idleNorthEastSprites; break;  // Idle North East
                        case 1: currentSpriteList = idleNorthWestSprites; break;  // Idle North West
                        case 2: currentSpriteList = idleSouthEastSprites; break;  // Idle South East
                        case 3: currentSpriteList = idleSouthWestSprites; break;  // Idle South West
                        default: currentSpriteList = idleSouthEastSprites; break;
                    }
                    // Quando para de se mover, reinicia a animação idle
                    currentFrame = 0;
                    frameTimer = 0f;
                    if (currentSpriteList != null && currentSpriteList.Count > 0)
                    {
                        spriteRenderer.sprite = currentSpriteList[0];
                    }
                    break;
            }
            
            // Reinicia a animação
            currentFrame = 0;
            frameTimer = 0f;
        }
        
        private int GetIsometricDirectionIndex(Vector2 direction)
        {
            // Mapeia Vector2 para índice de direção isométrica
            // North East: (1, 1) = 0
            // North West: (-1, 1) = 1
            // South East: (1, -1) = 2
            // South West: (-1, -1) = 3
            
            if (direction.x > 0 && direction.y > 0) return 0; // North East
            if (direction.x < 0 && direction.y > 0) return 1; // North West
            if (direction.x > 0 && direction.y < 0) return 2; // South East
            if (direction.x < 0 && direction.y < 0) return 3; // South West
            
            // Default para South East se não encontrar direção específica
            return 2;
        }
        
        void UpdateSpriteAnimation()
        {
            if (currentSpriteList == null || currentSpriteList.Count == 0) return;
            
            // Atualiza timer da animação
            frameTimer += Time.deltaTime;
            
            // Verifica se é hora de trocar de frame
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer = 0f;
                
                if (isAttacking)
                {
                    // Animação de ataque (executa uma vez)
                    currentFrame++;
                    if (currentFrame < currentSpriteList.Count)
                    {
                        spriteRenderer.sprite = currentSpriteList[currentFrame];
                        
                        // Verifica se a animação de ataque terminou (último frame)
                        if (currentFrame == currentSpriteList.Count - 1)
                        {
                            attackAnimationComplete = true;
                            CompleteAttack();
                        }
                    }
                }
                else
                {
                    // Animação normal (loop infinito)
                    currentFrame = (currentFrame + 1) % currentSpriteList.Count;
                    spriteRenderer.sprite = currentSpriteList[currentFrame];
                }
            }
        }
        
        void CompleteAttack()
        {
            isAttacking = false;
            attackAnimationComplete = false;
            
            // Volta para idle após o ataque
            if (enemyMovement != null)
            {
                Vector2 lastDirection = GetLastMovementDirection();
                SetDirection(lastDirection, "idle");
            }
        }
        
        private Vector2 GetLastMovementDirection()
        {
            // Pega a última direção de movimento do EnemyMovement
            if (enemyMovement != null)
            {
                return enemyMovement.GetLastMovementDirection();
            }
            
            return new Vector2(1, -1); // South East padrão
        }
        
        // Propriedades públicas
        public bool IsAttacking => isAttacking;
        public bool AttackAnimationComplete => attackAnimationComplete;
    }
}
