using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class highscore : MonoBehaviour
{
    // 서버 API의 기본 URL
    private string apiBaseUrl = "http://ec2-3-39-193-163.ap-northeast-2.compute.amazonaws.com:80/";

    // UI 요소들을 연결할 변수들
    public Text nameTextInput;
    public Text scoreTextInput;
    public Text nameResultText;
    public Text scoreResultText;

    // 고스코어 가져오기 버튼 클릭 시 실행되는 메서드
    public void GetScoreBtn()
    {
        // 결과 텍스트 초기화
        nameResultText.text = "Player: \n \n";
        scoreResultText.text = "Score: \n \n";

        // 고스코어 가져오기 코루틴 실행
        StartCoroutine(GetScores());
    }

    // 고스코어 전송 버튼 클릭 시 실행되는 메서드
    public void SendScoreBtn()
    {
        // 고스코어 전송 코루틴 실행
        StartCoroutine(PostScores(nameTextInput.text, Convert.ToInt32(scoreTextInput.text)));

        // 입력 필드 초기화
        nameTextInput.gameObject.transform.parent.GetComponent<InputField>().text = "";
        scoreTextInput.gameObject.transform.parent.GetComponent<InputField>().text = "";
    }

    // 고스코어 가져오기 코루틴
    IEnumerator GetScores()
    {
        // 고스코어 가져오기 요청
        UnityWebRequest hs_get = UnityWebRequest.Get(apiBaseUrl + "highscores");
        yield return hs_get.SendWebRequest();

        // 요청이 성공했을 경우
        if (hs_get.result == UnityWebRequest.Result.Success)
        {
            // 서버에서 받은 JSON 데이터를 역직렬화하여 리스트로 변환
            string dataText = hs_get.downloadHandler.text;
            List<HighScore> highScores = JsonUtility.FromJson<HighScoreList>(dataText).highScores;

            // 리스트에 있는 고스코어 정보를 텍스트로 출력
            foreach (var score in highScores)
            {
                nameResultText.text += score.name + "\n";
                scoreResultText.text += score.score + "\n";
            }
        }
        // 요청이 실패했을 경우
        else
        {
            Debug.Log("There was an error getting the high score: " + hs_get.error);
        }
    }

    // 고스코어 전송 코루틴
    IEnumerator PostScores(string name, int score)
    {
        // 새로운 고스코어 객체 생성
        HighScore newScore = new HighScore { name = name, score = score };

        // 객체를 JSON 형태로 직렬화
        string jsonData = JsonUtility.ToJson(newScore);

        // 서버에 고스코어 전송 요청
        UnityWebRequest hs_post = UnityWebRequest.PostWwwForm(apiBaseUrl + "addscore", "POST");
        hs_post.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        hs_post.downloadHandler = new DownloadHandlerBuffer();
        hs_post.SetRequestHeader("Content-Type", "application/json");

        yield return hs_post.SendWebRequest();

        // 요청이 성공했을 경우
        if (hs_post.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Score posted successfully!");
        }
        // 요청이 실패했을 경우
        else
        {
            Debug.Log("There was an error posting the high score: " + hs_post.error);
        }
    }

    // JSON 데이터를 고스코어 객체로 역직렬화하기 위한 클래스 정의
    [Serializable]
    private class HighScore
    {
        public string name;
        public int score;
    }

    // JSON 데이터의 배열을 감싸기 위한 클래스 정의
    [Serializable]
    private class HighScoreList
    {
        public List<HighScore> highScores;
    }
}
