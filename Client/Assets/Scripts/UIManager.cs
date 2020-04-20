using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour
{
    public string URLBase = "https://zfum6anmbl.execute-api.us-east-1.amazonaws.com/default/";
    // Login/Register Screen
    public GameObject Foreplay;
    public GameObject Login;
    public static UserData CurrentUser = null;
    public static UserData CurrentRival = null;
    public static string GameId;
    // Waiting Screen
    public GameObject Waiting;
    public GameObject RequestMatchBtn;
    public GameObject LogoutBtn;
    public GameObject WaitingLbl;
    public Text WaitingLblText;
    public static int State = 0;
    public static bool ChangeState = false;

    // Gaming Screen
    public GameObject PlayHUD;
    public GameObject Win;
    public GameObject Lose;
    public Text TimerLbl;
    public Text PlayerLbl;
    public Text OpponentLbl;

    public Text WelcomeLbl;

    public LoginManager loginManager;

    public void OnGoAgainBtn(){
        CurrentRival = null;
        StartCoroutine("RefreshUserData");
    }

    public IEnumerator RefreshUserData(){
        using (UnityWebRequest www = UnityWebRequest.Get(URLBase + "BTUsrMgm?Username=" + UIManager.CurrentUser.user_id))
        {
            www.method = UnityWebRequest.kHttpVerbGET;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
 
            yield return www.SendWebRequest();
            if (!www.isNetworkError)
            {
                var playerData = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
                CurrentUser = playerData;
                State = 1;
                WelcomeLbl.text = 
                    "Hi, " + UIManager.CurrentUser.user_id + " \n\n " + 
                    " Score: " + Math.Floor(UIManager.CurrentUser.score) +
                    " Wins: " + UIManager.CurrentUser.wins + 
                    " Losses: " + UIManager.CurrentUser.losses;
            }
 
        }

    }

    public GameManagerScript gameManager;

    IEnumerator MoveToGame(){
        WaitingLblText.text = "Your opponent is: " + CurrentRival.user_id + " \n\n Score: " + Math.Floor(CurrentRival.score);
        yield return new WaitForSeconds(5);
        State = 4;
        gameManager.StartGame();
        TimerLbl.text = "Setup time!";
        PlayerLbl.text = CurrentUser.user_id;
        OpponentLbl.text = CurrentRival.user_id; 
    }

    void Update(){
        switch (State){
            case 0: // Before Login or Register
                PlayHUD.SetActive(false);
                Foreplay.SetActive(true);
                Waiting.SetActive(false);
                Login.SetActive(true);
                break;
            case 1: // After Login/Register - before requesting a match
                PlayHUD.SetActive(false);
                Foreplay.SetActive(true);
                Waiting.SetActive(true);
                Login.SetActive(false);
                LogoutBtn.SetActive(true);
                RequestMatchBtn.SetActive(true);
                WaitingLbl.SetActive(false);
                break;
            case 2: // After Requesting a Match
                PlayHUD.SetActive(false);
                Foreplay.SetActive(true);
                Waiting.SetActive(true);
                Login.SetActive(false);
                LogoutBtn.SetActive(false);
                RequestMatchBtn.SetActive(false);
                WaitingLbl.SetActive(true);
                break;
            case 3: // After Receiving a Match
                PlayHUD.SetActive(false);
                Foreplay.SetActive(true);
                if(ChangeState){
                    ChangeState = false;
                    StartCoroutine("MoveToGame");
                }
                break;
            case 4: // Playing the game
                Win.SetActive(false);
                Lose.SetActive(false);
                PlayHUD.SetActive(true);
                Foreplay.SetActive(false);
                Waiting.SetActive(false);
                Login.SetActive(false);
                TimerLbl.text = gameManager.StatusText;
                break;
            case 5: // Game Results
                PlayHUD.SetActive(true);
                Foreplay.SetActive(false);
                Waiting.SetActive(false);
                Login.SetActive(false);
                if(gameManager.gameState == "Win"){
                    Win.SetActive(true);
                }
                else{
                    Lose.SetActive(true);
                }
                break;
        }
    }

}
