using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LoginManager : MonoBehaviour
{
    public string URLBase = "https://zfum6anmbl.execute-api.us-east-1.amazonaws.com/default/";
    public InputField UsernameTxt;
    public InputField PasswordTxt;
    public Text ErrorLbl;
    public Text WelcomeLbl;
    public void OnRegisterBtn(){
        if(string.IsNullOrWhiteSpace(UsernameTxt.text) || string.IsNullOrWhiteSpace(PasswordTxt.text)){
            ErrorLbl.text = "Please fill in register information";
            return;
        }
        ErrorLbl.text = string.Empty;
        StartCoroutine("PostRegister");
    }
    public void SimulateLogin(){
        UsernameTxt.text = UIManager.CurrentUser.user_id;
        PasswordTxt.text = UIManager.CurrentUser.password;
        StartCoroutine("PostRegister");
    }
    public IEnumerator PostRegister(){
        var requestBody = new LoginRequest(){
            Username = UsernameTxt.text.Trim(),
            Password = PasswordTxt.text.Trim()
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
            UIManager.CurrentUser = JsonUtility.FromJson<UserData>(returnString);
            UIManager.State = 1;
            WelcomeLbl.text = 
                "Hi, " + UIManager.CurrentUser.user_id + " \n\n " + 
                " Score: " + Math.Floor(UIManager.CurrentUser.score) +
                " Wins: " + UIManager.CurrentUser.wins + 
                " Losses: " + UIManager.CurrentUser.losses;
            ErrorLbl.text = string.Empty;
        }
        UsernameTxt.text = string.Empty;
        PasswordTxt.text = string.Empty;
    }
}
