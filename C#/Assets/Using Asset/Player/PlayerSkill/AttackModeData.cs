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

    // ����� ����Ʈ/����ü ���� ������
    public GameObject assetPrefab;

    public ElementType element;
}

public enum ElementType
{
    None, // 0
    Fire, // 1
    Water, // 2
    Wind, // 3
    Earth // 4
}
