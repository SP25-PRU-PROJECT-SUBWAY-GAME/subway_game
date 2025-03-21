﻿using UnityEngine;
//using GooglePlayGames;
using UnityEngine.SocialPlatforms;
//using GooglePlayGames.BasicApi;

public enum GameCamera
{
    Init = 0,
    Game = 1,
    Shop = 2,
    Respawn = 3
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return instance; } }
    private static GameManager instance;

    public PlayerMotor motor;
    public WorldGeneration worldGeneration;
    public SceneChunkGeneration sceneChunkGeneration;
    public GameObject[] cameras;
    public bool isConnectedToGooglePlayServices;

    private GameState state;

    private void Awake()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
#endif
        Debug.Log(Application.persistentDataPath);
    }

    private void Start()
    {
        instance = this;

        if (SaveManager.Instance != null)
        {
            Debug.Log("✅ SaveManager đã tồn tại trong GameScene.");

            // 🔥 Kiểm tra nếu có dữ liệu user từ `LoginScene`
            if (PlayerPrefs.HasKey("UserData"))
            {
                string json = PlayerPrefs.GetString("UserData");
                UserResponse userData = JsonUtility.FromJson<UserResponse>(json);

                if (userData != null)
                {
                    Debug.Log("✅ Dữ liệu user tìm thấy, tải vào SaveManager...");
                    SaveManager.Instance.SetSaveData(userData);

                    // Xóa dữ liệu sau khi load để tránh dùng lại không cần thiết
                    PlayerPrefs.DeleteKey("UserData");
                }
                else
                {
                    Debug.LogWarning("⚠ Không thể parse UserData từ PlayerPrefs!");
                }
            }
            else
            {
                Debug.Log("ℹ Không tìm thấy dữ liệu UserData trong PlayerPrefs, load mặc định.");
            }
        }
        else
        {
            Debug.LogError("❌ SaveManager.Instance là NULL trong GameScene!");
        }
        state = GetComponent<GameStateInit>();
        state.Construct();

        SignInToGooglePlayServices();
    }

    public void SignInToGooglePlayServices()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, (result) => {
            switch (result)
            {
                case SignInStatus.Success:
                    isConnectedToGooglePlayServices = true;
                    break;
                default:
                    isConnectedToGooglePlayServices = false;
                    break;
            }
        });
#endif
    }

    private void Update()
    {
        state.UpdateState();
    }

    public void ChangeState(GameState s)
    {
        state.Destruct();
        state = s;
        state.Construct();
    }

    public void ChangeCamera(GameCamera c)
    {
        foreach (GameObject go in cameras)
            go.SetActive(false);

        cameras[(int)c].SetActive(true);
    }
}
