using System;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTextmesh;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;


    private void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (e.winPlayerType == GameManager.Instance.GetLocalPlayerType())
        {
            resultTextmesh.text = "YOU WIN!!";
            resultTextmesh.color = winColor;
        }
        else
        {
            resultTextmesh.text = "YOU LOSE!!";
            resultTextmesh.color = loseColor;
        }
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
