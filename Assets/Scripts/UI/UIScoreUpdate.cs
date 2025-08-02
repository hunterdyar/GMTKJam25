using System;
using GMTK;
using TMPro;
using UnityEngine;

public class UIScoreUpdate : MonoBehaviour
{
    private GameManager gameManager;
    private TMP_Text _text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        gameManager = GameObject.FindFirstObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        UpdateScore();
    }

    public void UpdateScore()
    {
        _text.text = "Your Time: " + gameManager.Score + " frames";
    }
}
