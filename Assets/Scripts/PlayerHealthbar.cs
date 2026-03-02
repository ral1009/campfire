using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthbar : MonoBehaviour
{

    public gamemanager gameManager;
    public static PlayerHealthbar Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.PlayerHealth <= 0)
        {
            Debug.Log("Loss");
        }
        if (gameManager.EnemyHealth <= 0)
        {
            Debug.Log("Win");
        }
    }

    public void updatePlayerHealthbar(float currentHealth, float maxHealth)
    {
        float healthRatio = currentHealth / maxHealth;
        GetComponent<Image>().fillAmount = healthRatio;

        float newGlowX = (432 * healthRatio) - 2;
        transform.Find("Glow").GetComponent<RectTransform>().anchoredPosition = new Vector2(newGlowX, 0);
    }
}
