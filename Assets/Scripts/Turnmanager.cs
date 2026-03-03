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
    public Healthbar enemyHealthBar;       
    public PlayerHealthbar playerHealthbar; 
    public int damageDealt = 67676;
    public int HealAmount = 676;
    public int enemymaxhealth = 676741;

    [Header("Damage Popups")]
    public GameObject damageTextPrefab;
    private Transform uiCanvas;

    [Header("UDP Gesture Receiver")]
    public UdpGestureReceiverV2 gestureReceiver;
    public bool autoFindGestureReceiver = true;

    private bool actionLocked = false;
    private UdpGestureReceiverV2.HandsUpState prevHands = UdpGestureReceiverV2.HandsUpState.None;

    void Awake()
    {
        // Auto-link the gesture receiver
        if (autoFindGestureReceiver && gestureReceiver == null)
        {
#if UNITY_2023_1_OR_NEWER
            gestureReceiver = Object.FindFirstObjectByType<UdpGestureReceiverV2>();
#else
            gestureReceiver = FindObjectOfType<UdpGestureReceiverV2>();
#endif
        }
    }

    void Start()
    {
        // Find the Canvas for the damage popups
        GameObject canvasObj = GameObject.Find("MainCanvas");
        if (canvasObj != null) uiCanvas = canvasObj.transform;
    }

    void Update()
    {
        // 1. STATE CHECK
        bool neutral = (gameManager != null && gameManager.currentGameState == "Neutral");
        isPlayerTurn = neutral;

        if (!neutral)
        {
            actionLocked = false;
            if (gestureReceiver != null) prevHands = gestureReceiver.handsUpState;
            return;
        }

        if (actionLocked) return;

        // 2. INPUT COLLECTION
        bool keySword = Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame;
        bool keySpell = Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame;
        bool keyHeal  = Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame;
        bool keySkip  = Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;

        var handsNow = gestureReceiver ? gestureReceiver.handsUpState : UdpGestureReceiverV2.HandsUpState.None;
        bool gestureSword = (handsNow == UdpGestureReceiverV2.HandsUpState.Right) && (prevHands != handsNow);
        bool gestureSpell = (handsNow == UdpGestureReceiverV2.HandsUpState.Left)  && (prevHands != handsNow);
        bool gestureHeal  = (handsNow == UdpGestureReceiverV2.HandsUpState.Both)  && (prevHands != handsNow);
        bool gestureSkip  = gestureReceiver != null && gestureReceiver.crossPressedThisFrame;

        prevHands = handsNow;

        // 3. MOVE EXECUTION
        if (keySword || gestureSword || keySpell || gestureSpell)
        {
            move = (keySword || gestureSword) ? "sword" : "spell";
            actionLocked = true;
            StartCoroutine(AttackSequence());
        }
        else if ((keyHeal || gestureHeal) && gamemanager.Instance.PlayerHealth < playerAnimation.healthStart)
        {
            actionLocked = true;
            gamemanager.Instance.PlayerHealth = Mathf.Min(gamemanager.Instance.PlayerHealth + HealAmount, playerAnimation.healthStart);

            // Update Player UI
            if (playerHealthbar != null)
                playerHealthbar.updatePlayerHealthbar(gamemanager.Instance.PlayerHealth, playerAnimation.healthStart);

            gamemanager.Instance.enterEnemyAttack();
            StartCoroutine(UnlockNextFrame());
        }
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

        // 3. APPLY DAMAGE & UI POPUP
        enemyAnimation.TakeDamage();
        gamemanager.Instance.EnemyHealth -= damageDealt;
        
        // Update Enemy UI Bar
        if (enemyHealthBar != null)
            enemyHealthBar.updateHealthbar(gamemanager.Instance.EnemyHealth, enemymaxhealth);

        // SPAWN POPUP (Missing in previous merge)
        if (damageTextPrefab != null && uiCanvas != null)
        {
            GameObject popup = Instantiate(damageTextPrefab, uiCanvas);
            popup.transform.localPosition = Vector3.zero;
            
            DamageText dtScript = popup.GetComponent<DamageText>();
            if (dtScript != null) dtScript.setDamageNumber(damageDealt);
        }

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