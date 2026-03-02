using UnityEngine;
using System.Collections;

public class gamemanager : MonoBehaviour
{
    public static gamemanager Instance;
    
    void Awake() 
    {
        Instance = this;
    }

    public playeranimation playerAnimation;
    public juggernaut_anim_control juggernaut_Anim_Control;
    public int PlayerHealth = 6767;
    public int EnemyHealth = 676741;
    public Transform player;
    public Transform enemy;
    public float movespeed = 50f;
    public float rotationSpeed = 5f;
    public bool movementcompleted = false;

    [Header("State Tracking")]
    public string currentGameState; 
    
    private Vector3 enemyAttackPos = new Vector3(10.8428f, 0f, -1.08772f);
    private Vector3 playerNeutralPos = new Vector3(10.6f, 0f, 8.5f);
    private Vector3 enemyNeutralPos = new Vector3(10.8428f, 0f, -40.8f);
    private Vector3 playerAttackPos = new Vector3(10.6f, 0f, -31.22f);

    private Vector3 camAttackLocPos = new Vector3(1.81f, 2.02f, -0.7399999f);
    private Quaternion camAttackLocRot = Quaternion.Euler(16.658f, -51.884f, 0f);
    
    private Vector3 camNeutralLocPos = new Vector3(0.846f, 1.715f, -0.757f);
    private Quaternion camNeutralLocRot = Quaternion.Euler(10.913f, -19.813f, 1.589f);

    private Transform cam;
    
    // Reference to the active movement so we don't kill other scripts
    private Coroutine activeMoveCoroutine;

    void Start()
    {
        cam = Camera.main.transform;
        enterNeutral();
    }

    public void enterPlayerAttack() 
    {
        currentGameState = "PlayerAttack";
        playerAnimation.MoveForward();
        movementcompleted = false;
        Transition(playerAttackPos, enemyNeutralPos, camAttackLocPos, camAttackLocRot);
    }

    public void enterEnemyAttack() 
    {
        currentGameState = "EnemyAttack";
        playerAnimation.MoveBackward();
        movementcompleted = false;
        Transition(playerNeutralPos, enemyAttackPos, camAttackLocPos, camAttackLocRot);
    }

    public void enterNeutral() 
    {
        currentGameState = "Neutral";
        playerAnimation.MoveBackward();
        movementcompleted = false;
        Transition(playerNeutralPos, enemyNeutralPos, camNeutralLocPos, camNeutralLocRot);
    }

    public void enterTransition() 
    {
        currentGameState = "Transition";
        playerAnimation.MoveBackward();
        movementcompleted = false;
        Transition(playerNeutralPos, enemyNeutralPos, camNeutralLocPos, camNeutralLocRot);
    }

    private void Transition(Vector3 pT, Vector3 eT, Vector3 cP, Quaternion cR)
    {
        // FIX: Only stop the movement, not the TurnManager or Juggernaut logic!
        if (activeMoveCoroutine != null) StopCoroutine(activeMoveCoroutine);
        activeMoveCoroutine = StartCoroutine(MoveToState(pT, eT, cP, cR));
    }

    IEnumerator MoveToState(Vector3 pTarget, Vector3 eTarget, Vector3 cLocalPos, Quaternion cLocalRot)
    {
        while (Vector3.Distance(player.position, pTarget) > 0.01f || Vector3.Distance(enemy.position, eTarget) > 0.1f)
        {
            player.position = Vector3.MoveTowards(player.position, pTarget, movespeed * Time.deltaTime);
            enemy.position = Vector3.MoveTowards(enemy.position, eTarget, movespeed * Time.deltaTime);

            cam.localPosition = Vector3.MoveTowards(cam.localPosition, cLocalPos, movespeed * Time.deltaTime);
            cam.localRotation = Quaternion.Slerp(cam.localRotation, cLocalRot, rotationSpeed * Time.deltaTime);

            yield return null;
        }

        player.position = pTarget;
        enemy.position = eTarget;
        cam.localPosition = cLocalPos;
        cam.localRotation = cLocalRot;
        movementcompleted = true;
    }
}