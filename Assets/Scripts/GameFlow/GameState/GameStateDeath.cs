//using TMPro;
//using UnityEngine;
//using UnityEngine.Advertisements;
//using UnityEngine.UI;

//public class GameStateDeath : GameState //IUnityAdsListener
//{
//    public GameObject deathUI;
//    [SerializeField] private TextMeshProUGUI highscore;
//    [SerializeField] private TextMeshProUGUI currentScore;
//    [SerializeField] private TextMeshProUGUI fishTotal;
//    [SerializeField] private TextMeshProUGUI currentFish;

//    // Completion circle fields
//    [SerializeField] private Image completionCircle;
//    public float timeToDecision = 2.5f;
//    private float deathTime;

//    private void OnEnable()
//    {
//        // Advertisement.AddListener(this);
//    }

//    private void OnDisable()
//    {
//        // Advertisement.RemoveListener(this);
//    }

//    public override void Construct()
//    {
//        base.Construct();
//        GameManager.Instance.motor.PausePlayer();

//        deathTime = Time.time;
//        deathUI.SetActive(true);

//        // Prior to saving, set the highscore if needed
//        if (SaveManager.Instance.save.Highscore < (int)GameStats.Instance.score)
//        {
//            SaveManager.Instance.save.Highscore = (int)GameStats.Instance.score;
//            currentScore.color = Color.green;

//            if (GameManager.Instance.isConnectedToGooglePlayServices)
//            {
//                Debug.Log("Reporting score..");
//                Social.ReportScore(SaveManager.Instance.save.Highscore, GPGSIds.leaderboard_top_score, (success) =>
//                {
//                    if (!success) Debug.LogError("Unable to post highscore");
//                });

//                Social.ReportProgress(GPGSIds.achievement_joining_the_ladder, 100.0f, null);
//            }
//            else
//            {
//                Debug.Log("Not signed in.. unable to report score");
//            }
//        }
//        else
//        {
//            currentScore.color = Color.white;
//        }

//        SaveManager.Instance.save.Fish += GameStats.Instance.fishCollectedThisSession;
//        SaveManager.Instance.Save();

//        highscore.text = "Highscore :  " + SaveManager.Instance.save.Highscore;
//        currentScore.text = GameStats.Instance.ScoreToText();
//        fishTotal.text = "Total fish :" + SaveManager.Instance.save.Fish;
//        currentFish.text = GameStats.Instance.FishToText();
//    }

//    public override void Destruct()
//    {
//        deathUI.SetActive(false);
//    }

//    public override void UpdateState()
//    {
//        float ratio = (Time.time - deathTime) / timeToDecision;
//        completionCircle.color = Color.Lerp(Color.green, Color.red, ratio);
//        completionCircle.fillAmount = 1 - ratio;

//        if (ratio > 1)
//        {
//            completionCircle.gameObject.SetActive(false);
//        }
//    }

//    public void TryResumeGame()
//    {
//        AdManager.Instance.ShowRewardedAd();
//    }

//    public void ResumeGame()
//    {
//        brain.ChangeState(GetComponent<GameStateGame>());
//        GameManager.Instance.motor.RespawnPlayer();
//    }

//    public void ToMenu()
//    {
//        brain.ChangeState(GetComponent<GameStateInit>());

//        GameManager.Instance.motor.ResetPlayer();
//        GameManager.Instance.worldGeneration.ResetWorld();
//        GameManager.Instance.sceneChunkGeneration.ResetWorld();
//    }

//    public void EnableRevive()
//    {
//        completionCircle.gameObject.SetActive(true);
//    }

//    public void OnUnityAdsReady(string placementId)
//    {

//    }

//    public void OnUnityAdsDidError(string message)
//    {
//        Debug.Log(message);
//    }

//    public void OnUnityAdsDidStart(string placementId)
//    {

//    }

