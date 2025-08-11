using System.Collections;
using System.Collections.Generic;
using EnemySystem;
using UnityEngine;

[System.Serializable]
public class EnemyConfig
{
    public GameObject enemyPrefab; // Prefab do inimigo
    public float respawnDelay = 5f; // Tempo de espera para respawn
}

public class EnemyRespawnManager : MonoBehaviour
{
    [Header("Respawn Area Settings")]
    public float areaRadius; // Raio da área de respawn

    [Header("Enemy Settings")]
    public List<EnemyConfig> enemyConfigs = new List<EnemyConfig>(); // Lista de inimigos e seus tempos de respawn

    private Dictionary<EnemyConfig, GameObject> currentEnemies = new Dictionary<EnemyConfig, GameObject>();

    private void Start()
    {
        foreach (var config in enemyConfigs)
        {
            SpawnEnemy(config); // Instancia um inimigo para cada configuração no início
        }
    }

    public void OnEnemyDied(EnemyConfig config)
    {
        if (currentEnemies.ContainsKey(config) && currentEnemies[config] != null)
        {
            Destroy(currentEnemies[config]); // Destroi o inimigo atual
        }
        StartCoroutine(RespawnCoroutine(config)); // Inicia o respawn
    }

    private IEnumerator RespawnCoroutine(EnemyConfig config)
    {
        yield return new WaitForSeconds(config.respawnDelay); // Aguarda o tempo de respawn
        SpawnEnemy(config); // Spawna um novo inimigo
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, areaRadius); // Desenha a área de respawn como um círculo
    }

    private void SpawnEnemy(EnemyConfig config)
    {
        Vector2 randomCircle = Random.insideUnitCircle * areaRadius;
        Vector3 randomPosition = new Vector3(
            transform.position.x + randomCircle.x,
            transform.position.y,
            0 // Define a posição Z como sempre 0
        );

        GameObject newEnemy = Instantiate(config.enemyPrefab, randomPosition, Quaternion.identity); // Instancia o inimigo
        currentEnemies[config] = newEnemy;
        Enemy enemyScript = newEnemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.respawnManager = this; // Conecta o script de respawn
            enemyScript.respawnConfig = config; // Associa a configuração de respawn
        }
    }
}