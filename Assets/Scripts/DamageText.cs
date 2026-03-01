using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float destroyTime = 1f;
    private TextMeshProUGUI textMesh;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    // Call this right after spawning to set the number
    public void setDamageNumber(int damageAmount)
    {
        textMesh.SetText(damageAmount.ToString() + "!");
    }

    void Update()
    {
        // Move the text upwards over time
        transform.position += new Vector3(0, floatSpeed * Time.deltaTime, 0);

        // Optional: Fade out the alpha over time
        textMesh.alpha -= Time.deltaTime / destroyTime;

        // Clean up memory
        Destroy(gameObject, destroyTime);
    }
}