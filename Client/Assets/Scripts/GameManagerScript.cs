using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    public GridScript player1Defense;
    public GridScript player1Attack;
    public GridScript player2Defense;
    public GridScript player2Attack;
    
    private string gameState;
    private string attackFeedback;

    private bool timing = false;
    public int timer = 30;
    private Text timertxt;
    void Start()
    {
        gameState = "Setup";
        attackFeedback = "";
        Debug.Log("Welcome to Battleship!");
        Debug.Log("Please place your ships, both of you.");
        timertxt = GameObject.Find("Timer").GetComponent<Text>();
        timertxt.text = timer.ToString();
    }
    
    void Update()
    {
        if (gameState == "Setup")
            Setup();
            
        
    }
    IEnumerator Timer()
    {
        timing = true;
        while (timing)
        {
            timer--;
            timertxt.text = timer.ToString();
            if (timer <= 0)
            {
                timer = 30;
                timing = false;
                gameState = "Fumble";
                StopCoroutine("Timer");
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
    private void Setup()
    {
        if (!timing)
        {
            StartCoroutine("Timer");
        }
        if (player1Defense.SetupComplete() && player2Defense.SetupComplete())
        {
            StopCoroutine("Timer");
            timertxt.text = null;
            gameState = "Player1's Turn";
            player1Attack.SetIsActive(true);
            Debug.Log("Setup complete. It's Player 1's turn to attack.");
        }
    }

    public void AttackOpponent(Vector2Int loc)
    {
        if (gameState == "Player1's Turn")
        {
            player2Defense.AttackTile(loc);
            if (player2Defense.AllShipsDestroyed())
            {
                player1Attack.ShowTileText(loc, "H");
                gameState = "Player1 Won";
                player2Attack.SetIsActive(false);
                player1Attack.SetIsActive(false);
                Debug.Log("Player1 hits and destroys the last ship of Player2.");
                Debug.Log("Player1 won the game.");
            }
            else
            {
                gameState = "Player2's Turn";
                player1Attack.SetIsActive(false);
                player2Attack.SetIsActive(true);
                if (attackFeedback == "HIT")
                {
                    player1Attack.ShowTileText(loc, "H");
                    Debug.Log("Player1 hit a ship! It's Player2's turn to attack.");
                }
                else if (attackFeedback == "MISS")
                {
                    player1Attack.ShowTileText(loc, "M");
                    Debug.Log("Player1 missed. It's Player2's turn to attack.");
                }
                else
                    Debug.Log("GameManager: ERROR");
            }
        }
        else if (gameState == "Player2's Turn")
        {
            player1Defense.AttackTile(loc);
            if (player1Defense.AllShipsDestroyed())
            {
                player2Attack.ShowTileText(loc, "H");
                gameState = "Player2 Won";
                player2Attack.SetIsActive(false);
                player1Attack.SetIsActive(false);
                Debug.Log("Player2 hits and destroys the last ship of Player1.");
                Debug.Log("Player2 won the game.");
            }
            else
            {
                gameState = "Player1's Turn";
                player2Attack.SetIsActive(false);
                player1Attack.SetIsActive(true);
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
        } 
    }

    public void GiveAttackFeedback(string msg)
    {
        attackFeedback = msg;
    }
}
