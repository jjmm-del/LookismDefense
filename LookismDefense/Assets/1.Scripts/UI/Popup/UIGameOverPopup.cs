using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIGameOverPopup : UIPopup
{
    [SerializeField]
    private TMP_Text reasonText;
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += ShowGameOver;
        }

        Hide();
        
    }

    private void ShowGameOver(string reason)
    {
        if (reasonText != null)
        {
            reasonText.text = reason;
        }

        UIManager.Instance?.OpenPopup(this);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= ShowGameOver;
        }
    }
}
