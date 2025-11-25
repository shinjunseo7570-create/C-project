using UnityEngine;

public class RaidalMenuManager : MonoBehaviour
{
    [Header("����")]
    public GameObject elementMenuRoot;     
    public GameObject attackMenuRoot;


    public PlayerSkillController player;


    public float slowTimeScale = 0.2f;

    public bool isMenuOpen = false;

    void Start()
    {
        // �����ҋ� �޴� ����
        elementMenuRoot.SetActive(false);
        attackMenuRoot.SetActive(false);
    }

    void Update()
    {
        // �޴� ���ÿ� �ȶ߰� ����
        if (!isMenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("[RM] Q pressed");
                OpenElementMenu();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("[RM] E pressed");
                OpenAttackMenu();
            }
        }
    }

    public bool IsMenuOpen()
    {
        return isMenuOpen;
    }

    public void OpenElementMenu()
    {
        isMenuOpen = true;
        elementMenuRoot.SetActive(true);
        attackMenuRoot.SetActive(false);

        // ���ο� ���
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public void OpenAttackMenu()
    {
        isMenuOpen = true;
        attackMenuRoot.SetActive(true);
        elementMenuRoot.SetActive(false);

        // ���ο� ���
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public void CloseAllMenus()
    {
        isMenuOpen = false;
        elementMenuRoot.SetActive(false);
        attackMenuRoot.SetActive(false);

        // �ð� �������
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    // ---------- ��ư���� ȣ���� �Լ��� ----------

    // �Ӽ� ����
    public void SelectElement_Fire()
    {
        player.SetElement(ElementType.Fire);
        Debug.Log("원소변형됨 : 불");
        CloseAllMenus();
    }

    public void SelectElement_Water()
    {
        player.SetElement(ElementType.Water);
        Debug.Log("원소변형됨 : 물");
        CloseAllMenus();
    }

    public void SelectElement_Wind()
    {
        player.SetElement(ElementType.Wind);
        Debug.Log("원소변형됨 : 바람");
        CloseAllMenus();
    }

    public void SelectElement_Earth()
    {
        player.SetElement(ElementType.Earth);
        Debug.Log("원소변형됨 : 땅");
        CloseAllMenus();
    }

    // ���� Ÿ�� ����
    public void SelectAttack_Normal()
    {
        player.SetAttackType(AttackType.Normal);
        Debug.Log("공격방식 변형됨 : 일반");
        CloseAllMenus();
    }

    public void SelectAttack_Melee()
    {
        player.SetAttackType(AttackType.Melee);
        Debug.Log("공격방식 변형됨 : 근접");
        CloseAllMenus();
    }

    public void SelectAttack_Snipe()
    {
        player.SetAttackType(AttackType.Snipe);
        Debug.Log("공격방식 변형됨 : 저격");
        CloseAllMenus();
    }

    public void SelectAttack_Area()
    {
        player.SetAttackType(AttackType.Area);
        Debug.Log("공격방식 변형됨 : 지정");
        CloseAllMenus();
    }
}
