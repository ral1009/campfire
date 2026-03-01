using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Required for Coroutines

public class Turnmanager : MonoBehaviour
{
    public string move;
    public playeranimation playerAnimation;

    void Update()
    {
        // Only allow input during the Neutral state
        if (gamemanager.Instance.currentGameState == "Neutral")
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                move = "sword";
                // Start a Coroutine so we can "wait" for the movement to finish
                StartCoroutine(AttackSequence());
            }
            else if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                move = "spell";
                StartCoroutine(AttackSequence());
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
                gamemanager.Instance.enterEnemyAttack();}
                        else if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                move = "skip";
                gamemanager.Instance.enterEnemyAttack();
            }
        }
    }

    IEnumerator AttackSequence()
    {
        gamemanager.Instance.enterPlayerAttack();

        while (gamemanager.Instance.movementcompleted == false)
        {
            yield return null;
        }
        playerAnimation.Attack();
        Debug.Log("Arrived and Attacking!");
        gamemanager.Instance.enterEnemyAttack();
    }
}