using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser user;

    [Header("Login References")]
    public TMPro.TMP_InputField loginEmailField;
    public TMPro.TMP_InputField loginPasswordField;
    public UnityEngine.UI.Button loginButton;

    [Header("Register References")]
    public TMPro.TMP_InputField registerEmailField;
    public TMPro.TMP_InputField registerPasswordField;
    public TMPro.TMP_InputField registerConfirmPasswordField;
    public UnityEngine.UI.Button registerButton;

    [Header("UI Manager")]
    public UIManager uiManager;

    void Start()
    {
        // Inicializa o Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase Auth inicializado com sucesso!");
            }
            else
            {
                Debug.LogError("Não foi possível inicializar o Firebase: " + task.Result);
            }
        });

        // Configura os botões
        if (loginButton != null)
            loginButton.onClick.AddListener(LoginUser);
        
        if (registerButton != null)
            registerButton.onClick.AddListener(RegisterUser);
    }

    public void LoginUser()
    {
        if (string.IsNullOrEmpty(loginEmailField.text) || string.IsNullOrEmpty(loginPasswordField.text))
        {
            Debug.LogWarning("Email e senha são obrigatórios!");
            return;
        }

        LoginUserAsync(loginEmailField.text, loginPasswordField.text);
    }

    public void RegisterUser()
    {
        if (string.IsNullOrEmpty(registerEmailField.text) || string.IsNullOrEmpty(registerPasswordField.text))
        {
            Debug.LogWarning("Email e senha são obrigatórios!");
            return;
        }

        if (registerPasswordField.text != registerConfirmPasswordField.text)
        {
            Debug.LogWarning("As senhas não coincidem!");
            return;
        }

        RegisterUserAsync(registerEmailField.text, registerPasswordField.text);
    }

    public void CreateAccountFromUI(string email, string password, string accountName)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Email e senha são obrigatórios!");
            return;
        }

        if (string.IsNullOrEmpty(accountName))
        {
            Debug.LogWarning("Nome da conta é obrigatório!");
            return;
        }

        RegisterUserAsync(email, password, accountName);
    }

    public void LoginUserFromUI(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Email e senha são obrigatórios!");
            return;
        }

        LoginUserAsync(email, password);
    }

    private async void LoginUserAsync(string email, string password)
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = result.User;
            
            Debug.Log("Login realizado com sucesso! Usuário: " + user.Email);
            
            // Limpa os campos
            ClearLoginFields();
            
            // Vai para o dashboard
            if (uiManager != null)
            {
                Debug.Log("UIManager encontrado, chamando ShowDashboardScreen...");
                uiManager.ShowDashboardScreen();
            }
            else
            {
                Debug.LogError("UIManager não está configurado! Arraste o GameObject com UIManager no Inspector.");
            }
        }
        catch (FirebaseException ex)
        {
            Debug.LogError("Erro no login: " + ex.Message);
            HandleAuthError(ex);
        }
    }

    private async void RegisterUserAsync(string email, string password, string accountName = "")
    {
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;
            
            Debug.Log("Conta criada com sucesso! Usuário: " + user.Email);

            // Se foi fornecido um nome de conta, salva no perfil do usuário
            if (!string.IsNullOrEmpty(accountName))
            {
                var profile = new Firebase.Auth.UserProfile 
                {
                    DisplayName = accountName
                };
                await user.UpdateUserProfileAsync(profile);
                Debug.Log($"Nome da conta definido como: {accountName}");
            }
            
            // Limpa os campos
            ClearRegisterFields();
            
            // Vai para o dashboard
            if (uiManager != null)
            {
                Debug.Log("UIManager encontrado, chamando ShowDashboardScreen...");
                uiManager.ShowDashboardScreen();
            }
            else
            {
                Debug.LogError("UIManager não está configurado! Arraste o GameObject com UIManager no Inspector.");
            }
        }
        catch (FirebaseException ex)
        {
            Debug.LogError("Erro ao criar conta: " + ex.Message);
            HandleAuthError(ex);
        }
    }

    public void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
            user = null;
            Debug.Log("Logout realizado com sucesso!");
            
            // Volta para a tela de login
            if (uiManager != null)
                uiManager.ShowLoginScreen();
        }
    }

    private void HandleAuthError(FirebaseException ex)
    {
        AuthError errorCode = (AuthError)ex.ErrorCode;
        
        switch (errorCode)
        {
            case AuthError.InvalidEmail:
                Debug.LogError("Email inválido!");
                break;
            case AuthError.WrongPassword:
                Debug.LogError("Senha incorreta!");
                break;
            case AuthError.UserNotFound:
                Debug.LogError("Usuário não encontrado!");
                break;
            case AuthError.EmailAlreadyInUse:
                Debug.LogError("Este email já está em uso!");
                break;
            case AuthError.WeakPassword:
                Debug.LogError("Senha muito fraca!");
                break;
            default:
                Debug.LogError("Erro de autenticação: " + ex.Message);
                break;
        }
    }

    private void ClearLoginFields()
    {
        if (loginEmailField != null) loginEmailField.text = "";
        if (loginPasswordField != null) loginPasswordField.text = "";
    }

    private void ClearRegisterFields()
    {
        if (registerEmailField != null) registerEmailField.text = "";
        if (registerPasswordField != null) registerPasswordField.text = "";
        if (registerConfirmPasswordField != null) registerConfirmPasswordField.text = "";
    }

    public bool IsUserLoggedIn()
    {
        return user != null;
    }

    public string GetUserEmail()
    {
        return user != null ? user.Email : "";
    }

    public string GetUserId()
    {
        return user != null ? user.UserId : "";
    }

    public string GetAccountName()
    {
        return user != null && !string.IsNullOrEmpty(user.DisplayName) ? user.DisplayName : "Usuário";
    }

    public void SaveCharacter(string characterPath, Dictionary<string, object> characterData, System.Action<bool, string> callback)
    {
        if (user == null)
        {
            Debug.LogError("Usuário não está autenticado. Não é possível salvar o personagem.");
            callback?.Invoke(false, "Usuário não autenticado");
            return;
        }

        var dbReference = Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference;

        dbReference.Child(characterPath).SetValueAsync(characterData).ContinueWith(task =>
        {
            if (task.IsCompleted && task.Exception == null)
            {
                Debug.Log("Personagem salvo com sucesso no Firebase Realtime Database.");
                callback?.Invoke(true, "Personagem salvo com sucesso");
            }
            else
            {
                Debug.LogError("Erro ao salvar personagem no Firebase Realtime Database: " + task.Exception);
                callback?.Invoke(false, task.Exception?.Message);
            }
        });
    }
}
