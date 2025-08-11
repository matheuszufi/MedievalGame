using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Auth;

public class DashboardUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject characterList;
    public GameObject characterButtonPrefab;
    public GameObject characterPrefab; // Prefab do personagem para instanciar
    public Transform spawnPoint; // Ponto de spawn padrão (opcional)
    public Button createCharacterButton;
    public Button manageAccountButton;
    public Button logoutButton;

    private UIManager uiManager;
    private bool isLoading = false;

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();

        if (uiManager == null)
        {
            Debug.LogError("UIManager não encontrado na cena.");
            return;
        }

        createCharacterButton.onClick.AddListener(OpenCreateCharacterScreen);
        manageAccountButton.onClick.AddListener(OpenManageAccountScreen);
        logoutButton.onClick.AddListener(Logout);
    }

    void OnEnable()
    {
        if (!isLoading && FirebaseAuth.DefaultInstance?.CurrentUser != null)
        {
            LoadCharacters();
        }
    }

    public void LoadCharacters()
    {
        if (isLoading) return;
        
        isLoading = true;
        StartCoroutine(LoadCharactersCoroutine());
    }

    private IEnumerator LoadCharactersCoroutine()
    {
        Debug.Log("Carregando personagens...");
        
        // Limpa a lista atual
        foreach (Transform child in characterList.transform)
        {
            Destroy(child.gameObject);
        }

        if (FirebaseAuth.DefaultInstance?.CurrentUser == null)
        {
            Debug.LogWarning("Usuário não está logado.");
            isLoading = false;
            yield break;
        }

        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        Debug.Log($"User ID: {userId}");
        
        DatabaseReference databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        DatabaseReference charactersRef = databaseRef.Child("users").Child(userId).Child("characters");
        
        // Força busca direto do servidor, sem cache
        charactersRef.KeepSynced(false);
        var task = charactersRef.GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Erro: {task.Exception.Message}");
            isLoading = false;
            yield break;
        }

        if (task.Result.Exists)
        {
            Debug.Log($"Encontrados {task.Result.ChildrenCount} personagens ATUALIZADOS do servidor");
            
            foreach (var character in task.Result.Children)
            {
                string characterName = character.Key;
                string level = character.Child("level").Value?.ToString() ?? "1";

                GameObject characterButton = Instantiate(characterButtonPrefab, characterList.transform);

                Text buttonText = characterButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = $"{characterName} - Level {level}";
                }

                Button button = characterButton.GetComponent<Button>();
                if (button != null)
                {
                    string tempName = characterName;
                    button.onClick.AddListener(() => SelectCharacter(tempName, character));
                }
            }
        }
        else
        {
            Debug.Log("Nenhum personagem encontrado no servidor");
        }

        isLoading = false;
    }

    private void SelectCharacter(string characterName, Firebase.Database.DataSnapshot characterData)
    {
        Debug.Log($"Personagem selecionado: {characterName}");
        
        // Carrega os dados do personagem do Firebase
        LoadCharacterIntoGame(characterName, characterData);
        
        // Vai para a tela de jogo
        uiManager.ShowPlayUIScreen();
    }

    private void LoadCharacterIntoGame(string characterName, Firebase.Database.DataSnapshot characterData)
    {
        // Remove qualquer personagem existente (se houver)
        Character existingCharacter = FindObjectOfType<Character>();
        if (existingCharacter != null)
        {
            Destroy(existingCharacter.gameObject);
        }

        // Verifica se o prefab está configurado
        if (characterPrefab == null)
        {
            Debug.LogError("Character Prefab não está configurado no DashboardUIManager!");
            return;
        }

        // Determina a posição de spawn
        Vector3 spawnPosition;
        float posX = float.Parse(characterData.Child("posX").Value?.ToString() ?? "0");
        float posY = float.Parse(characterData.Child("posY").Value?.ToString() ?? "0");
        
        // Se há uma posição salva, usa ela; senão usa o spawn point padrão
        if (posX != 0 || posY != 0)
        {
            spawnPosition = new Vector3(posX, posY, 0);
        }
        else if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
        }
        else
        {
            spawnPosition = Vector3.zero;
        }

        // Instancia o prefab do personagem
        GameObject characterInstance = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
        Character characterScript = characterInstance.GetComponent<Character>();
        
        if (characterScript != null)
        {
            // Carrega os dados do Firebase para o personagem instanciado
            characterScript.characterName = characterName;
            characterScript.characterLevel = int.Parse(characterData.Child("level").Value?.ToString() ?? "1");
            characterScript.characterHealth = int.Parse(characterData.Child("currentHealth").Value?.ToString() ?? "180");
            characterScript.characterMaxHealth = int.Parse(characterData.Child("maxHealth").Value?.ToString() ?? "180");
            characterScript.characterMana = int.Parse(characterData.Child("currentMana").Value?.ToString() ?? "180");
            characterScript.characterMaxMana = int.Parse(characterData.Child("maxMana").Value?.ToString() ?? "180");
            characterScript.characterExperience = int.Parse(characterData.Child("experience").Value?.ToString() ?? "0");
            characterScript.characterSpeed = int.Parse(characterData.Child("speed").Value?.ToString() ?? "4");
            characterScript.characterPosX = (int)posX;
            characterScript.characterPosY = (int)posY;
            characterScript.characterGold = int.Parse(characterData.Child("goldAmount").Value?.ToString() ?? "0");
            
            Debug.Log($"Personagem {characterName} instanciado na posição: {spawnPosition}");
            Debug.Log($"Stats carregados - Level: {characterScript.characterLevel}, HP: {characterScript.characterHealth}, Gold: {characterScript.characterGold}");
        }
        else
        {
            Debug.LogError("O prefab do personagem não possui o script Character!");
            Destroy(characterInstance);
        }
    }

    private void OpenCreateCharacterScreen()
    {
        uiManager.ShowCreateCharacterScreen();
    }

    private void OpenManageAccountScreen()
    {
        uiManager.ShowManageAccountScreen();
    }

    private void Logout()
    {
        uiManager.ShowLoginScreen();
    }

    public void RefreshCharacterList()
    {
        Debug.Log("Atualizando lista de personagens...");
        isLoading = false; // Reset para permitir novo carregamento
        LoadCharacters();
    }
}
