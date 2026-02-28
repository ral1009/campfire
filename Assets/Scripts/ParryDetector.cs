using UnityEngine;
using UnityEngine.InputSystem;

public class ParryDetector : MonoBehaviour
{
    bool Parried = false;
    private Animator anim;
    public GameObject weaponHitbox;
    private GameObject mainPlayer;
    private Vector3 homePosition;
    private bool isReturning = false;
    
    public float returnSpeed = 100f;
    public float delayTime = 1.3f;
    public float crossfadetime = 0.2f;
    public Vector3 offset = new Vector3(0,0,0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponentInParent<Animator>();
        mainPlayer = anim.gameObject; 
        homePosition = mainPlayer.transform.position;
        homePosition += offset;
    }

    // Update is called once per frame
    void Update()
    {
        if (isReturning)
        {
            mainPlayer.transform.position = Vector3.MoveTowards(
                mainPlayer.transform.position, 
                homePosition, 
                returnSpeed * Time.deltaTime
            );

            if (Vector3.Distance(mainPlayer.transform.position, homePosition) < 0.01f)
            {
                isReturning = false;
                anim.CrossFade("OneHand_Up_Stand_Idle_A_2",crossfadetime);
            }
        }
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
            }
        else if (Parried == false)
            {
                Debug.Log("Hit");
                anim.CrossFade("Hit_F_1",crossfadetime);
                Invoke("StartReturn", delayTime);
            }
    }
    
    }
    void StartReturn()
    {
        isReturning = true;
        anim.CrossFade("OneHand_Up_Run_F_InPlace",crossfadetime);
    }
}
