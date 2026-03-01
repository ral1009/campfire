using UnityEngine;
using UnityEngine.InputSystem;

public class ParryDetector : MonoBehaviour
{
    bool Parried = false;
    private Animator anim;
    public GameObject weaponHitbox;
    private GameObject mainPlayer;
    
    public float delayTime = 1.3f;
    public float crossfadetime = 0.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponentInParent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void OnTriggerEnter(Collider other)
    {
        Parried = false;
        if (weaponHitbox != null && other.CompareTag("EnemyHitBox"))
    {
        Debug.Log("Contact!");
        if (Keyboard.current.spaceKey.isPressed)
            {   
                Debug.Log("Parried");
                Parried = true;
                anim.Play("OneHand_Up_Shield_Block_Hit_1_InPlace");
                anim.Play("OneHand_Up_Stand_Idle_A_2");
            }
        else if (Parried == false)
            {
                Debug.Log("Hit");
                anim.CrossFade("Hit_F_1",crossfadetime);
            }
    }
    
    }
}
