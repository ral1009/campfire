using UnityEngine;
using System.Collections;

public class gamemanager : MonoBehaviour
{
    public Transform player;
    public Transform enemy;
    public float movespeed = 50f;
    public float rotationSpeed = 5f;

    [Header("State Tracking")]
    // This will show "Neutral", "PlayerAttack", or "EnemyAttack" in the Inspector
    public string currentGameState; 

    // Hardcoded World Positions
    private Vector3 enemyAttackPos = new Vector3(10.8428f, 0f, -1.08772f);
    private Vector3 playerNeutralPos = new Vector3(10.6f, 0f, 8.5f);
    private Vector3 enemyNeutralPos = new Vector3(10.8428f, 0f, -40.8f);
    private Vector3 playerAttackPos = new Vector3(10.6f, 0f, -31.22f);

    // Hardcoded Local Values (Relative to Player Parent)
    private Vector3 camAttackLocPos = new Vector3(1.81f, 2.02f, -0.7399999f);
    private Quaternion camAttackLocRot = Quaternion.Euler(16.658f, -51.884f, 0f);
    
    private Vector3 camNeutralLocPos = new Vector3(0.846f, 1.715f, -0.757f);
    private Quaternion camNeutralLocRot = Quaternion.Euler(10.913f, -19.813f, 1.589f);

    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
        enterNeutral();
    }

    public void enterPlayerAttack() 
    {
        currentGameState = "PlayerAttack";
        Transition(playerAttackPos, enemyNeutralPos, camAttackLocPos, camAttackLocRot);
    }

    public void enterEnemyAttack() 
    {
        currentGameState = "EnemyAttack";
        Transition(playerNeutralPos, enemyAttackPos, camAttackLocPos, camAttackLocRot);
    }

    public void enterNeutral() 
    {
        currentGameState = "Neutral";
        Transition(playerNeutralPos, enemyNeutralPos, camNeutralLocPos, camNeutralLocRot);
    }

    private void Transition(Vector3 pT, Vector3 eT, Vector3 cP, Quaternion cR)
    {
        StopAllCoroutines();
        StartCoroutine(MoveToState(pT, eT, cP, cR));
    }

    IEnumerator MoveToState(Vector3 pTarget, Vector3 eTarget, Vector3 cLocalPos, Quaternion cLocalRot)
    {
        while (Vector3.Distance(player.position, pTarget) > 0.01f)
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
    }
}