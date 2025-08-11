using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemySystem
{
    public enum MovementState
    {
        Idle,
        Patrolling,
        Chasing,
        Attacking,
        Returning
    }

    public class EnemyMovement : MonoBehaviour
    {
        [Header("Patrolling Settings")]
        [Tooltip("Radius of the patrolling area")]
        public float patrollingRadius = 5f;

        [Header("Chasing Settings")]
        [Tooltip("Radius of the detection area for chasing players")]
        public float chasingRadius = 10f;
        
        [Header("Stop Distance")]
        [Tooltip("Distance to stop from the player")]
        public float stopDistance = 1f;

        [Header("Grid Settings")]
        [Tooltip("Size of each grid tile")]
        public Vector2 gridSize = new Vector2(2f, 1f); // Isometric 2:1 ratio

        private MovementState currentState = MovementState.Idle;
        private Enemy enemyComponent;
        private EnemyAnim enemyAnim;
        private EnemyCombat enemyCombat;
        private Transform playerTransform;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private bool isMoving = false;
        private float moveTimer = 0f;
        private const float MOVE_INTERVAL = 2f;
        private Vector2 lastMovementDirection = new Vector2(1, -1); // Default: South East
        private float stuckTimer = 0f;
        private const float STUCK_TIME_LIMIT = 5f; // Se ficar preso por 5 segundos, escolhe nova direção
        private Vector3 lastPosition;
        private float movementTimer = 0f;
        private const float MOVEMENT_TIMEOUT = 5f; // Timeout para chegar ao destino
        private Queue<Vector3> pathQueue = new Queue<Vector3>(); // Fila de pontos para seguir
        private bool isFollowingPath = false;
        
        // Isometric directions (NE, NW, SE, SW)
        private readonly Vector2[] isometricDirections = new Vector2[]
        {
            new Vector2(1, 1),   // North East
            new Vector2(-1, 1),  // North West
            new Vector2(1, -1),  // South East
            new Vector2(-1, -1)  // South West
        };

        private void Start()
        {
            enemyComponent = GetComponent<Enemy>();
            if (enemyComponent == null)
            {
                Debug.LogError("Enemy component not found!");
                return;
            }

            enemyAnim = GetComponent<EnemyAnim>();
            if (enemyAnim == null)
            {
                Debug.LogWarning("EnemyAnim component not found!");
            }

            enemyCombat = GetComponent<EnemyCombat>();
            if (enemyCombat == null)
            {
                Debug.LogWarning("EnemyCombat component not found!");
            }

            startPosition = transform.position;
            targetPosition = transform.position;
            lastPosition = transform.position;
            
            // Busca inicial pelo player (pode não existir ainda)
            FindPlayer();

            // Set initial idle animation
            if (enemyAnim != null)
            {
                enemyAnim.SetDirection(lastMovementDirection, "idle");
            }
        }

        private void Update()
        {
            if (enemyComponent == null) return;

            CheckForPlayer();
            HandleMovement();
        }

        private void FindPlayer()
        {
            // Procura por player com tag "Player"
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log($"Player found: {player.name}");
            }
            else
            {
                // Tenta encontrar por componente Character
                Character[] characters = FindObjectsOfType<Character>();
                if (characters.Length > 0)
                {
                    playerTransform = characters[0].transform;
                    Debug.Log($"Player found by Character component: {characters[0].name}");
                }
            }
        }

        private void CheckForPlayer()
        {
            // Se não temos referência do player, tenta encontrar
            if (playerTransform == null)
            {
                FindPlayer();
            }
            
            // Se ainda não encontrou, sai do método
            if (playerTransform == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            
            // Lógica de estados baseada na distância
            if (distanceToPlayer > chasingRadius)
            {
                // Player fora do chasing radius - Estado Patrolling (Idle)
                if (currentState != MovementState.Idle)
                {
                    currentState = MovementState.Idle;
                    isMoving = false;
                    moveTimer = 0f;
                }
            }
            else if (distanceToPlayer <= chasingRadius && distanceToPlayer > stopDistance)
            {
                // Player dentro do chasing radius mas fora da stop distance - Estado Chasing
                if (currentState != MovementState.Chasing)
                {
                    Debug.Log("PLAYER DETECTED! Starting chase...");
                    currentState = MovementState.Chasing;
                    isMoving = false; // Reset movement para calcular nova rota
                }
            }
            else if (distanceToPlayer <= stopDistance)
            {
                // Player muito próximo (dentro da stop distance) - Estado Attacking
                if (currentState != MovementState.Attacking)
                {
                    Debug.Log("ATTACKING PLAYER! Enemy entered attack mode.");
                    currentState = MovementState.Attacking;
                    isMoving = false;
                    
                    // Atualiza animação para idle virado para o player
                    if (enemyAnim != null)
                    {
                        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                        Vector2 attackDirection = GetBestIsometricDirection(directionToPlayer);
                        enemyAnim.SetDirection(attackDirection, "idle");
                        lastMovementDirection = attackDirection;
                    }
                }
            }
        }

        private void HandleMovement()
        {
            CheckIfStuck();
            CheckMovementTimeout();
            
            switch (currentState)
            {
                case MovementState.Idle:
                    HandleIdleMovement();
                    break;
                case MovementState.Chasing:
                    HandleChasing();
                    break;
                case MovementState.Attacking:
                    HandleAttacking();
                    break;
            }
        }

        private void CheckMovementTimeout()
        {
            if (isMoving)
            {
                movementTimer += Time.deltaTime;
                
                if (movementTimer >= MOVEMENT_TIMEOUT)
                {
                    isMoving = false;
                    isFollowingPath = false;
                    pathQueue.Clear();
                    movementTimer = 0f;
                    
                    // Gera uma nova rota
                    if (currentState == MovementState.Idle)
                    {
                        TryMoveRandomDirection();
                    }
                    else if (currentState == MovementState.Chasing && playerTransform != null)
                    {
                        GeneratePathToPlayer();
                    }
                }
            }
            else
            {
                movementTimer = 0f;
            }
        }

        private void CheckIfStuck()
        {
            // Verifica se o inimigo está preso (não se movendo por muito tempo)
            if (isMoving)
            {
                float distanceMoved = Vector3.Distance(transform.position, lastPosition);
                if (distanceMoved < 0.01f) // Se quase não se moveu
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer >= STUCK_TIME_LIMIT)
                    {
                        // Se ficar preso por muito tempo, para o movimento e escolhe nova direção
                        Debug.Log("Enemy stuck! Choosing new direction...");
                        isMoving = false;
                        stuckTimer = 0f;
                        
                        // Força escolha de nova direção
                        if (currentState == MovementState.Idle)
                        {
                            TryMoveRandomDirection();
                        }
                        
                        // Update animation to idle when stuck
                        if (enemyAnim != null)
                        {
                            enemyAnim.SetDirection(lastMovementDirection, "idle");
                        }
                    }
                }
                else
                {
                    stuckTimer = 0f; // Reset timer se está se movendo
                }
                
                lastPosition = transform.position;
            }
        }

        

        private void HandleIdleMovement()
        {
            if (isMoving)
            {
                MoveToTarget();
                return;
            }

            moveTimer += Time.deltaTime;
            if (moveTimer >= MOVE_INTERVAL)
            {
                moveTimer = 0f;
                TryMoveRandomDirection();
            }
        }

        private void HandleChasing()
        {
            if (playerTransform == null) return;

            if (!isMoving && !isFollowingPath)
            {
                GeneratePathToPlayer();
            }
            else
            {
                MoveToTarget();
            }
        }

        private void HandleAttacking()
        {
            // Para completamente o movimento durante o ataque
            isMoving = false;
            
            // Mantém a direção virada para o player
            if (playerTransform != null)
            {
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                Vector2 attackDirection = GetBestIsometricDirection(directionToPlayer);
                
                // Atualiza animação de idle apenas se a direção mudou
                if (lastMovementDirection != attackDirection && enemyAnim != null)
                {
                    enemyAnim.SetDirection(attackDirection, "idle");
                    lastMovementDirection = attackDirection;
                }
            }
            
            // Executa ataque se o componente de combate existir
            if (enemyCombat != null)
            {
                enemyCombat.TryAttackPlayer();
            }
            
            // Log de debug para mostrar que está atacando
        }

        private void GeneratePathToPlayer()
        {
            if (playerTransform == null) return;
            
            Vector3 playerPos = playerTransform.position;
            Vector3 currentPos = transform.position;
            
            // Verifica se já está próximo o suficiente (dentro da stop distance)
            float distanceToPlayer = Vector3.Distance(currentPos, playerPos);
            if (distanceToPlayer <= stopDistance)
            {
                return;
            }
            
            // Tenta movimento direto primeiro
            Vector3 directionToPlayer = (playerPos - currentPos).normalized;
            Vector2 bestDirection = GetBestIsometricDirection(directionToPlayer);
            Vector3 directTarget = currentPos + new Vector3(bestDirection.x * gridSize.x, bestDirection.y * gridSize.y, 0);
            
            // Verifica se o próximo movimento não vai passar da stop distance
            float distanceAfterMove = Vector3.Distance(directTarget, playerPos);
            if (distanceAfterMove < stopDistance)
            {
                // Movimento direto passaria da stop distance, calcula posição exata
                Vector3 idealPosition = playerPos - (directionToPlayer * stopDistance);
                
                // Usa a direção mais próxima do ideal
                Vector3 directionToIdeal = (idealPosition - currentPos).normalized;
                bestDirection = GetBestIsometricDirection(directionToIdeal);
                directTarget = currentPos + new Vector3(bestDirection.x * gridSize.x, bestDirection.y * gridSize.y, 0);
            }
            
            if (IsValidPosition(directTarget) && !HasObstacleInPath(currentPos, directTarget))
            {
                // Movimento direto possível
                SetSingleTarget(directTarget, bestDirection);
            }
            else
            {
                // Precisa encontrar rota alternativa
                FindAlternativePath(playerPos);
            }
        }

        private void FindAlternativePath(Vector3 targetPos)
        {
            // Tenta diferentes direções para contornar obstáculos
            Vector3 currentPos = transform.position;
            List<Vector2> availableDirections = new List<Vector2>();
            
            foreach (Vector2 direction in isometricDirections)
            {
                Vector3 testPos = currentPos + new Vector3(direction.x * gridSize.x, direction.y * gridSize.y, 0);
                if (IsValidPosition(testPos) && !HasObstacleInPath(currentPos, testPos))
                {
                    availableDirections.Add(direction);
                }
            }
            
            if (availableDirections.Count > 0)
            {
                // Escolhe a direção que mais se aproxima do player
                Vector2 bestDirection = GetBestDirectionTowardsTarget(availableDirections, targetPos);
                Vector3 nextPos = currentPos + new Vector3(bestDirection.x * gridSize.x, bestDirection.y * gridSize.y, 0);
                SetSingleTarget(nextPos, bestDirection);
            }
            else
            {
                Debug.Log("No valid path found. Staying in place.");
            }
        }

        private Vector2 GetBestDirectionTowardsTarget(List<Vector2> directions, Vector3 targetPos)
        {
            Vector2 bestDirection = directions[0];
            float bestScore = float.MinValue;
            
            Vector3 directionToTarget = (targetPos - transform.position).normalized;
            
            foreach (Vector2 direction in directions)
            {
                Vector3 dir3D = new Vector3(direction.x, direction.y, 0).normalized;
                float dot = Vector3.Dot(directionToTarget, dir3D);
                
                if (dot > bestScore)
                {
                    bestScore = dot;
                    bestDirection = direction;
                }
            }
            
            return bestDirection;
        }

        private void SetSingleTarget(Vector3 target, Vector2 direction)
        {
            targetPosition = target;
            lastMovementDirection = direction;
            isMoving = true;
            isFollowingPath = false;
            movementTimer = 0f;
            
            if (enemyAnim != null)
            {
                enemyAnim.SetDirection(lastMovementDirection, "movement");
            }
        }

        private bool HasObstacleInPath(Vector3 start, Vector3 end)
        {
            // Verifica se há obstáculos no caminho usando Raycast
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            
            RaycastHit2D hit = Physics2D.Raycast(start, direction, distance);
            
            // Se atingiu algo que não seja o próprio inimigo, há obstáculo
            return hit.collider != null && hit.collider.transform != transform;
        }

        private void TryMoveRandomDirection()
        {
            List<Vector2> validDirections = new List<Vector2>();
            
            foreach (Vector2 direction in isometricDirections)
            {
                Vector3 newPosition = transform.position + new Vector3(direction.x * gridSize.x, direction.y * gridSize.y, 0);
                if (IsValidPosition(newPosition))
                {
                    validDirections.Add(direction);
                }
            }

            if (validDirections.Count > 0)
            {
                Vector2 randomDirection = validDirections[Random.Range(0, validDirections.Count)];
                targetPosition = transform.position + new Vector3(randomDirection.x * gridSize.x, randomDirection.y * gridSize.y, 0);
                lastMovementDirection = randomDirection;
                isMoving = true;
                movementTimer = 0f;

                // Update animation to movement
                if (enemyAnim != null)
                {
                    enemyAnim.SetDirection(lastMovementDirection, "movement");
                }
            }
        }

        private void MoveToTarget()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, enemyComponent.speed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                movementTimer = 0f;

                // Update animation to idle when movement stops
                if (enemyAnim != null)
                {
                    enemyAnim.SetDirection(lastMovementDirection, "idle");
                }
            }
        }

        private Vector2 GetBestIsometricDirection(Vector3 worldDirection)
        {
            Vector2 bestDirection = isometricDirections[0];
            float bestDot = -1f;

            foreach (Vector2 direction in isometricDirections)
            {
                Vector3 dir3D = new Vector3(direction.x, direction.y, 0).normalized;
                float dot = Vector3.Dot(worldDirection, dir3D);
                
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestDirection = direction;
                }
            }

            return bestDirection;
        }

        private bool IsValidPosition(Vector3 position)
        {
            // Check if position is within patrolling radius
            if (Vector3.Distance(startPosition, position) > patrollingRadius)
                return false;

            // Check for colliders at the target position
            Collider2D collider = Physics2D.OverlapPoint(position);
            return collider == null || collider.transform == transform;
        }

        // Public method to get the last movement direction for animation
        public Vector2 GetLastMovementDirection()
        {
            return lastMovementDirection;
        }

        // Public property to check if enemy is currently moving
        public bool IsMoving => isMoving;

        private void OnDrawGizmosSelected()
        {
            // Draw patrolling radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, patrollingRadius);

            // Draw chasing radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chasingRadius);
            
            // Draw stop distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stopDistance);

            // Draw grid
            Gizmos.color = Color.cyan;
            if (isMoving)
            {
                Gizmos.DrawLine(transform.position, targetPosition);
                Gizmos.DrawWireCube(targetPosition, Vector3.one * 0.5f);
            }
            
            // Draw line to player if chasing or attacking
            if (playerTransform != null && (currentState == MovementState.Chasing || currentState == MovementState.Attacking))
            {
                Gizmos.color = currentState == MovementState.Attacking ? Color.red : Color.blue;
                Gizmos.DrawLine(transform.position, playerTransform.position);
            }
        }
    }
}
