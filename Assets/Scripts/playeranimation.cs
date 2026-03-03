using UnityEngine;
using System.Collections;

public class playeranimation : MonoBehaviour
{
    public Animator anim;
    public static playeranimation Instance;
    
    [Header("Health Settings")]
    public PlayerHealthbar playerHealthbar;
    public int healthStart = 6767;
    public int damageTaken = 676;

    [Header("UI Feedback")]
    public GameObject damageTextPrefab;
    private Transform uiCanvas;

    void Awake() 
    {
        Instance = this;
    }

    void Start()
    {
        // Automatically find the Canvas for popups
        GameObject canvasObj = GameObject.Find("MainCanvas");
        if (canvasObj != null) 
        {
            uiCanvas = canvasObj.transform;
        }
        else 
        {
            Debug.LogWarning("PlayerAnimation: Could not find 'MainCanvas'. Popups won't show.");
        }
    }

    public void Attack() 
    {
        anim.CrossFade("Enemy_Attack_1_Run", 0.2f);
    }

    public void Parry()
    {
        anim.Play("OneHand_Up_Shield_Block_Hit_1_InPlace");
    }

    public void TakeDamage()
    {
        anim.CrossFade("Hit_F_1", 0.2f);
    }

    public void MoveForward()
    {
        anim.CrossFade("OneHand_Up_Run_F_InPlace", 0.2f);
    }

    public void MoveBackward()
    {
        anim.CrossFade("OneHand_Up_Run_B", 0.2f);
    }

    public void DecreaseHealth()
    {
        // 1. Set the damage amount

        // 2. Update the actual health variable in GameManager
        gamemanager.Instance.PlayerHealth -= damageTaken;

        // 3. Update the visual health bar (FIXED: removed the extra -676 here)
        if (playerHealthbar != null)
        {
            playerHealthbar.updatePlayerHealthbar(gamemanager.Instance.PlayerHealth, healthStart);
        }

        // 4. Spawn the Damage Popup
        if (damageTextPrefab != null && uiCanvas != null)
        {
            GameObject popup = Instantiate(damageTextPrefab, uiCanvas);
            popup.transform.localPosition = Vector3.zero; // Centers it on screen
            
            DamageText dt = popup.GetComponent<DamageText>();
            if (dt != null) 
            {
                dt.setDamageNumber(damageTaken);
            }
        }

        Debug.Log("Player Health: " + gamemanager.Instance.PlayerHealth);
    }
}