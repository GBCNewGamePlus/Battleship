using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

using UnityEngine;
using UnityEngine.Networking;

public class GameManagerScript : MonoBehaviour
{
    public string URLBase = "https://zfum6anmbl.execute-api.us-east-1.amazonaws.com/default/";
    public string GameServer = "34.236.164.9";
    public int GamePort = 44332;
    public int ConnectionTimeout = 30;
    private UdpClient udp;
    public const int totalNumberShips = 5;
    public GridScript defense;
    public GridScript attack;
    private string attackedCoords;
    public string StatusText;
    private string gameState = "Pregame";
    private ReturnAttack latestAttackMessage;
    /*
     Pregame    - Shows login and join UI
     Setup      - Player has to setup the board
     SetupWait  - Player waiting for Opponent to setup board
     Turn       - It's the player's turn to attack
     Wait       - It's the opponent's turn to attack
     Win        - Player won
     Lost       - Player lost
     */
    private string attackFeedback;

    private Vector2Int FromCoord(string coord){
        var returningValue = new Vector2Int();
        Debug.Log("Before Transformation: " + coord);
        coord = coord.Substring(1,coord.Length-2);
        Debug.Log("After Transformation: " + coord);
        var eachC = coord.Split(',');
        returningValue.x = int.Parse(eachC[0]);
        returningValue.y = int.Parse(eachC[1]);
        return returningValue;
    }
    void Update()
    {
        if (gameState.StartsWith("Setup")){
            if(gameState == "Setup"){
                var missingQtd = totalNumberShips - defense.shipCount;
                StatusText = "Still need to deploy " + missingQtd.ToString() + " ship"+ (missingQtd > 1? "s" : ""); 
            }
            Setup();
        }
        if(latestAttackMessage != null){
            var currentAM = latestAttackMessage;
            latestAttackMessage = null;
            Vector2Int loc = FromCoord(currentAM.coordinates);
            Debug.Log("Dealing with the latest attack message");
            if(gameState == "Turn"){
                Debug.Log("Turn");
                if(currentAM.win == "true"){
                    Debug.Log("It's a win!");
                    attack.ShowTileText(loc, "H");
                    gameState = "Win";
                    attack.SetIsActive(false);
                    StatusText="You won it!";
                    UIManager.State = 5;
                }
                else{
                    if(currentAM.hit == "true"){
                        Debug.Log("You hit something");
                        attack.ShowTileText(loc, "H");
                    }
                    else{
                        Debug.Log("You hit water");
                        attack.ShowTileText(loc, "M");
                    }
                    Debug.Log("Now it's your opponent's turn");
                    gameState = "Wait";
                    StatusText = "It's opponent's time to attack";
                    attack.SetIsActive(false);
                }
            }
            else if(gameState =="Wait"){
                Debug.Log("Wait");
                defense.AttackTile(loc);
                if(currentAM.win == "true"){
                    Debug.Log("you lost the game!");
                    gameState = "Lost";
                    attack.SetIsActive(false);
                    StatusText = "You Lost the game!";
                    UIManager.State = 5;
                }
                else{
                    Debug.Log("Now it's your time to attack");
                    gameState = "Turn";
                    StatusText = "It's your time to attack";
                    attack.SetIsActive(true);
                }
                
            }
        }

    }
    private void Setup()
    {
        if (defense.SetupComplete())
        {
            SendTableReady();
            UIManager.CurrentUser.boardSet = true;
            if(UIManager.CurrentRival.boardSet)
            {
                Debug.Log("The rival has their table setup");
                FirstRound();
            }
            else{
                Debug.Log("We have to wait for the rival");
                gameState = "SetupWait";
                StatusText = "Waiting for opponent to setup table";
            }

        }
    }

    private void FirstRound(){
        if(UIManager.CurrentUser.start == "true"){
            gameState = "Turn";
            attack.SetIsActive(true);
            StatusText = "It's your time to attack";
        }
        else{
            gameState = "Wait";
            StatusText = "It's opponent's time to attack";
        }
    }
    public void StartGame(){
        gameState = "Setup";
        attackFeedback = "";
        attackedCoords = "";
        latestAttackMessage = null;
        defense.StartGame(totalNumberShips);
        attack.StartGame(totalNumberShips);
        // Establishes connection with the server
        // and sends first message
        Debug.Log("Connecting to the Gameplay server");        
        udp = new UdpClient();
        udp.Connect(GameServer, GamePort);
        udp.BeginReceive(new AsyncCallback(OnReceived), udp);
        SendHello();
    }
    private void SendHello(){
        var userGameData = new GameCode(){
            cmd = "new",
            user_id = UIManager.CurrentUser.user_id,
            game_id = UIManager.GameId
        };
        string userGameString = JsonUtility.ToJson(userGameData);
        Byte[] sendBytes = Encoding.UTF8.GetBytes(userGameString);
        Debug.Log("Sending Hello!!");
        udp.Send(sendBytes, sendBytes.Length);
    }

