using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelector : MonoBehaviour
{
    public GameObject selectUI; // Referência ao UI Image para seleção

    public static GameObject currentSelection; // Objeto atualmente selecionado

    [Header("Debug")] // Adiciona uma seção de depuração no Inspector
    [SerializeField]
    private GameObject currentSelectionDebug; // Exibe o inimigo selecionado no Inspector

    void Start()
    {
        if (selectUI != null)
        {
            selectUI.SetActive(false); // Garante que o selectUI começa desativado
        }
    }

    void OnMouseDown()
    {
        if (currentSelection == gameObject)
        {
            DeselectObject(); // Cancela a seleção se o mesmo objeto for clicado
        }
        else
        {
            if (currentSelection != null)
            {
                currentSelection.GetComponent<EnemySelector>().DeselectObject(); // Desativa o selectUI do objeto anterior
            }

            SelectObject();
        }
    }

    private void SelectObject()
    {
        currentSelection = gameObject;

        if (selectUI != null)
        {
            selectUI.SetActive(true); // Ativa o selectUI
        }
    }

    private void DeselectObject()
    {
        if (selectUI != null)
        {
            selectUI.SetActive(false); // Desativa o selectUI
        }

        currentSelection = null;
    }

    private void Update()
    {
        // Atualiza o campo de depuração com o inimigo atualmente selecionado
        currentSelectionDebug = currentSelection;
    }
}
