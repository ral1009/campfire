using UnityEngine;

public class playeranimation : MonoBehaviour
{
    public Animator anim;
    public static playeranimation Instance;
    public int healthStart;
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
        anim.CrossFade("OneHand_Up_Run_F_InPlace",0.2f);
    }

    public void MoveBackward()
    {
        anim.CrossFade("OneHand_Up_Run_B",0.2f);
    }

    public void DecreaseHealth()
    {
        gamemanager.Instance.PlayerHealth-=676;
        Debug.Log(gamemanager.Instance.PlayerHealth);
    }
}
