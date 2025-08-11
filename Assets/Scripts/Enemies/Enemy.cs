using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.UI; // Necessário para manipular elementos de UI


namespace EnemySystem
{
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy Attributes")]
        [Tooltip("Name of the enemy")]
        public string enemyName;
        public int maxHp;
        public int currentHp;
        public int experience;
        public float speed;
        public int damage;

        [Header("UI Elements")]
        public Image lifeBar;

        [Header("Gold Drop Settings")]
        public int minGoldDrop = 10; // Mínimo de ouro que pode ser dropado
        public int maxGoldDrop = 50; // Máximo de ouro que pode ser dropado

        private bool isDead = false;

        public EnemyRespawnManager respawnManager; // Referência ao gerenciador de respawn
        public EnemyConfig respawnConfig; // Configuração de respawn associada

        private Character character;

        private void Start()
        {
            StartCoroutine(FindCharacterWithDelay());
            UpdateLifeBar();
        }

        private IEnumerator FindCharacterWithDelay()
        {
            // Tenta encontrar o personagem por alguns frames
            int attempts = 0;
            while (character == null && attempts < 30)
            {
                // Busca por diferentes métodos
                character = FindObjectOfType<Character>();
                
                if (character == null)
                {
                    // Busca alternativa por GameObject com tag
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null)
                    {
                        character = playerObj.GetComponent<Character>();
                    }
                }
                
                if (character == null)
                {
                    // Busca alternativa por nome
                    GameObject[] allObjects = FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        Character charComponent = obj.GetComponent<Character>();
                        if (charComponent != null)
                        {
                            character = charComponent;
                            break;
                        }
                    }
                }
                
                if (character == null)
                {
                    yield return new WaitForSeconds(0.2f);
                    attempts++;
                }
            }
        }

        private void Update()
        {
            // Verifica se o HP do inimigo chegou a 0 ou menos e se ainda não morreu
            if (currentHp <= 0 && !isDead)
            {
                Die();
            }
        }

        public void TakeDamage(int damage)
        {
            currentHp -= damage;
            if (currentHp < 0) currentHp = 0;
            UpdateLifeBar();
        }

        private void UpdateLifeBar()
        {
            if (lifeBar != null)
            {
                lifeBar.fillAmount = (float)currentHp / maxHp;
            }
        }

        private void Die()
        {
            isDead = true;

            // Remove da seleção se estiver selecionado
            EnemySelector enemySelector = GetComponent<EnemySelector>();
            if (enemySelector != null && EnemySelector.currentSelection == gameObject)
            {
                EnemySelector.currentSelection = null;
            }

            // Se character ainda é null, tenta encontrar novamente
            if (character == null)
            {
                character = FindObjectOfType<Character>();
                
                if (character == null)
                {
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null)
                    {
                        character = playerObj.GetComponent<Character>();
                    }
                }
            }

            // Dropar ouro diretamente
            if (character != null)
            {
                DropGold(character);
            }

            // Notifica o gerenciador de respawn
            if (respawnManager != null && respawnConfig != null)
            {
                respawnManager.OnEnemyDied(respawnConfig);
            }

            Destroy(gameObject);
        }

        // Método para resetar o inimigo ao máximo de vida (útil para respawn)
        public void ResetToFullHealth()
        {
            currentHp = maxHp;
            isDead = false;
            UpdateLifeBar();
        }

        // Método para dropar ouro ao morrer
        public void DropGold(Character character)
        {
            if (character != null)
            {
                int goldAmount = Random.Range(minGoldDrop, maxGoldDrop + 1);
                character.characterGold += goldAmount;

                // Atualiza o ouro no UI
                PlayUIManager.Instance?.UpdateCharacterGold(character.characterGold);

                // Atualiza o ouro no Firebase como goldAmount
                character.UpdateFirebaseProperty("goldAmount", character.characterGold);
            }
        }
    }
}

