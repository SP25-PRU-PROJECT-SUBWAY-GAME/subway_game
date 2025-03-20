using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get { return instance; } }
    private static SaveManager instance;

    public SaveState save;
    private string baseUrl = "http://localhost:7979/users";
    private string userId;

    public Action<SaveState> OnLoad;
    public Action<SaveState> OnSave;

    private void Awake()
    {
        instance = this;
    }

    // ✅ Gán dữ liệu khi đăng nhập thành công
    public void SetSaveData(UserResponse userData)
    {
        if (userData == null)
        {
            Debug.LogError("❌ userData is NULL!");
            return;
        }

        Debug.Log($"📌 Dữ liệu API trả về: Fish={userData.fish}, HighScore={userData.highScore}");



        save = new SaveState
        {
            Highscore = userData.highScore,
            Fish = userData.fish,
            CurrentHatIndex = userData.currentHatIndex,
            UnlockedHatFlag = userData.unlockedHatFlag,
            LastSaveTime = DateTime.Now
        };

        userId = userData.id;
        Debug.Log(userId);
        Debug.Log("✅ Game data loaded from API!");
        OnLoad?.Invoke(save);
    }
    // ✅ Gửi toàn bộ `SaveState` lên server
    public void Save()
    {
        if (save == null || string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Cannot save: No user data or user ID not found.");
            return;
        }

        save.LastSaveTime = DateTime.Now;
        StartCoroutine(SaveToServer());
        OnSave?.Invoke(save);
    }

    [System.Serializable]
    public class SaveDataModel
    {
        public int fish;
        public int highScore;

        public SaveDataModel(int fish, int highScore)
        {
            this.fish = fish;
            this.highScore = highScore;
        }
    }
    IEnumerator SaveToServer()
    {
        string url = $"{baseUrl}/save/{userId}";


        SaveDataModel saveData = new SaveDataModel(save.Fish, save.Highscore);

        string jsonData = JsonUtility.ToJson(saveData);
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Game data saved to server.");
            }
            else
            {
                Debug.LogError("Failed to save data: " + request.error);
            }
        }

    }


}

