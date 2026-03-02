using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ParryDetector : MonoBehaviour
{
    public Animator anim;
    public playeranimation playerAnimation;

    [Header("UDP Gesture Receiver")]
    public UdpGestureReceiverV2 gestureReceiver;
    public bool autoFindGestureReceiver = true;

    [Header("Generous Windows")]
    public float parryBufferTime = 0.3f;
    public float lateGracePeriod = 0.1f;

    private float lastParryPressTime = -10f;
    private bool isProcessingHit = false;

    void Awake()
    {
        if (autoFindGestureReceiver && gestureReceiver == null)
        {
#if UNITY_2023_1_OR_NEWER
            gestureReceiver = Object.FindFirstObjectByType<UdpGestureReceiverV2>();
#else
            gestureReceiver = FindObjectOfType<UdpGestureReceiverV2>();
#endif
        }
    }

    void Update()
    {
        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool crossPressed = gestureReceiver != null && gestureReceiver.crossPressedThisFrame;

        if (spacePressed || crossPressed)
            lastParryPressTime = Time.time;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyHitBox") && !isProcessingHit)
        {
            if (Time.time - lastParryPressTime <= parryBufferTime) ExecuteParry();
            else StartCoroutine(LateParryWindow());
        }
    }

    IEnumerator LateParryWindow()
    {
        isProcessingHit = true;
        float hitTime = Time.time;
        float timer = 0f;

        while (timer < lateGracePeriod)
        {
            timer += Time.unscaledDeltaTime;

            if (lastParryPressTime >= hitTime)
            {
                ExecuteParry();
                isProcessingHit = false;
                yield break;
            }
            yield return null;
        }

        ExecuteHit();
        isProcessingHit = false;
    }

    void ExecuteParry()
    {
        playerAnimation.Parry();
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }

    void ExecuteHit()
    {
        playerAnimation.TakeDamage();
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }
}