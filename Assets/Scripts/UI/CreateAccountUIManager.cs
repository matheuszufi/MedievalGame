using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CreateAccountUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField accountNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public Button createAccountButton;
    public Button backButton;

    // Start is called before the first frame update
    void Start()
    {
        // Configura os listeners dos botões
        createAccountButton.onClick.AddListener(OnCreateAccountClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCreateAccountClicked()
    {
        string accountName = accountNameInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            Debug.LogError("Todos os campos devem ser preenchidos.");
            return;
        }

        if (password != confirmPassword)
        {
            Debug.LogError("As senhas não coincidem.");
            return;
        }

        // Lógica para criar conta (exemplo: chamar AuthManager ou Firebase)
        Debug.Log($"Criando conta para {accountName} com email {email}");
        // AuthManager.CreateAccount(email, password, OnAccountCreated);
    }

    private void OnBackClicked()
    {
        // Volta para a tela de login
        Debug.Log("Voltando para a tela de login.");
        SceneManager.LoadScene("LoginUI");
    }

    // Callback para criação de conta (exemplo)
    private void OnAccountCreated(bool success, string message)
    {
        if (success)
        {
            Debug.Log("Conta criada com sucesso!");
            SceneManager.LoadScene("LoginUI");
        }
        else
        {
            Debug.LogError($"Erro ao criar conta: {message}");
        }
    }
}
