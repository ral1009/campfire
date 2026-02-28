using UnityEngine;

public class Logic : MonoBehaviour
{
    public bool isPlayerTurn = true;
    public int bossHP = 50000;
    public int playerHP = 100;
    public int healPotsLeft = 3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void dealDamageToBoss(int damage)
    {
        bossHP -= damage;
    }

    public void dealDamageToPlayer(int damage)
    {
        playerHP -= damage;
    }

    public void attack()
    {
        dealDamageToBoss(10000);
    }

    public void heal()
    {
        playerHP -= 40;
        healPotsLeft -= 1;
    }
}