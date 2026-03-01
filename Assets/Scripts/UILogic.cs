using UnityEngine;

public class UILogic : MonoBehaviour
{
    private CanvasGroup optionsCanvasGroup;
    public Turnmanager turnmanager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        optionsCanvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if (turnmanager.isPlayerTurn)
        {
            optionsCanvasGroup.alpha += 0.1f;
        } 
        else
        {
            optionsCanvasGroup.alpha -= 0.1f;
        }
    }
}
