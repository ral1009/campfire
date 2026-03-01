using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; 

public class juggernaut_anim_control : MonoBehaviour
{
    public gamemanager gameManager;
    public Animator anim;
    public GameObject weaponHitbox;
    public static juggernaut_anim_control Instance;

    private Coroutine timeRoutine;
    public float slowdown;
    
    // Track if we are already attacking to prevent the Update loop from looping forever
    private bool isAttacking = false;

    void Awake()
    {
        // Finding the animator here prevents the NullReferenceException in GameManager
        anim = GetComponent<Animator>();
        Instance = this;
    }

    void Start()
    {
        if(weaponHitbox != null) weaponHitbox.SetActive(false);
    }

    void Update()
    {
        // Check if it's the Enemy turn and we haven't started the attack sequence yet
        if (gamemanager.Instance != null && gamemanager.Instance.currentGameState == "EnemyAttack" && !isAttacking)
        {
            StartCoroutine(EnemyAttackSequence());
        }
    }

    IEnumerator EnemyAttackSequence()
    {
        isAttacking = true;
        yield return new WaitForSeconds(1.3f); 
        // Decide how many times to swing (1 to 4)
        int rand = Random.Range(1, 6); 

        for (int i = 0; i < rand; i++)
        {
            // Pick a random attack (1 to 6)
            int choice = Random.Range(1, 6); 
            if (choice == 1) anim.CrossFade("OneHand_Up_Attack_A_1_InPlace", 0.2f);
            else if (choice == 2) anim.CrossFade("OneHand_Up_Attack_A_2_InPlace", 0.2f);
            else if (choice == 3) anim.CrossFade("OneHand_Up_Attack_A_3_InPlace", 0.2f);
            else if (choice == 4) anim.CrossFade("OneHand_Up_Attack_B_1_InPlace", 0.2f);
            else if (choice == 5) anim.CrossFade("OneHand_Up_Attack_B_2_InPlace", 0.2f);
            else if (choice == 6) anim.CrossFade("OneHand_Up_Attack_B_3_InPlace", 0.2f);

            // Wait for the animation to actually play before picking the next one
            yield return new WaitForSeconds(2.5f); 
        }

        // Once all random attacks are done, go back to Neutral
        yield return new WaitForSeconds(.5f); 
        anim.CrossFade("OneHand_Up_Run_B 0", 0.2f);
        gameManager.enterNeutral();
        isAttacking = false;
    }

    public void RunForward()
    {
        Debug.Log("Juggernaut is sprinting!");
        if (anim != null)
        {
            anim.CrossFade("OneHand_Up_Sprint_F", 0.2f);
        }
    }

    public void TakeDamage()
    {
        anim.CrossFade("Hit_F_2", 0.2f);
    }

    // --- HITBOX & TIME LOGIC ---

    public void ActivateHitbox()
    {
        if(weaponHitbox != null) weaponHitbox.SetActive(true);
        Debug.Log("Hitbox ON - Starting Parry Window");

        if (timeRoutine != null) StopCoroutine(timeRoutine);
        timeRoutine = StartCoroutine(SlowMotionWindow(slowdown, 0.6f));
    }

    public void DeactivateHitbox()
    {
        if(weaponHitbox != null) weaponHitbox.SetActive(false);
        Debug.Log("Hitbox OFF - Normal Speed");

        if (timeRoutine != null) StopCoroutine(timeRoutine);
        ResetTime();
    }

    IEnumerator SlowMotionWindow(float targetScale, float duration)
    {
        Time.timeScale = targetScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        float t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 3f;
            Time.timeScale = Mathf.Lerp(targetScale, 1f, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }
        ResetTime();
    }

    public void ResetTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}