using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Turnmanager : MonoBehaviour
{
    public gamemanager gameManager;
    public string move;
    public playeranimation playerAnimation;
    public juggernaut_anim_control enemyAnimation;

    [Header("UI & Health")]
    public bool isPlayerTurn; 
    public Healthbar enemyHealthBar;       // For the Juggernaut
    public PlayerHealthbar playerHealthbar; // For the Player

    [Header("UDP Gesture Receiver")]
    public UdpGestureReceiverV2 gestureReceiver;
    public bool autoFindGestureReceiver = true;

    private bool actionLocked = false;
    private UdpGestureReceiverV2.HandsUpState prevHands = UdpGestureReceiverV2.HandsUpState.None;

    void Awake()
    {
        // Auto-link the gesture receiver if it exists in the scene
        if (autoFindGestureReceiver && gestureReceiver == null)
        {
#if UNITY_2023_1_OR_NEWER
            gestureReceiver = Object.FindFirstObjectByType<UdpGestureReceiverV2>();
#else
            gestureReceiver = FindObjectOfType<UdpGestureReceiverV2>();
#endif
        }
    }

    void Update()
    {
        // 1. STATE CHECK
        bool neutral = (gameManager != null && gameManager.currentGameState == "Neutral");
        isPlayerTurn = neutral;

        // If we aren't in Neutral, reset the lock and track the "last" hand position
        if (!neutral)
        {
            actionLocked = false;
            if (gestureReceiver != null) prevHands = gestureReceiver.handsUpState;
            return;
        }

        // If we are already doing a move, ignore all further input
        if (actionLocked) return;

        // 2. INPUT COLLECTION
        // Keyboard Fallback
        bool keySword = Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame;
        bool keySpell = Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame;
        bool keyHeal  = Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame;
        bool keySkip  = Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;

        // Gesture Logic
        var handsNow = gestureReceiver ? gestureReceiver.handsUpState : UdpGestureReceiverV2.HandsUpState.None;
        bool gestureSword = (handsNow == UdpGestureReceiverV2.HandsUpState.Right) && (prevHands != handsNow);
        bool gestureSpell = (handsNow == UdpGestureReceiverV2.HandsUpState.Left)  && (prevHands != handsNow);
        bool gestureHeal  = (handsNow == UdpGestureReceiverV2.HandsUpState.Both)  && (prevHands != handsNow);
        bool gestureSkip  = gestureReceiver != null && gestureReceiver.crossPressedThisFrame;

        prevHands = handsNow; // Update history for the next frame

        // 3. MOVE EXECUTION
        // --- ATTACK (SWORD/SPELL) ---
        if (keySword || gestureSword || keySpell || gestureSpell)
        {
            move = (keySword || gestureSword) ? "sword" : "spell";
            actionLocked = true;
            StartCoroutine(AttackSequence());
        }
        // --- HEAL ---
        else if ((keyHeal || gestureHeal) && gamemanager.Instance.PlayerHealth < 6767)
        {
            actionLocked = true;
            
            // Logic: Add 676 HP, but don't go over the max (6767)
            if (gamemanager.Instance.PlayerHealth <= 6091)
                gamemanager.Instance.PlayerHealth += 676;
            else
                gamemanager.Instance.PlayerHealth = 6767;

            move = "heal";
            Debug.Log("Healed! HP: " + gamemanager.Instance.PlayerHealth);

            // UI UPDATE: Update the player's health bar visually
            if (playerHealthbar != null)
                playerHealthbar.updatePlayerHealthbar(gamemanager.Instance.PlayerHealth, 6767);

            gamemanager.Instance.enterEnemyAttack();
            StartCoroutine(UnlockNextFrame());
        }
        // --- SKIP ---
        else if (keySkip || gestureSkip)
        {
            move = "skip";
            actionLocked = true;
            gamemanager.Instance.enterEnemyAttack();
            StartCoroutine(UnlockNextFrame());
        }
    }

    IEnumerator UnlockNextFrame()
    {
        yield return null;
        actionLocked = false;
    }

    IEnumerator AttackSequence()
    {
        // 1. Move to Enemy
        gamemanager.Instance.enterPlayerAttack();
        while (gamemanager.Instance.movementcompleted == false) yield return null;

        // 2. Play Animation & Wait for Impact
        playerAnimation.Attack();
        yield return new WaitForSeconds(0.7f);

        // 3. Apply Damage
        enemyAnimation.TakeDamage();
        gamemanager.Instance.EnemyHealth -= 67676;
        
        // UI UPDATE: Update the Juggernaut's health bar
        if (enemyHealthBar != null)
            enemyHealthBar.updateHealthbar(gamemanager.Instance.EnemyHealth, 676741);

        // 4. Recovery & Return
        yield return new WaitForSeconds(2.0f);
        gamemanager.Instance.enterTransition();
        while (gamemanager.Instance.movementcompleted == false) yield return null;

        // 5. Trigger Enemy Turn
        enemyAnimation.RunForward();
        gamemanager.Instance.enterEnemyAttack();

        actionLocked = false;
    }
}