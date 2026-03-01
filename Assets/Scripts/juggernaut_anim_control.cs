using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Needed for IEnumerator

public class juggernaut_anim_control : MonoBehaviour
{
    public gamemanager gameManager;
    public Animator anim;
    public GameObject weaponHitbox;
    public static juggernaut_anim_control Instance;

    private Coroutine timeRoutine;
    public float slowdown;

    void Awake()
    {
        anim = GetComponent<Animator>();
        Instance = this;
    }
    void Start()
    {
        if(weaponHitbox != null) weaponHitbox.SetActive(false);
    }

    void Update()
    {
        if (gamemanager.Instance != null && gamemanager.Instance.currentGameState == "EnemyAttack")
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) anim.CrossFade("OneHand_Up_Attack_A_1_InPlace", 0.2f);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) anim.CrossFade("OneHand_Up_Attack_A_2_InPlace", 0.2f);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) anim.CrossFade("OneHand_Up_Attack_A_3_InPlace", 0.2f);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) anim.CrossFade("OneHand_Up_Attack_B_1_InPlace", 0.2f);
            if (Keyboard.current.digit5Key.wasPressedThisFrame) anim.CrossFade("OneHand_Up_Attack_B_2_InPlace", 0.2f);
            if (Keyboard.current.digit6Key.wasPressedThisFrame) anim.CrossFade("OneHand_Up_Attack_B_3_InPlace", 0.2f);
        }
    }
    public void TakeDamage()
    {
        anim.CrossFade("Hit_F_2", 0.2f);
    }
    public void RunForward()
    {
        Debug.Log("run");
        anim.CrossFade("OneHand_Up_Sprint_F",0.2f);
    }
    public void ActivateHitbox()
    {
        if(weaponHitbox != null) weaponHitbox.SetActive(true);
        Debug.Log("Hitbox ON - Starting Parry Window");

        // Start slow-mo when attack becomes active
        if (timeRoutine != null) StopCoroutine(timeRoutine);
        timeRoutine = StartCoroutine(SlowMotionWindow(slowdown, 0.6f)); // 40% speed for 0.6 seconds
    }

    public void DeactivateHitbox()
    {
        if(weaponHitbox != null) weaponHitbox.SetActive(false);
        Debug.Log("Hitbox OFF - Normal Speed");

        // Cancel slow-mo if the attack ends naturally
        if (timeRoutine != null) StopCoroutine(timeRoutine);
        ResetTime();
    }

    IEnumerator SlowMotionWindow(float targetScale, float duration)
    {
        Time.timeScale = targetScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        // Smooth ramp back up
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