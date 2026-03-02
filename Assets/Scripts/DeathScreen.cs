using UnityEngine;

public class DeathScreen : MonoBehaviour
{
    private CanvasGroup deathScreenCanvasGroup;
    public bool dead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        deathScreenCanvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if (dead)
        {
            deathScreenCanvasGroup.alpha += 0.05f;
        }
    }
}
