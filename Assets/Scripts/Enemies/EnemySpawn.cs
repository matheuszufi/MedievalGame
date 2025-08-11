using System.Collections;
using UnityEngine;
using EnemySystem;

public class EnemySpawn : MonoBehaviour
{
    [Tooltip("Tempo em segundos até o próximo respawn após a morte do inimigo")]
    public float respawnTime = 5f;

    [Tooltip("Prefab do inimigo que será instanciado")]
    public GameObject enemyPrefab;

    private GameObject currentEnemy;

    void Start()
    {
        SpawnEnemy();
    }

    public void OnEnemyDied()
    {
        if (currentEnemy != null)
        {
            Debug.Log("[EnemySpawn] Destruindo o inimigo atual...");
            Destroy(currentEnemy); // Destroi o inimigo atual
            StartCoroutine(RespawnCoroutine()); // Inicia o respawn
        }
        else
        {
            Debug.LogWarning("[EnemySpawn] Nenhum inimigo para destruir.");
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            Debug.Log("[EnemySpawn] Instanciando um novo inimigo...");
            currentEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogError("[EnemySpawn] Nenhum prefab de inimigo foi atribuído no Inspector.");
        }
    }
}
