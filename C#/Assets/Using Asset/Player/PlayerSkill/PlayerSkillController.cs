using UnityEngine;
using System.Collections.Generic;

// 조합 데이터를 저장할 구조체
[System.Serializable]
public struct SkillCombination
{
    public AttackType attackType;   // 공격 모드 (Normal, Snipe, Melee, Area)
    public ElementType elementType; // 속성 (Fire, Water, Wind, Earth)
    public GameObject projectilePrefab; // 나갈 투사체 프리팹
}

public class PlayerSkillController : MonoBehaviour
{
    [Header("상태 데이터")]
    public ElementType currentElement = ElementType.Fire;
    public AttackModeData currentAttackMode;

    [Header("설정 데이터 (데이터베이스)")]
    // 무기 데이터 (공격 속도, 사거리 등 스펙용)
    public List<AttackModeData> availableAttackModes;

    // 속성 데이터 (속성 아이콘이나 정보용)
    public List<AttackModeData> availableElements;

    // 16가지 조합 테이블 리스트
    public List<SkillCombination> skillCombinations;

    [Header("UI 및 매니저 연결")]
    public GameObject elementSelectionUI;
    public GameObject attackSelectionUI;
    public RaidalMenuManager menuManager;

    [Header("발사 위치 설정")]
    public Transform Balsa;


    private float nextActionTime = 0f;
    private bool isSelecting = false;
    private const float TimeScaleFactor = 0.1f;

    // 현재 선택된 속성/무기 번호
    private int currentElementIndex = 0;
    private int currentWeaponIndex = 0;

    private void Start()
    {
        
        // 게임 시작 시 첫 번째 무기, 첫 번째 속성으로 초기화
        if (availableAttackModes.Count > 0)
        {
            EquipWeapon(0);
        }

        // 초기 속성 설정 
        if (availableElements.Count > 0)
        {
            EquipElement(0);
        }
    }

    void Update()
    {
        // 메뉴가 열려있으면 공격 불가
        bool menuOpen = menuManager != null && menuManager.IsMenuOpen();
        if (menuOpen) return;

        // 마우스 클릭 시 공격
        if (Input.GetMouseButtonDown(0) && !isSelecting)
        {
            PerformAttack();
        }
    }

    // 공격 실행 함수
    public void PerformAttack()
    {
        if (Time.time < nextActionTime) return;
        if (currentAttackMode == null) return;

        // 현재 무기 타입과 속성에 맞는 프리팹 찾기
        GameObject finalPrefab = FindCombiPrefab(currentAttackMode.attackType, currentElement);

        // 만약 조합을 못 찾았다면? 기본 설정된 프리팹 사용
        if (finalPrefab == null)
        {
            Debug.LogWarning($"[Skill] 조합을 찾지 못해 기본 프리팹을 사용합니다: {currentAttackMode.attackType} + {currentElement}");
            finalPrefab = currentAttackMode.assetPrefab;
        }

        // 마우스 위치 계산 (Area)
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector3 spawnPosition;
        Quaternion spawnRotation;

        // 공격 타입에 따른 위치 분기 처리
        if (currentAttackMode.attackType == AttackType.Area)
        {
            // [Area] 마우스 클릭 위치에 생성
            spawnPosition = mouseWorldPos;
            spawnRotation = Quaternion.identity; // 회전 없음 (필요하면 변경)
        }
        else
        {
            // [Normal, Snipe, Melee] 플레이어 발사대 위치에서 생성
            spawnPosition = (Balsa != null) ? Balsa.position : transform.position;
            spawnPosition.z = 0f;

            // 마우스 방향으로 회전
            Vector3 direction = (mouseWorldPos - spawnPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            spawnRotation = Quaternion.Euler(0, 0, angle);
        }

        // 투사체 생성 
        if (finalPrefab != null)
        {
            GameObject projectile = Instantiate(finalPrefab, spawnPosition, spawnRotation);

            // 투사체 스크립트에 데이터 전달
            SkillController skillScript = projectile.GetComponent<SkillController>();
            if (skillScript != null)
            {
                skillScript.Init(currentAttackMode.projectileSpeed, currentAttackMode.range, currentAttackMode.damage);
            }
        }

        // 쿨타임 적용
        nextActionTime = Time.time + currentAttackMode.attackInterval;
    }

    // 리스트에서 조건에 맞는 프리팹을 검색하는 함수
    private GameObject FindCombiPrefab(AttackType aType, ElementType eType)
    {
        foreach (var combi in skillCombinations)
        {
            if (combi.attackType == aType && combi.elementType == eType)
            {
                return combi.projectilePrefab;
            }
        }
        return null; // 못 찾음
    }

    //무기 및 속성 교체

    private void EquipWeapon(int index)
    {
        if (index < 0 || index >= availableAttackModes.Count) return;

        currentWeaponIndex = index;
        currentAttackMode = availableAttackModes[index];
        // Debug.Log($"무기 변경됨: {currentAttackMode.name}");
    }

    private void EquipElement(int index)
    {
        if (index < 0 || index >= availableElements.Count) return;

        currentElementIndex = index;
        // [수정] availableElements 안의 element 속성을 가져옴 (오류 해결)
        currentElement = availableElements[index].elementType;

        Debug.Log($"속성 변경됨: {currentElement}");
    }

    // UI나 키 입력으로 속성 변경 시 호출
    public void SetElement(ElementType element)
    {
        if (availableElements == null || availableElements.Count == 0) return;

        // 들어온 속성과 일치하는 데이터를 리스트에서 찾아서 장착
        for (int i = 0; i < availableElements.Count; i++)
        {
            if (availableElements[i].elementType == element)
            {
                EquipElement(i);
                return;
            }
        }
    }

    // UI나 키 입력으로 공격 타입 변경 시 호출
    public void SetAttackType(AttackType type)
    {
        if (availableAttackModes == null || availableAttackModes.Count == 0) return;

        // 들어온 타입과 일치하는 무기 데이터를 리스트에서 찾아서 장착
        for (int i = 0; i < availableAttackModes.Count; i++)
        {
            if (availableAttackModes[i].attackType == type)
            {
                EquipWeapon(i);
                return;
            }
        }
        Debug.LogWarning($"해당 타입({type})의 무기 데이터가 리스트에 없습니다.");
    }
}