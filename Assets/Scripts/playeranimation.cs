using UnityEngine;

public class playeranimation : MonoBehaviour
{
    private Animator anim;
    public static playeranimation Instance;
    void Awake() 
{
    Instance = this;
}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack() {
        anim.CrossFade("PlayerOneHand_Up_Attack_A_1_InPlace 1", 0.2f);
    }

    public void Parry()
    {
        anim.Play("OneHand_Up_Shield_Block_Hit_1_InPlace");
    }

    public void TakeDamage()
    {
        anim.CrossFade("Hit_F_1", 0.2f);
    }
}
