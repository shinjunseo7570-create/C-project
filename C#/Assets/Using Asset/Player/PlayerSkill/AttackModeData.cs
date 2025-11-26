using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackModeData", menuName = "Skill System/Attack Mode Data")]
public class AttackModeData : ScriptableObject // **ScriptableObject�� ��ӹ޾ƾ� �մϴ�.**
{
    // ���� ����� �̸� (�⺻�� ������ ����)
    public string modeName = "Default Mode";

    // ���� ����
    public float attackInterval = 1.0f;

    // ������
    public float damage = 1.0f;

    // ��Ÿ�
    public float range = 5.0f;

    //����ü �ӵ�
    public float projectileSpeed = 10.0f;

    // ���� Ÿ�� ����
    public bool isMultiTarget = false;

    public string elementName = "Default Element";

    // ����� ����Ʈ/����ü ���� ������
    public GameObject assetPrefab;

    public ElementType element; 
    public AttackType attackType;
}

public enum ElementType
{

    Fire, 
    Water, 
    Wind, 
    Earth 
}

public enum AttackType
{
    Normal,
    Snipe,
    Melee,
    Area
}
