using UnityEngine;
using UnityEngine.InputSystem;

public class Turnmanager : MonoBehaviour
{
    public gamemanager gameManager;
    public string move;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gamemanager.Instance.currentGameState == "Neutral")
    {
        if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                gamemanager.Instance.enterPlayerAttack();
                move = "sword";
            }
        else if (Keyboard.current.dKey.wasPressedThisFrame && gamemanager.Instance.PlayerHealth < 6767)
            {
                if (gamemanager.Instance.PlayerHealth <= 6091){
                gamemanager.Instance.PlayerHealth += 676;}
                else{
                    gamemanager.Instance.PlayerHealth = 6767;
                }
                move = "heal";
                Debug.Log(gamemanager.Instance.PlayerHealth);
                gamemanager.Instance.enterEnemyAttack();
            }
        else if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                gamemanager.Instance.enterPlayerAttack();
                move = "spell";
            }
        else if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                move = "skip";
                gamemanager.Instance.enterEnemyAttack();
            }
        Debug.Log(move);
    }
    }
}
