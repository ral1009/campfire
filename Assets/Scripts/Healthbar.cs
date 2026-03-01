using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{

    public gamemanager gameManager;
    public static Healthbar Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        updateHealthbar(676741, 676741);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void updateHealthbar(float currentHealth, float maxHealth)
    {
        float healthRatio = currentHealth / maxHealth;
        GetComponent<Image>().fillAmount = healthRatio;

        float newGlowX = (432 * healthRatio) - 2;
        transform.Find("Glow").GetComponent<RectTransform>().anchoredPosition = new Vector2(newGlowX, 0);
    }
}
