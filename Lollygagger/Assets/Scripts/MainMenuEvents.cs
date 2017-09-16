/*
 * Copyright (C) 2015 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Mainmenu events.
using UnityEngine.SceneManagement;

//튜토리얼때 추가된 네임스페이스
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class MainMenuEvents : MonoBehaviour
{
	public float fadeSpeed = 1.5f; 
	public Image mFader;
	bool toBlack = false;
	bool toClear = true;

    //튜토리얼때 추가된 변수
    private Text signInButtonText;
    private Text authStatus;

    private GameObject achButton; //튜토리얼: 업적 보여주는 버튼이 추가되었으므로 변수 추가
    private GameObject ldrButton; //튜토리얼: 리더보드 보여주기

    void Awake() {
        mFader.color = Color.black;
        mFader.gameObject.SetActive(true);
        toClear = true;
    }

	void Start() {

        GameObject startButton = GameObject.Find("startButton");
        EventSystem.current.firstSelectedGameObject = startButton;

        // ADD Play Game Services init code here.
        //https://codelabs.developers.google.com/codelabs/playservices_unity/index.html?index=..%2F..%2Findex#6
        //튜토리얼의 가이드에 따라 추가된 코드

        //클라이언트 설정 생성
        PlayGamesClientConfiguration config = new
            PlayGamesClientConfiguration.Builder()
            .EnableSavedGames()
            .Build();
        //게임 저장 기능 추가 : .EnableSavedGames()

        //디버깅 아웃풋 활성화
        PlayGamesPlatform.DebugLogEnabled = true;

        //초기화 및 플랫폼 활성화
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();

        // ADD THESE LINES
        // Get object instances
        signInButtonText =
            GameObject.Find("signInButton").GetComponentInChildren<Text>();
        authStatus = GameObject.Find("authStatus").GetComponent<Text>();

        achButton = GameObject.Find("achButton"); //업적 버튼 변수에 등록되도록 검색
        ldrButton = GameObject.Find("ldrButton"); //리더보드 버튼 변수에 등록되도록 검색

        //본 스크립트가 시작될때 곧바로 아래 SignIncallback 콜백함수를 이용해 로그인
        PlayGamesPlatform.Instance.Authenticate(SignInCallback, true);

    }

    public void Update()
    {
        achButton.SetActive(Social.localUser.authenticated);
        ldrButton.SetActive(Social.localUser.authenticated);
        //업적/리더보드 버튼은 GPGS의 로그인 여부에 따라 숨겨지겨나 보여지거나 할것이다.

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Application.isEditor)
                System.Diagnostics.Process.GetCurrentProcess().Kill();

            //Application.Quit은 앱크래시나는 버그가 있어 이렇게 처리
        }
    }

    void LateUpdate()
    {
        if (toBlack) {
            FadeToBlack();
        }
        else if (toClear) {
            FadeToClear();
        }
    }

	public void Play ()
	{
		Debug.Log ("Playing!!");
		toBlack = true;
		// Make sure the texture is enabled.
		mFader.gameObject.SetActive(true);
		
		// Start fading towards black.
		FadeToBlack();

		FadeController fader = gameObject.GetComponentInChildren<FadeController>();
		if (fader != null) {
			fader.FadeToLevel(()=>SceneManager.LoadScene("GameScene"));
		}
		else {
            SceneManager.LoadScene("GameScene");
		}
		

	}

	void FadeToClear ()
	{
		// Lerp the colour of the texture between itself and black.
		mFader.color = Color.Lerp(mFader.color, Color.clear, fadeSpeed * Time.deltaTime);
		// If the screen is almost black...
		if(mFader.color.a <= 0.05f) {

			toClear = false;
			mFader.gameObject.SetActive(false);
			
		}
	}

	void FadeToBlack ()
	{
		// Lerp the colour of the texture between itself and black.
		mFader.color = Color.Lerp(mFader.color, Color.black, fadeSpeed * Time.deltaTime);
		// If the screen is almost black...
		if(mFader.color.a >= 0.95f) {
			// ... reload the level.
			toBlack = false;

		}
	}

    public void SignIn() //튜토리얼에 추가된 함수
    {
        Debug.Log("signInButton clicked!");
        //SignIn버튼이 눌렸는지 확인. 해당버튼의 OnClick delegate에 추가됨.

        //연결되어 있지 않으면 GPGS에 연결
        if (!PlayGamesPlatform.Instance.localUser.authenticated)
        {
            // Sign in with Play Game Services, showing the consent dialog
            // by setting the second parameter to isSilent=false.
            PlayGamesPlatform.Instance.Authenticate(SignInCallback, false);

        }
        else
        {
            //이미 연결되어잇으면 로그아웃
            // Sign out of play games
            PlayGamesPlatform.Instance.SignOut();

            // Reset UI
            signInButtonText.text = "Sign In";
            authStatus.text = "";
        }
    }

    public void SignInCallback(bool success) //튜토리얼에 추가된 함수
    {
        if (success)
        {
            Debug.Log("(Lollygagger) Signed in!");

            // Change sign-in button text
            signInButtonText.text = "Sign out";

            // Show the user's name
            authStatus.text = "Signed in as: " + Social.localUser.userName;
        }
        else
        {
            Debug.Log("(Lollygagger) Sign-in failed...");

            // Show failure message
            signInButtonText.text = "Sign in";
            authStatus.text = "Sign-in failed";
        }
    }

    //튜토리얼: 업적보여주기
    public void ShowAchievements()
    {
        Debug.Log("ShowAchievements Button Clicked");

        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            Debug.Log("User is authenticated. Will show achievements");
            PlayGamesPlatform.Instance.ShowAchievementsUI();
        }
        else
        {
            Debug.Log("Cannot show Achievements, not logged in");
        }
    }

    //튜토리얼: 리더보드 보여주기
    public void ShowLeaderboards()
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            PlayGamesPlatform.Instance.ShowLeaderboardUI();
        }
        else
        {
            Debug.Log("Cannot show leaderboard: not authenticated");
        }
    }


}
