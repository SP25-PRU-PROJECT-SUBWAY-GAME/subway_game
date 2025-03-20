using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;

public class LoginController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPanel, signupPanel, forgetPasswordPanel, notificationPanel;

    [Header("Login Fields")]
    public TMP_InputField loginEmail, loginPassword;

    [Header("Signup Fields")]
    public TMP_InputField signupEmail, signupPassword, signupCPassword, signupUserName;

    [Header("Notification")]
    public TMP_Text notif_Title_Text, notif_Message_Text;

    private string baseUrl = "http://localhost:7979/users"; // Base API URL

    void Start()
    {
        OpenLoginPanel(); // Mở panel login khi start
    }

    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }

    public void OpenSignUpPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        forgetPasswordPanel.SetActive(false);
    }

    public void OpenForgetPassPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        forgetPasswordPanel.SetActive(true);
    }

    // ========================== ĐĂNG NHẬP ==========================
    public void LoginUser()
    {
        if (string.IsNullOrEmpty(loginEmail.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            ShowNotification("Error", "Fields Empty! Please Input Details In All Fields");
            return;
        }

        StartCoroutine(LoginRequest());
    }

    [System.Serializable]
    public class LoginData
    {
        public string email;
        public string password;

        public LoginData(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }
    IEnumerator LoginRequest()
    {
        string url = baseUrl + "/login";

        LoginData loginData = new(loginEmail.text, loginPassword.text);
        string jsonData = JsonUtility.ToJson(loginData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            Debug.Log("Response: " + request.downloadHandler.text);

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                UserResponse userData = JsonUtility.FromJson<UserResponse>(response);

                if (userData == null)
                {
                    Debug.LogError("❌ Failed to parse userData!");
                    yield break;
                }

                // ✅ Lưu token để sử dụng cho các request tiếp theo
                PlayerPrefs.SetString("UserToken", userData.token);


                // ✅ Lưu `userData` tạm vào PlayerPrefs để GameScene lấy lại
                PlayerPrefs.SetString("UserData", JsonUtility.ToJson(userData));

                ShowNotification("Success", "Login Successful!");
                yield return new WaitForSeconds(1f);

                // ✅ Chuyển scene sang GameScene
                SceneManager.LoadScene("Game");
            }
            else
            {
                ShowNotification("Error", "Invalid Email or Password!");
            }
        }
    }


    // ========================== ĐĂNG KÝ ==========================
    public void SignUpUser()
    {
        if (string.IsNullOrEmpty(signupEmail.text) || string.IsNullOrEmpty(signupPassword.text) ||
            string.IsNullOrEmpty(signupCPassword.text) || string.IsNullOrEmpty(signupUserName.text))
        {
            ShowNotification("Error", "Fields Empty! Please Input Details In All Fields");
            return;
        }

        if (signupPassword.text != signupCPassword.text)
        {
            ShowNotification("Error", "Passwords do not match!");
            return;
        }

        StartCoroutine(RegisterRequest());
    }

    [System.Serializable]
    public class RegisterData
    {
        public string email;
        public string password;
        public string username;

        public RegisterData(string email, string password, string username)
        {
            this.email = email;
            this.password = password;
            this.username = username;
        }
    }

    IEnumerator RegisterRequest()
    {
        string url = baseUrl + "/register";

        // ✅ Dùng class thay vì `new {}`
        RegisterData registerData = new RegisterData(signupEmail.text, signupPassword.text, signupUserName.text);
        string jsonData = JsonUtility.ToJson(registerData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            Debug.Log("Response: " + request.downloadHandler.text); // 🔥 Debug server response

            if (request.result == UnityWebRequest.Result.Success)
            {
                ShowNotification("Success", "Account Created Successfully!");
                yield return new WaitForSeconds(1f);
                OpenLoginPanel();
            }
            else
            {
                ShowNotification("Error", "Registration Failed!");
                Debug.LogError("Register Error: " + request.downloadHandler.text);
            }
        }
    }


    // ========================== THÔNG BÁO ==========================
    private void ShowNotification(string title, string message)
    {
        notif_Title_Text.text = title;
        notif_Message_Text.text = message;
        notificationPanel.SetActive(true);
    }

    public void CloseNotif_Panel()
    {
        Debug.Log("CloseNotif_Panel() được gọi");
        notificationPanel.SetActive(false);
    }
}

// ========================== CLASS PARSE JSON ==========================

[System.Serializable]
public class UserResponse
{
    public string message;
    public string token;
    public string id;
    public string name;
    public int fish;
    public int highScore;
    public int currentHatIndex;
    public int[] unlockedHatFlag; // Danh sách mũ đã mở khóa (0 hoặc 1)
}