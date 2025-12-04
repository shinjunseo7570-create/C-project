using UnityEngine;
using UnityEngine.UI;

public class Enemy_Boss_Hp : MonoBehaviour
{
    public Enemy enemy;
    public Slider hpSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Setup(Enemy enemy)
    {
        this.enemy = enemy;

        if (enemy != null && hpSlider != null)
        {
            hpSlider.maxValue = enemy.maxHealth;
            hpSlider.value = enemy.health;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (enemy == null || hpSlider == null) return;
        hpSlider.value = enemy.health;
    }
}