    void OnReceived(IAsyncResult result){
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);
        Debug.Log("***********************************");        
        Debug.Log(returnData);        
        var latestMessage = JsonUtility.FromJson<GameCommand>(returnData);
        try{
            switch(latestMessage.cmd){
                case "table":
                    Debug.Log("Setting Rival board value");
                    UIManager.CurrentRival.boardSet = true;
                    break;
                case "attack":
                    Debug.Log("Receving Attack Data");
                    latestAttackMessage = JsonUtility.FromJson<ReturnAttack>(returnData);
                    break;
                /*
                case commands.OTHERS:
                    lock(lockSpawn){
                        spawnMessages.Enqueue(latestMessage);
                    }
                    break;
                case commands.DELETE:
                    lock(lockDelete){
                        deleteMessages.Enqueue(latestMessage);
                    }
                    break;
                default:
                    Debug.Log("Error - no suitable message found!!!!!");
                    break;*/
            }
        }
        catch (Exception e){
            Debug.Log(e.ToString());
        }
       // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    private void SendTableReady(){
        var userTableData = new TableReady(){
            cmd = "table",
            user_id = UIManager.CurrentUser.user_id,
            game_id = UIManager.GameId,
            intact_cells = defense.OccupiedNumber,
            occupied_cells = defense.OccupiedCells
        };
        string userTableString = JsonUtility.ToJson(userTableData);
        Byte[] sendBytes = Encoding.UTF8.GetBytes(userTableString);
        Debug.Log("Sending TableReady!!");
        udp.Send(sendBytes, sendBytes.Length);
    }

    public void AttackOpponent(Vector2Int loc)
    {
        string coordText =  "[" + loc.x + "," + loc.y +"]";
        if(attackedCoords.IndexOf(coordText) >=0){
            StatusText = "Select another coord, please!";
            return;
        }
        var attackData = new GameAttack(){
            cmd = "attack",
            game_id = UIManager.GameId,
            user_id = UIManager.CurrentUser.user_id,
            coordinates = "[" + loc.x + "," + loc.y +"]"
        };
        var attackDataString = JsonUtility.ToJson(attackData);
        Byte[] sendBytes = Encoding.UTF8.GetBytes(attackDataString);
        Debug.Log("Sending Attack!!");
        udp.Send(sendBytes, sendBytes.Length);

        /*
        if (gameState == "Player1's Turn")
        {
            player2Defense.AttackTile(loc);
            if (player2Defense.AllShipsDestroyed())
            {
                attack.ShowTileText(loc, "H");
                gameState = "Player1 Won";
                player2Attack.SetIsActive(false);
                attack.SetIsActive(false);
                Debug.Log("Player1 hits and destroys the last ship of Player2.");
                Debug.Log("Player1 won the game.");
            }
            else
            {
                gameState = "Player2's Turn";
                attack.SetIsActive(false);
                player2Attack.SetIsActive(true);
                if (attackFeedback == "HIT")
                {
                    attack.ShowTileText(loc, "H");
                    Debug.Log("Player1 hit a ship! It's Player2's turn to attack.");
                }
                else if (attackFeedback == "MISS")
                {
                    attack.ShowTileText(loc, "M");
                    Debug.Log("Player1 missed. It's Player2's turn to attack.");
                }
                else
                    Debug.Log("GameManager: ERROR");
            }
        }
        else if (gameState == "Player2's Turn")
        {
            defense.AttackTile(loc);
            if (defense.AllShipsDestroyed())
            {
                player2Attack.ShowTileText(loc, "H");
                gameState = "Player2 Won";
                player2Attack.SetIsActive(false);
                attack.SetIsActive(false);
                Debug.Log("Player2 hits and destroys the last ship of Player1.");
                Debug.Log("Player2 won the game.");
            }
            else
            {
                gameState = "Player1's Turn";
                player2Attack.SetIsActive(false);
                attack.SetIsActive(true);
                if (attackFeedback == "HIT")
                {
                    Debug.Log("Player2 hit a ship! It's Player1's turn to attack.");
                    player2Attack.ShowTileText(loc, "H");
                }
                else if (attackFeedback == "MISS")
                {
                    Debug.Log("Player2 missed. It's Player1's turn to attack.");
                    player2Attack.ShowTileText(loc, "M");
                }
                else
                    Debug.Log("GameManager: ERROR");
            }
        } */
    }

    public void GiveAttackFeedback(string msg)
    {
        //attackFeedback = msg;
    }
}
