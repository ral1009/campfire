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

    [Header("UI Feedback")]
    public GameObject parryTextPrefab;
    private Transform uiCanvas;

    private float lastParryPressTime = -10f;
    private bool isProcessingHit = false;

    void Awake()
    {
        // Auto-find the UI Canvas
        GameObject canvasObj = GameObject.Find("MainCanvas");
        if (canvasObj != null) uiCanvas = canvasObj.transform;

        // Auto-find the Gesture Receiver
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
            // Check Early Buffer
            if (Time.time - lastParryPressTime <= parryBufferTime)
            {
                ExecuteParry();
            }
            else
            {
                // Start Late Window
                StartCoroutine(LateParryWindow());
            }
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

        // SPAWN UI FEEDBACK
        if (parryTextPrefab != null && uiCanvas != null)
        {
            GameObject popup = Instantiate(parryTextPrefab, uiCanvas);
            popup.transform.localPosition = Vector3.zero;
            
            // Trigger the text logic if the component exists
            ParryText pText = popup.GetComponent<ParryText>();
            if (pText != null) pText.createParryText();
        }
    }

    void ExecuteHit()
    {
        playerAnimation.TakeDamage();
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }
}