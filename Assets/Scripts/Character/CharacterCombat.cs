using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;

public class CharacterCombat : MonoBehaviour
{
    public float attackRadius = 2f; // Raio da área de ataque
    public int attackDamage = 10; // Dano do ataque
    public float attackCooldown = 2f; // Cooldown entre ataques

    private float lastAttackTime = 0f; // Tempo do último ataque
    private bool isAttacking = false; // Estado de ataque
    
    // Referências aos componentes
    private CharacterAnim characterAnim;
    private Character character;

    // Start is called before the first frame update
    void Start()
    {
        // Busca componentes necessários
        characterAnim = GetComponent<CharacterAnim>();
        character = GetComponent<Character>();
        
        if (characterAnim == null)
        {
            Debug.LogError("CharacterAnim não encontrado no GameObject!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Não permite novas ações durante ataque
        if (isAttacking) return;

        // Verifica se há um inimigo selecionado
        if (EnemySelector.currentSelection != null)
        {
            float distance = Vector3.Distance(transform.position, EnemySelector.currentSelection.transform.position);

            // Verifica se o inimigo está dentro da área de ataque
            if (distance <= attackRadius && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack(EnemySelector.currentSelection);
                lastAttackTime = Time.time;
            }
        }
    }

    private void Attack(GameObject enemy)
    {
        // Define o estado de ataque
        isAttacking = true;
        
        // Calcula a direção do inimigo em relação ao personagem
        Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
        
        // Inicia a animação de ataque na direção correta
        if (characterAnim != null)
        {
            characterAnim.PlayAttackAnimation(directionToEnemy);
            
            // Calcula a duração da animação
            float animationDuration = characterAnim.GetAttackAnimationDuration();
            
            // Aplica o dano após a animação
            StartCoroutine(ApplyDamageAfterAnimation(enemy, animationDuration));
        }
        else
        {
            // Se não há animação, aplica dano imediatamente
            StartCoroutine(ApplyDamageAfterAnimation(enemy, 0f));
        }
    }
    
    private IEnumerator ApplyDamageAfterAnimation(GameObject enemy, float animationDuration)
    {
        // Espera a duração da animação
        yield return new WaitForSeconds(animationDuration);
        
        // Verifica se o inimigo ainda existe
        if (enemy == null)
        {
            Debug.LogWarning("Inimigo foi destruído durante a animação de ataque.");
            isAttacking = false;
            yield break;
        }

        // Aplica dano ao inimigo
        EnemySystem.Enemy enemyComponent = enemy.GetComponent<EnemySystem.Enemy>();
        if (enemyComponent != null)
        {
            Debug.Log($"Atacando {enemyComponent.enemyName} causando {attackDamage} de dano.");
            enemyComponent.TakeDamage(attackDamage);

            // Verifica se o inimigo foi derrotado
            if (enemyComponent.currentHp <= 0)
            {
                Debug.Log($"{enemyComponent.enemyName} foi derrotado!");

                // Adiciona experiência ao jogador
                if (character != null)
                {
                    character.characterExperience += enemyComponent.experience;
                    Debug.Log($"Jogador recebeu {enemyComponent.experience} de experiência!");

                    // Atualiza experiência no Firebase
                    UpdateExperienceInFirebase(character, enemyComponent.experience);
                }
            }
        }

        // Finaliza o estado de ataque
        isAttacking = false;
    }
    
    private void UpdateExperienceInFirebase(Character playerCharacter, int experienceGained)
    {
        if (FirebaseAuth.DefaultInstance?.CurrentUser != null)
        {
            string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            string characterPath = $"users/{userId}/characters/{playerCharacter.characterName}";

            DatabaseReference databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
            databaseRef.Child(characterPath).Child("experience").SetValueAsync(playerCharacter.characterExperience).ContinueWith(task =>
            {
                if (task.IsCompleted && task.Exception == null)
                {
                    Debug.Log("Experiência atualizada no Firebase com sucesso.");
                }
                else
                {
                    Debug.LogError($"Erro ao atualizar experiência no Firebase: {task.Exception?.Message}");
                }
            });
        }
        else
        {
            Debug.LogWarning("Usuário não está logado. Não é possível atualizar experiência no Firebase.");
        }
    }
    
    // Propriedade pública para verificar se está atacando
    public bool IsAttacking => isAttacking;

    void OnDrawGizmosSelected()
    {
        // Desenha o gizmo da área de ataque 2 unidades acima
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, attackRadius);
    }
}
