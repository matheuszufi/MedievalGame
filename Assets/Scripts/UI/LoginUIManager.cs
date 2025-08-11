using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginUIManager : MonoBehaviour
{
    [Header("Login UI")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button createAccountButton;

    [Header("Managers")]
    public AuthManager authManager;
    public UIManager uiManager;

    void Start()
    {
        // Configura os botões
        if (loginButton != null)
            loginButton.onClick.AddListener(LoginUser);
        
        if (createAccountButton != null)
            createAccountButton.onClick.AddListener(GoToCreateAccount);
    }

    public void LoginUser()
    {
        // Valida os campos obrigatórios
        if (string.IsNullOrEmpty(emailInput.text))
        {
            Debug.LogWarning("Email é obrigatório!");
            return;
        }

        if (string.IsNullOrEmpty(passwordInput.text))
        {
            Debug.LogWarning("Senha é obrigatória!");
            return;
        }

        // Chama o AuthManager para fazer login
        if (authManager != null)
        {
            authManager.LoginUserFromUI(emailInput.text, passwordInput.text);
        }
        else
        {
            Debug.LogError("AuthManager não está configurado!");
        }
    }

    public void GoToCreateAccount()
    {
        if (uiManager != null)
        {
            Debug.Log("Navegando para a tela de criação de conta...");
            ClearLoginFields();
            uiManager.ShowCreateAccountScreen();
        }
        else
        {
            Debug.LogError("UIManager não está configurado!");
        }
    }

    private void ClearLoginFields()
    {
        if (emailInput != null) emailInput.text = "";
        if (passwordInput != null) passwordInput.text = "";
    }

    void Update()
    {
        // Permite fazer login pressionando Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            LoginUser();
        }
    }
}
