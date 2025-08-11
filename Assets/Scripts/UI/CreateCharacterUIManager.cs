using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateCharacterUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField characterNameInput;
    public Button createCharacterButton;
    public Button backButton; // Novo botão de voltar

    private AuthManager authManager; // Substitui FirebaseManager por AuthManager

    // Start is called before the first frame update
    void Start()
    {
        // Obtém referência ao AuthManager
        authManager = FindObjectOfType<AuthManager>();

        if (authManager == null)
        {
            Debug.LogError("AuthManager não encontrado na cena.");
            return;
        }

        // Configura os listeners dos botões
        createCharacterButton.onClick.AddListener(CreateCharacter);
        backButton.onClick.AddListener(BackToDashboard);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CreateCharacter()
    {
        string characterName = characterNameInput.text;

        if (string.IsNullOrEmpty(characterName))
        {
            Debug.LogError("O nome do personagem não pode estar vazio.");
            return;
        }

        string userId = authManager.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Usuário não está autenticado. Não é possível criar o personagem.");
            return;
        }

        // Dados do personagem como dicionário
        Dictionary<string, object> characterData = new Dictionary<string, object>
        {
            { "maxHealth", 180 },
            { "currentHealth", 180 },
            { "maxMana", 180 },
            { "currentMana", 180 },
            { "posX", 0 },
            { "posY", 0 },
            { "speed", 4f },
            { "level", 1 },
            { "experience", 0 },
            { "goldAmount", 0 }
        };

        // Caminho no Firebase
        string characterPath = $"users/{userId}/characters/{characterName}";

        // Salva no Firebase
        authManager.SaveCharacter(characterPath, characterData, (success, message) =>
        {
            if (success)
            {
                Debug.Log("Personagem criado com sucesso!");
                
                // Vai para o dashboard e atualiza a lista
                BackToDashboard();
                
                // Atualiza a lista de personagens no dashboard
                DashboardUIManager dashboardManager = FindObjectOfType<DashboardUIManager>();
                if (dashboardManager != null)
                {
                    dashboardManager.RefreshCharacterList();
                }
            }
            else
            {
                Debug.LogError($"Erro ao criar personagem: {message}");
            }
        });
    }

    private void BackToDashboard()
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowDashboardScreen();
        }
        else
        {
            Debug.LogError("UIManager não encontrado na cena.");
        }
    }
}
