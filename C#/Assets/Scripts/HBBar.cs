using UnityEngine;
using UnityEngine.UI;

public class HBBar : MonoBehaviour
{
    public PlayerInteract player;
    public Slider hpSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpSlider.maxValue = player.maxHp;
        hpSlider.value = player.Hp;
    }

    // Update is called once per frame
    void Update()
    {
        hpSlider.value = player.Hp;
    }
}
