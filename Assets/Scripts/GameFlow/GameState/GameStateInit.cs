﻿using TMPro;
using UnityEngine;

public class GameStateInit : GameState
{
    public GameObject menuUI;
    [SerializeField] private TextMeshProUGUI hiscoreText;
    [SerializeField] private TextMeshProUGUI fishcountText;
    [SerializeField] private AudioClip menuLoopMusic;

    public override void Construct()
    {

        Debug.Log("✅ GameStateInit.Construct() được gọi!");
        GameManager.Instance.ChangeCamera(GameCamera.Init);

        if (SaveManager.Instance != null && SaveManager.Instance.save != null)
        {
            Debug.Log("✅ Load UI từ SaveManager:");
            Debug.Log("📌 Highscore: " + SaveManager.Instance.save.Highscore);
            Debug.Log("📌 Fish: " + SaveManager.Instance.save.Fish);

            hiscoreText.text = "Highscore: " + SaveManager.Instance.save.Highscore.ToString();
            fishcountText.text = "Fish: " + SaveManager.Instance.save.Fish.ToString();
        }
        else
        {
            Debug.LogError("❌ SaveManager hoặc save chưa được khởi tạo!");
        }

        menuUI.SetActive(true);

        if (SaveManager.Instance.save.Fish >= 300)
            Social.ReportProgress(GPGSIds.achievement_money_in_the_bank, 100.0f, null);

        AudioManager.Instance.PlayMusicWithXFade(menuLoopMusic, 0.5f);
    }

    public override void Destruct()
    {
        menuUI.SetActive(false);
    }

    public void OnPlayClick()
    {
        brain.ChangeState(GetComponent<GameStateGame>());
        GameStats.Instance.ResetSession();
        GetComponent<GameStateDeath>().EnableRevive();
    }

    public void OnShopClick()
    {
        brain.ChangeState(GetComponent<GameStateShop>());
    }

    public void OnAchievementClick()
    {
        if (GameManager.Instance.isConnectedToGooglePlayServices)
        {
            Social.ShowAchievementsUI();
        }
        else
        {
            GameManager.Instance.SignInToGooglePlayServices();
        }
    }

    public void OnLeaderboardClick()
    {
        if (GameManager.Instance.isConnectedToGooglePlayServices)
        {
            Social.ShowLeaderboardUI();
        }
        else
        {
            GameManager.Instance.SignInToGooglePlayServices();
        }
    }
}