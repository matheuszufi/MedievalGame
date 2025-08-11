using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    
    // Componentes
    private Rigidbody2D rb;
    private CharacterAnim characterAnim;
    
    // Variáveis de movimento
    private float moveH, moveV;
    private bool isMoving = false;
    private int lastDirection = 0; // 0=N, 1=NE, 2=E, 3=SE, 4=S, 5=SW, 6=W, 7=NW
    
    // Referência para verificar se está atacando
    private Character character;
    private CharacterCombat characterCombat;
    
    void Start()
    {
        // Configura Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
        // Busca componentes
        characterAnim = GetComponent<CharacterAnim>();
        character = GetComponent<Character>();
        characterCombat = GetComponent<CharacterCombat>();
    }
    
    void Update()
    {
        HandleMovement();
    }
    
    void FixedUpdate()
    {
        // Captura input para movimento em 8 direções
        moveH = Input.GetAxis("Horizontal");
        moveV = Input.GetAxis("Vertical");
        
        // Só aplica movimento se não estiver atacando
        if (!IsAttacking())
        {
            // Aplica movimento com proporção isométrica (X:Y = 2:1)
            Vector2 movement = new Vector2(moveH, moveV * 0.5f).normalized * moveSpeed;
            rb.velocity = movement;
        }
        else
        {
            // Para o movimento durante o ataque
            rb.velocity = Vector2.zero;
        }
    }
    
    void HandleMovement()
    {
        // Não permite movimento durante ataque
        if (IsAttacking()) return;
        
        // Determina se está se movendo
        isMoving = (moveH != 0 || moveV != 0);
        
        if (isMoving)
        {
            // Calcula o ângulo em graus
            float angle = Mathf.Atan2(moveV, moveH) * Mathf.Rad2Deg;
            
            // Normaliza o ângulo para 0-360
            if (angle < 0) angle += 360;
            
            // Determina a direção baseada no ângulo
            int direction = GetDirectionFromAngle(angle);
            lastDirection = direction;
            
            // Notifica o sistema de animação
            if (characterAnim != null)
            {
                characterAnim.SetDirection(direction, "movement");
            }
        }
        else
        {
            // Parado - usa sprites de idle da última direção
            if (characterAnim != null)
            {
                characterAnim.SetDirection(lastDirection, "idle");
            }
        }
    }
    
    int GetDirectionFromAngle(float angle)
    {
        // Divide 360 graus em 8 seções de 45 graus cada
        angle = (angle + 22.5f) % 360f;
        int index = Mathf.FloorToInt(angle / 45f);
        
        // Mapeia para: 0=N, 1=NE, 2=E, 3=SE, 4=S, 5=SW, 6=W, 7=NW
        int[] mapping = { 2, 1, 0, 7, 6, 5, 4, 3 };
        return mapping[index];
    }
    
    bool IsAttacking()
    {
        // Verifica primeiro no CharacterCombat, depois no Character
        if (characterCombat != null)
        {
            return characterCombat.IsAttacking;
        }
        
        if (character != null)
        {
            return character.IsAttacking;
        }
        
        return false;
    }
    
    // Propriedades públicas para outros scripts
    public bool IsMoving => isMoving;
    public int LastDirection => lastDirection;
    public float MoveH => moveH;
    public float MoveV => moveV;
    
    // Método para parar movimento (usado durante ataques)
    public void StopMovement()
    {
        rb.velocity = Vector2.zero;
    }
}
