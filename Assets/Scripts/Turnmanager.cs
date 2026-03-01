using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Turnmanager : MonoBehaviour
{
    public gamemanager gameManager;
    public string move;
    public playeranimation playerAnimation;
    public juggernaut_anim_control enemyAnimation;

    public bool isPlayerTurn;
    public Healthbar healthBar;

    void Update()
    {
        // We only take input if the game is currently in "Neutral"
        if (gameManager.currentGameState == "Neutral")
        {
            isPlayerTurn = true;

            // --- ATTACK LOGIC ---
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                move = "sword";
                StartCoroutine(AttackSequence());
            }
            else if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                move = "spell";
                StartCoroutine(AttackSequence());
            }
            
            // --- HEAL LOGIC ---
            else if (Keyboard.current.dKey.wasPressedThisFrame && gamemanager.Instance.PlayerHealth < 6767)
            {
                if (gamemanager.Instance.PlayerHealth <= 6091) {
                    gamemanager.Instance.PlayerHealth += 676;
                } else {
                    gamemanager.Instance.PlayerHealth = 6767;
                }
                move = "heal";
                Debug.Log("Healed! Current Health: " + gamemanager.Instance.PlayerHealth);
                
                // Heal moves straight to Enemy Turn
                gamemanager.Instance.enterEnemyAttack();
            }
            
            // --- SKIP LOGIC ---
            else if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                move = "skip";
                gamemanager.Instance.enterEnemyAttack();
            }
        } 
        else
        {
            isPlayerTurn = false;
        }
    }

    IEnumerator AttackSequence()
    {
        // 1. START PLAYER MOVE
        gamemanager.Instance.enterPlayerAttack();
        
        // Wait until the player physically reaches the enemy
        while (gamemanager.Instance.movementcompleted == false)
        {
            yield return null;
        }

        // 2. EXECUTE ATTACK
        playerAnimation.Attack();
        Debug.Log("Arrived and Attacking!");

        // Wait for the sword to "land" (0.7s)
        yield return new WaitForSeconds(0.7f);
        
        enemyAnimation.TakeDamage();
        
        // Subtract health and update the UI
        gamemanager.Instance.EnemyHealth -= 67676;
        healthBar.updateHealthbar(gamemanager.Instance.EnemyHealth, 676741);

        // 3. WAIT FOR ANIMATION TO FINISH
        // Give the player time to finish their swing before moving back
        yield return new WaitForSeconds(2.0f);

        // 4. SWITCH TO TRANSITION (Moving back to start)
        Debug.Log("Starting Transition back...");
        gamemanager.Instance.enterTransition();

        // IMPORTANT: Wait for the "Move Back" to finish
        // If movementcompleted isn't reset in GameManager, this might skip instantly.
        while (gamemanager.Instance.movementcompleted == false)
        {
            yield return null;
        }

        // 5. START ENEMY ATTACK
        // Now that the player is safely back at their spot, the Juggernaut begins
        Debug.Log("Transition complete. Enemy attacking now!");
        enemyAnimation.RunForward();
        gamemanager.Instance.enterEnemyAttack();
        
        // Note: Do NOT call enterNeutral() here. 
        // The game stays in "EnemyAttack" so the Parry logic can run.
    }
}