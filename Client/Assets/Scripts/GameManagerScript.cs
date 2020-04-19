using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public const int totalNumberShips = 5;
    public GridScript defense;
    public GridScript attack;
    public string StatusText;
    
    private string gameState = "Pregame";
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

    void Update()
    {
        if (gameState == "Setup"){
            var missingQtd = totalNumberShips - defense.shipCount;
            StatusText = "Still need to deploy " + missingQtd.ToString() + " ship"+ (missingQtd > 1? "s" : ""); 
            Setup();
        }
    }
    public void StartGame(){
        gameState = "Setup";
        attackFeedback = "";
        defense.StartGame(totalNumberShips);
        attack.StartGame(totalNumberShips);
    }
    private void Setup()
    {
        if (defense.SetupComplete())
        {
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
    }

    public void AttackOpponent(Vector2Int loc)
    {
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
