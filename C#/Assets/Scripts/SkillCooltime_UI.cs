using UnityEngine;
using UnityEngine.UI;

public class SkillCooltime_UI : MonoBehaviour
{
    public RaidalMenuManager menu;
    public Image qCool;
    public Image eCool;

    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        if (menu == null) return;

        float qRatio = menu.GetQCooldownRatio();
        float eRatio = menu.GetECooldownRatio();

        qCool.fillAmount = 1f - qRatio;
        eCool.fillAmount = 1f - eRatio;
    }
}