//    //public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
//    //{
//    //    completionCircle.gameObject.SetActive(false);
//    //    switch (showResult)
//    //    {
//    //        case ShowResult.Failed:
//    //            ToMenu();
//    //            break;
//    //        case ShowResult.Finished:
//    //            ResumeGame();
//    //            break;
//    //        default:
//    //            break;
//    //    }
//    //}
//}

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class GameStateDeath : GameState
{
    [Header("UI Elements")]
    public GameObject deathUI;  // Màn hình chết (Canvas_Death)
    [SerializeField] private GameObject[] gameUIElements; // Các UI cần ẩn khi hiển thị quiz
    [SerializeField] private TextMeshProUGUI highscore;
    [SerializeField] private TextMeshProUGUI currentScore;
    [SerializeField] private TextMeshProUGUI fishTotal;
    [SerializeField] private TextMeshProUGUI currentFish;

    [Header("Question Panel")]
    [SerializeField] private GameObject questionPanel; // Panel câu hỏi
    [SerializeField] private TMP_Text questionText; // Hiển thị câu hỏi
    [SerializeField] private Button[] answerButtons; // Các nút đáp án
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    private string correctAnswerId;

    [Header("Revive Timer")]
    [SerializeField] private Image completionCircle;
    public float timeToDecision = 2.5f;
    private float deathTime;


    private Dictionary<Button, string> btnAnswerIdMapping = new Dictionary<Button, string>();


    public override void Destruct()
    {
        deathUI.SetActive(false);
    }



    public override void Construct()
    {
        base.Construct();
        GameManager.Instance.motor.PausePlayer();

        deathTime = Time.time;
        deathUI.SetActive(true);
        ShowGameUI();
        questionPanel.SetActive(false);
        ResetAnswerButtons();

        // Prior to saving, set the highscore if needed
        if (SaveManager.Instance.save.Highscore < (int)GameStats.Instance.score)
        {
            SaveManager.Instance.save.Highscore = (int)GameStats.Instance.score;
            currentScore.color = Color.green;

            if (GameManager.Instance.isConnectedToGooglePlayServices)
            {
                Debug.Log("Reporting score..");
                Social.ReportScore(SaveManager.Instance.save.Highscore, GPGSIds.leaderboard_top_score, (success) =>
                {
                    if (!success) Debug.LogError("Unable to post highscore");
                });

                Social.ReportProgress(GPGSIds.achievement_joining_the_ladder, 100.0f, null);
            }
            else
            {
                Debug.Log("Not signed in.. unable to report score");
            }
        }
        else
        {
            currentScore.color = Color.white;
        }

        SaveManager.Instance.save.Fish += GameStats.Instance.fishCollectedThisSession;
        Debug.Log($"📥 Trước khi lưu: Fish = {SaveManager.Instance.save.Fish}, HighScore = {SaveManager.Instance.save.Highscore}");
        SaveManager.Instance.Save();

        highscore.text = "Highscore :  " + SaveManager.Instance.save.Highscore;
        currentScore.text = GameStats.Instance.ScoreToText();
        fishTotal.text = "Total fish :" + SaveManager.Instance.save.Fish;
        currentFish.text = GameStats.Instance.FishToText();
    }

    public override void UpdateState()
    {
        float ratio = (Time.time - deathTime) / timeToDecision;
        completionCircle.color = Color.Lerp(Color.green, Color.red, ratio);
        completionCircle.fillAmount = 1 - ratio;
        if (ratio > 1) completionCircle.gameObject.SetActive(false);
    }

    public void TryResumeGame()
    {
        HideGameUI();
        StartCoroutine(FetchQuestion());
    }
    void HideGameUI()
    {
        foreach (GameObject ui in gameUIElements)
        {
            ui.SetActive(false);
        }
    }
    IEnumerator FetchQuestion()
    {
        string url = "http://localhost:7979/question/random";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            QuestionResponse questionData = JsonUtility.FromJson<QuestionResponse>(jsonResponse);

            if (questionData?.question == null || questionData.question.answer == null)
            {
                Debug.LogError("❌ Lỗi: Dữ liệu từ API bị null!");
                yield break;
            }

            // 🔥 Reset lại Dictionary
            btnAnswerIdMapping.Clear();

            // 🔥 Reset UI trước khi hiển thị câu hỏi mới
            ResetAnswerButtons();

            // Cập nhật câu hỏi
            questionText.text = questionData.question.title;
            correctAnswerId = "";

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < questionData.question.answer.Length)
                {
                    answerButtons[i].gameObject.SetActive(true);
                    Text buttonText = answerButtons[i].GetComponentInChildren<Text>();
                    buttonText.text = questionData.question.answer[i].content;
                    answerButtons[i].interactable = true;

                    // 🔥 Lưu ID của câu trả lời vào dictionary
                    btnAnswerIdMapping[answerButtons[i]] = questionData.question.answer[i].id;

                    if (questionData.question.answer[i].isCorrect)
                        correctAnswerId = questionData.question.answer[i].id;

                    int index = i;
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].onClick.AddListener(() => CheckAnswer(questionData.question.answer[index].id, answerButtons[index]));
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }

            questionPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ Lỗi khi gọi API: " + request.error);
        }
    }


    void CheckAnswer(string selectedAnswerId, Button selectedButton)
    {
        bool isCorrect = selectedAnswerId == correctAnswerId;

        if (isCorrect)
        {
            selectedButton.GetComponent<Image>().color = correctColor;
            StartCoroutine(ResumeAfterDelay());
        }
        else
        {
            foreach (var btn in answerButtons)
            {
                Text btnText = btn.GetComponentInChildren<Text>();

                // 🔥 Nếu là nút người dùng chọn sai ➝ Màu đỏ
                if (btn == selectedButton)
                {
                    btn.GetComponent<Image>().color = wrongColor;
                }
                // 🔥 Nếu là câu trả lời đúng ➝ Màu xanh
                else if (btnText.text == GetCorrectAnswerText())
                {
                    btn.GetComponent<Image>().color = correctColor;
                }
            }

            StartCoroutine(ReturnToMenu());
        }
    }

    // ✅ Hàm lấy nội dung của câu trả lời đúng
    private string GetCorrectAnswerText()
    {
        foreach (var btn in answerButtons)
        {
            if (btnAnswerIdMapping.ContainsKey(btn) && btnAnswerIdMapping[btn] == correctAnswerId)
            {
                return btn.GetComponentInChildren<Text>().text;
            }
        }
        return "";
    }


    // ✅ Reset UI trước khi hiển thị câu hỏi mới
    private void ResetAnswerButtons()
    {
        foreach (var button in answerButtons)
        {
            button.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f);
            button.interactable = true;
            button.onClick.RemoveAllListeners();
        }
    }

    IEnumerator ResumeAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        ResetAnswerButtons();
        questionPanel.SetActive(false);
        deathUI.SetActive(false);
        ResumeGame();
    }

    IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(2f);
        ResetAnswerButtons();
        questionPanel.SetActive(false);
        deathUI.SetActive(false);
        ToMenu();
    }


    public void ResumeGame()
    {
        GameStats.Instance.fishCollectedThisSession = 0;
        brain.ChangeState(GetComponent<GameStateGame>());
        GameManager.Instance.motor.RespawnPlayer();
    }

    public void ToMenu()
    {
        brain.ChangeState(GetComponent<GameStateInit>());
        GameManager.Instance.motor.ResetPlayer();
        GameManager.Instance.worldGeneration.ResetWorld();
        GameManager.Instance.sceneChunkGeneration.ResetWorld();
    }

    public void EnableRevive()
    {
        completionCircle.gameObject.SetActive(true);
    }

    void ShowGameUI()
    {
        foreach (GameObject ui in gameUIElements)
        {
            ui.SetActive(true);
        }
    }

}

// ========================== CLASS PARSE JSON ==========================

[System.Serializable]
public class QuestionResponse
{
    public string message;
    public Question question;
}

[System.Serializable]
public class Question
{
    public string id;
    public string title;
    public Answer[] answer;
}

[System.Serializable]
public class Answer
{
    public string id;
    public string content;
    public bool isCorrect;
}
