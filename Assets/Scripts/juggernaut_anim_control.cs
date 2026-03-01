using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class juggernaut_anim_control : MonoBehaviour
{
    private Animator anim;
    public GameObject weaponHitbox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        if(weaponHitbox != null) weaponHitbox.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            anim.Play("OneHand_Up_Attack_A_1_InPlace");
        } else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            anim.Play("OneHand_Up_Attack_A_2_InPlace");
        } else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
             anim.Play("OneHand_Up_Attack_A_3_InPlace");
        }   else if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            anim.Play("OneHand_Up_Attack_B_1_InPlace");
        } else if (Keyboard.current.digit5Key.wasPressedThisFrame){
            anim.Play("OneHand_Up_Attack_B_2_InPlace");
        } else if (Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            anim.Play("OneHand_Up_Attack_B_3_InPlace");
        }}
    public void ActivateHitbox()
        {
        if(weaponHitbox != null) weaponHitbox.SetActive(true);
        Debug.Log("Hitbox ON");
        }

    public void DeactivateHitbox()
        {
        if(weaponHitbox != null) weaponHitbox.SetActive(false);
        Debug.Log("Hitbox OFF");
        }
}


