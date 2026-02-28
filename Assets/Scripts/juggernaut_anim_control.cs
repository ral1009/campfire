using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class juggernaut_anim_control : MonoBehaviour
{
    private Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            anim.Play("OneHand_Up_Attack_A_1");
        } else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            anim.Play("OneHand_Up_Attack_A_2");
        } else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
             anim.Play("OneHand_Up_Attack_A_3");
        }   else if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            anim.Play("OneHand_Up_Attack_B_1");
        } else if (Keyboard.current.digit5Key.wasPressedThisFrame){
            anim.Play("OneHand_Up_Attack_B_2");
        } else if (Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            anim.Play("OneHand_Up_Attack_B_3");
        }
    }
}
