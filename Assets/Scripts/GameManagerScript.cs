using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public GridScript player1Defense;
    public GridScript player1Attack;
    public GridScript player2Defense;
    public GridScript player2Attack;
    
    private string gameState;

    void Start()
    {
        gameState = "Setup";
        Debug.Log("Welcome to Battleship!");
        Debug.Log("Please place your ships, both of you.");
    }
    
    void Update()
    {
        if (gameState == "Setup")
            Setup();
    }

    private void Setup()
    {
        if (player1Defense.SetupComplete() && player2Defense.SetupComplete())
        {
            gameState = "Player1's Turn";
            Debug.Log("Setup complete, it's Player 1's turn.");
        }
    }

    private void Player1()
    {

    }

    private void Player2()
    {

    }
}
