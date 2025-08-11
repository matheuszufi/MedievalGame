using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Cinemachine")]
    public CinemachineVirtualCamera virtualCamera;
    
    [Header("Player Detection")]
    public float searchInterval = 0.5f; // Busca por player a cada 0.5 segundos
    
    private Transform currentPlayerTarget;
    private float lastSearchTime;

    void Start()
    {
        // Se não foi configurado no Inspector, tenta encontrar automaticamente
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }

        if (virtualCamera == null)
        {
            Debug.LogError("CinemachineVirtualCamera não encontrada! Adicione uma à cena ou configure no Inspector.");
        }
    }

    void Update()
    {
        // Procura por um player periodicamente se não há target atual
        if (currentPlayerTarget == null && Time.time - lastSearchTime >= searchInterval)
        {
            SearchForPlayer();
            lastSearchTime = Time.time;
        }
    }

    private void SearchForPlayer()
    {
        // Procura por um GameObject com script Character (que seria o player)
        Character player = FindObjectOfType<Character>();
        
        if (player != null && player.transform != currentPlayerTarget)
        {
            SetCameraTarget(player.transform);
            Debug.Log($"Camera agora segue o player: {player.characterName}");
        }
    }

    public void SetCameraTarget(Transform target)
    {
        if (virtualCamera != null && target != null)
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
            currentPlayerTarget = target;
            
            Debug.Log($"Camera configurada para seguir: {target.name}");
        }
        else
        {
            Debug.LogWarning("VirtualCamera ou Target está nulo!");
        }
    }

    public void ClearCameraTarget()
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = null;
            virtualCamera.LookAt = null;
            currentPlayerTarget = null;
            
            Debug.Log("Camera target removido.");
        }
    }

    // Método público para forçar busca imediata por player
    public void FindAndFollowPlayer()
    {
        SearchForPlayer();
    }
}
