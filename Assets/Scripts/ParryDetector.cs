using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ParryDetector : MonoBehaviour
{
    public Animator anim;
    public playeranimation playerAnimation;
    
    [Header("Generous Windows")]
    public float parryBufferTime = 0.3f; // How long "Space" stays active BEFORE the hit
    public float lateGracePeriod = 0.1f; // How long they have to press Space AFTER the hit
    
    private float lastSpacePressTime = -10f;
    private bool isProcessingHit = false;

    void Update()
    {
        // Whenever Space is pressed, "Buffer" it
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            lastSpacePressTime = Time.time;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyHitBox") && !isProcessingHit)
        {
            // 1. CHECK EARLY BUFFER (Did they press Space just BEFORE contact?)
            if (Time.time - lastSpacePressTime <= parryBufferTime)
            {
                ExecuteParry();
            }
            else
            {
                // 2. START LATE WINDOW (Give them a chance to press Space AFTER contact)
                StartCoroutine(LateParryWindow());
            }
        }
    }

    IEnumerator LateParryWindow()
    {
        isProcessingHit = true;
        float timer = 0f;

        while (timer < lateGracePeriod)
        {
            timer += Time.unscaledDeltaTime;

            // If they press Space now, they saved themselves!
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ExecuteParry();
                isProcessingHit = false;
                yield break; 
            }
            yield return null;
        }

        // 3. IF WINDOW EXPIRES: They actually get hit
        ExecuteHit();
        isProcessingHit = false;
    }

    void ExecuteParry()
    {
        Debug.Log("Parried! (Buffer or Late Window)");
        playerAnimation.Parry();
        
        // Return time to normal immediately on success
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }

    void ExecuteHit()
    {
        Debug.Log("Hit Taken");
        playerAnimation.TakeDamage();
        if (gamemanager.Instance != null) gamemanager.Instance.PlayerHealth -= 676;
        
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }
}