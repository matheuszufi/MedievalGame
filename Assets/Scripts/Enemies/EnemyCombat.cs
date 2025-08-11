using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemySystem
{
    public class EnemyCombat : MonoBehaviour
    {
        [Header("Combat Settings")]
        [Tooltip("Damage dealt to the player")]
        public int attackDamage = 10;
        
        [Tooltip("Time between attacks")]
        public float attackCooldown = 2f;
        
        [Tooltip("Range to attack the player")]
        public float attackRange = 1f;
        
        private float lastAttackTime = 0f;
        private Enemy enemyComponent;
        private Transform playerTransform;
        
        private void Start()
        {
            enemyComponent = GetComponent<Enemy>();
            
            if (enemyComponent == null)
            {
                Debug.LogError("Enemy component not found on EnemyCombat!");
            }
        }
        
        private void Update()
        {
            // Busca o player se ainda não foi encontrado
            if (playerTransform == null)
            {
                FindPlayer();
            }
            else
            {
                TryAttackPlayer();
            }
        }
        
        private void FindPlayer()
        {
            // Procura por player com tag "Player"
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                // Tenta encontrar por componente Character
                Character[] characters = FindObjectsOfType<Character>();
                if (characters.Length > 0)
                {
                    playerTransform = characters[0].transform;
                }
   
            }
        }
        
        public void TryAttackPlayer()
        {
            // Verifica se pode atacar (cooldown)
            if (Time.time - lastAttackTime < attackCooldown)
            {
                return;
            }

            // Verifica se o player existe e está no alcance
            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }
            
        }
        
        private void AttackPlayer()
        {
            Character playerCharacter = playerTransform.GetComponent<Character>();
            if (playerCharacter != null)
            {
                // Aplica dano ao player
                playerCharacter.TakeDamage(attackDamage);
            }
       
        }
        
        // Método público para verificar se pode atacar
        public bool CanAttack()
        {
            return Time.time - lastAttackTime >= attackCooldown;
        }
        
        // Método público para obter o alcance de ataque
        public float GetAttackRange()
        {
            return attackRange;
        }
    }
}