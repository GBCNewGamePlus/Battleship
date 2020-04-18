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
    public string MatchupServer = "34.236.164.9";
    public int MatchupPort = 55443;
    public int ConnectionTimeout = 30;
    // Login/Register Screen
    public GameObject Foreplay;
    public GameObject Login;
    public InputField UsernameTxt;
    public InputField PasswordTxt;
    public Text ErrorLbl;
    public static UserData CurrentUser = null;
    public static UserData CurrentRival = null;
    public void OnRegisterBtn(){
        if(string.IsNullOrWhiteSpace(UsernameTxt.text) || string.IsNullOrWhiteSpace(PasswordTxt.text)){
            ErrorLbl.text = "Please fill in register information";
            return;
        }
        ErrorLbl.text = string.Empty;
        StartCoroutine("PostRegister");
    }
    public IEnumerator PostRegister(){
        var requestBody = new LoginRequest(){
            Username = UsernameTxt.text,
            Password = PasswordTxt.text
        };
        var postData = JsonUtility.ToJson(requestBody);
        using (UnityWebRequest www = UnityWebRequest.Put(URLBase + "BTUsrMgm", postData))
        {
            www.method = UnityWebRequest.kHttpVerbPUT;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
 
            yield return www.SendWebRequest();
 
            if (www.isNetworkError)
            {
                ErrorLbl.text = www.error;
            }
            else
            {
                ReturnLoginRegister(www.downloadHandler.text);
            }
        }

    }
    public void OnLoginBtn(){
        if(string.IsNullOrWhiteSpace(UsernameTxt.text) || string.IsNullOrWhiteSpace(PasswordTxt.text)){
            ErrorLbl.text = "Please fill in login information";
            return;
        }
        ErrorLbl.text = string.Empty;
        StartCoroutine("PostLogin");
    }
    public IEnumerator PostLogin(){

        var requestBody = new LoginRequest(){
            Username = UsernameTxt.text,
            Password = PasswordTxt.text
        };
        var postData = JsonUtility.ToJson(requestBody);
        using (UnityWebRequest www = UnityWebRequest.Put(URLBase + "BTLogin", postData))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
 
            yield return www.SendWebRequest();
 
            if (www.isNetworkError)
            {
                ErrorLbl.text = www.error;
            }
            else
            {
                ReturnLoginRegister(www.downloadHandler.text);
            }
        }
    }

    private void ReturnLoginRegister(string returnString){
        if(returnString.IndexOf("error") >=0){
            var srvError = JsonUtility.FromJson<ServerError>(returnString);
            ErrorLbl.text = srvError.error;
        }
        else{
            CurrentUser = JsonUtility.FromJson<UserData>(returnString);
            State = 1;
        }
        UsernameTxt.text = string.Empty;
        PasswordTxt.text = string.Empty;
        ErrorLbl.text = string.Empty;
        WelcomeLbl.text = 
            "Hi, " + CurrentUser.user_id + " \n\n " + 
            " Score: " + Math.Floor(CurrentUser.score) +
            " Wins: " + CurrentUser.wins + 
            " Losses: " + CurrentUser.losses;
    }

    // Waiting Screen
    public GameObject Waiting;
    public GameObject RequestMatchBtn;
    public GameObject LogoutBtn;
    public GameObject WaitingLbl;
    public UdpClient udp;
    public UdpClient udp_debug;
    public Text WelcomeLbl;
    public Text WaitingLblText;
    private bool changeState = false;
    public void OnRequestBtn(){

        WaitingLblText.text = "Waiting for opponents";
        /* Formating message to the server */
        var userMatchData = new UserScore(){
            user_id = CurrentUser.user_id,
            level = (int)Math.Floor(CurrentUser.score / 400),
            score = CurrentUser.score
        };
        string userMatchString = JsonUtility.ToJson(userMatchData);
        Byte[] sendBytes = Encoding.UTF8.GetBytes(userMatchString);

        Debug.Log("Connecting to the server");
        udp = new UdpClient();
        udp.Connect(MatchupServer, MatchupPort);
        udp.Send(sendBytes, sendBytes.Length);
        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        StartCoroutine("WaitingForConnection" );

        udp_debug = new UdpClient();
        udp_debug.Connect(MatchupServer, MatchupPort);
        Byte[] sendBytesDebug = Encoding.UTF8.GetBytes(userMatchString);
        udp_debug.Send(sendBytesDebug, sendBytes.Length);
        udp_debug.BeginReceive(new AsyncCallback(OnReceived_Debug), udp_debug);

        State = 2;
    }
    void OnReceived(IAsyncResult result){
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.UTF8.GetString(message);
        var match = JsonUtility.FromJson<GameMatch>(returnData);
        CurrentRival = new UserData(){
            user_id = match.opponent.user_id,
            score = match.opponent.score
        };
        State = 3;
        changeState = true;
    }

    IEnumerator MoveToGame(){
        WaitingLblText.text = "Your opponent is: " + CurrentRival.user_id + " \n\n Score: " + Math.Floor(CurrentRival.score);
        yield return new WaitForSeconds(5);
        State = 4;
    }

    IEnumerator WaitingForConnection(){
        yield return new WaitForSeconds(ConnectionTimeout);
        if (State == 2) {
            WaitingLblText.text = "Nobody is here :(";
            yield return new WaitForSeconds(5);
            State = 1;
        }
    }
    void OnReceived_Debug(IAsyncResult result){
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.UTF8.GetString(message);
        //udp_debug.Dispose();
    }

    public void OnLogoutBtn(){

        StartCoroutine("PostLogout");
    }
    public IEnumerator PostLogout(){

        var requestBody = new LoginRequest(){
            Username = UsernameTxt.text,
            Password = PasswordTxt.text
        };
        var postData = JsonUtility.ToJson(requestBody);
        using (UnityWebRequest www = UnityWebRequest.Get(URLBase + "BTLogout?Username=" + UIManager.CurrentUser.user_id))
        {
            www.method = UnityWebRequest.kHttpVerbGET;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
 
            yield return www.SendWebRequest();
            CurrentUser = null;
            State = 0;
            WelcomeLbl.text = "";
        }
    }

    private int State = 0;
    void Update(){
        switch (State){
            case 0: // Before Login or Register
                Foreplay.SetActive(true);
                Waiting.SetActive(false);
                Login.SetActive(true);
                break;
            case 1: // After Login/Register - before requesting a match
                Foreplay.SetActive(true);
                Waiting.SetActive(true);
                Login.SetActive(false);
                LogoutBtn.SetActive(true);
                RequestMatchBtn.SetActive(true);
                WaitingLbl.SetActive(false);
                break;
            case 2: // After Requesting a Match
                Foreplay.SetActive(true);
                Waiting.SetActive(true);
                Login.SetActive(false);
                LogoutBtn.SetActive(false);
                RequestMatchBtn.SetActive(false);
                WaitingLbl.SetActive(true);
                break;
            case 3: // After Receiving a Match
                Foreplay.SetActive(true);
                if(changeState){
                    changeState = false;
                    StartCoroutine("MoveToGame");
                }
                break;
            case 4: // Playing the game
                Foreplay.SetActive(false);
                Waiting.SetActive(false);
                Login.SetActive(false);
                break;
            case 5: // Game Results
                Foreplay.SetActive(false);
                Waiting.SetActive(false);
                Login.SetActive(false);
                break;
        }
    }

}
