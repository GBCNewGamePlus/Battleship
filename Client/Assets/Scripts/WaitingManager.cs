using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class WaitingManager : MonoBehaviour
{
    public string URLBase = "https://zfum6anmbl.execute-api.us-east-1.amazonaws.com/default/";
    public string MatchupServer = "34.236.164.9";
    public int MatchupPort = 55443;
    public int ConnectionTimeout = 30;
    private UdpClient udp;
    public GameObject Waiting;
    public GameObject RequestMatchBtn;
    public GameObject LogoutBtn;
    public GameObject WaitingLbl;
    public Text WelcomeLbl;
    public Text WaitingLblText;

    public void OnRequestBtn(){

        WaitingLblText.text = "Waiting for opponents";
        /* Formating message to the server */
        var userMatchData = new UserScore(){
            user_id = UIManager.CurrentUser.user_id,
            level = (int)Math.Floor(UIManager.CurrentUser.score / 400),
            score = UIManager.CurrentUser.score
        };
        string userMatchString = JsonUtility.ToJson(userMatchData);
        Byte[] sendBytes = Encoding.UTF8.GetBytes(userMatchString);

        Debug.Log("Connecting to the server");
        udp = new UdpClient();
        udp.Connect(MatchupServer, MatchupPort);
        udp.Send(sendBytes, sendBytes.Length);
        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        StartCoroutine("WaitingForConnection" );

        UIManager.State = 2;
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
        UIManager.CurrentRival = new UserData(){
            user_id = match.opponent.user_id,
            score = match.opponent.score,
            start = match.opponent.start,
            boardSet = false
        };
        UIManager.CurrentUser.start = match.you.start;
        UIManager.CurrentUser.boardSet = false;
        UIManager.State = 3;
        UIManager.ChangeState = true;
    }

    IEnumerator WaitingForConnection(){
        yield return new WaitForSeconds(ConnectionTimeout);
        if (UIManager.State == 2) {
            WaitingLblText.text = "Nobody is here :(";
            yield return new WaitForSeconds(5);
            UIManager.State = 1;
        }
    }
    public void OnLogoutBtn(){

        StartCoroutine("PostLogout");
    }
    public IEnumerator PostLogout(){

        using (UnityWebRequest www = UnityWebRequest.Get(URLBase + "BTLogout?Username=" + UIManager.CurrentUser.user_id))
        {
            www.method = UnityWebRequest.kHttpVerbGET;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
 
            yield return www.SendWebRequest();
            UIManager.CurrentUser = null;
            UIManager.State = 0;
            WelcomeLbl.text = "";
        }
    }


}
