using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject loginScreen;
    public GameObject createAccountScreen;
    public GameObject dashboardScreen;
    public GameObject createCharacterScreen;
    public GameObject manageAccountScreen;
    public GameObject playUIScreen;



    // Start is called before the first frame update
    void Start()
    {
        ShowLoginScreen(); // Exibe a tela de login ao iniciar o jogo
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadNewScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ShowLoginScreen()
    {
        loginScreen.SetActive(true);
        createAccountScreen.SetActive(false);
        dashboardScreen.SetActive(false);
        createCharacterScreen.SetActive(false);
        manageAccountScreen.SetActive(false);
        playUIScreen.SetActive(false);
    }

    public void ShowCreateAccountScreen()
    {
        loginScreen.SetActive(false);
        createAccountScreen.SetActive(true);
        dashboardScreen.SetActive(false);
        createCharacterScreen.SetActive(false);
        manageAccountScreen.SetActive(false);
        playUIScreen.SetActive(false);
    }

    public void ShowDashboardScreen()
    {
        loginScreen.SetActive(false);
        createAccountScreen.SetActive(false);
        dashboardScreen.SetActive(true);
        createCharacterScreen.SetActive(false);
        manageAccountScreen.SetActive(false);
        playUIScreen.SetActive(false);

        Debug.Log("Dashboard exibido - atualizando lista de personagens...");
        
        // Atualiza a lista de personagens quando mostrar o dashboard
        DashboardUIManager dashboardManager = FindObjectOfType<DashboardUIManager>();
        if (dashboardManager != null)
        {
            dashboardManager.RefreshCharacterList();
        }
    }

    public void ShowCreateCharacterScreen()
    {
        loginScreen.SetActive(false);
        createAccountScreen.SetActive(false);
        dashboardScreen.SetActive(false);
        createCharacterScreen.SetActive(true);
        manageAccountScreen.SetActive(false);
        playUIScreen.SetActive(false);
    }

    public void ShowManageAccountScreen()
    {
        loginScreen.SetActive(false);
        createAccountScreen.SetActive(false);
        dashboardScreen.SetActive(false);
        createCharacterScreen.SetActive(false);
        manageAccountScreen.SetActive(true);
        playUIScreen.SetActive(false);
    }

    public void ShowPlayUIScreen()
    {
        loginScreen.SetActive(false);
        createAccountScreen.SetActive(false);
        dashboardScreen.SetActive(false);
        createCharacterScreen.SetActive(false);
        manageAccountScreen.SetActive(false);
        playUIScreen.SetActive(true);

        Debug.Log("Tela de jogo exibida.");
    }
}
