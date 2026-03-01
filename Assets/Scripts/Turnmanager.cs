using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Required for Coroutines

public class Turnmanager : MonoBehaviour
{
    public string move;
    public playeranimation playerAnimation;
    public juggernaut_anim_control enemyAnimation;
    
    public Healthbar healthBar;

    void Update()
    {
        if (gamemanager.Instance.currentGameState == "Neutral")
        {
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
        yield return new WaitForSeconds(0.7f);
        enemyAnimation.TakeDamage();
        healthBar.updateHealthbar(gamemanager.Instance.EnemyHealth-=67676,676741);
        yield return new WaitForSeconds(2.5f);
        gamemanager.Instance.enterEnemyAttack();
    }
}