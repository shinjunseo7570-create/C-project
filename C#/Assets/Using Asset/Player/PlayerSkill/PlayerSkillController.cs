using UnityEngine;
using System.Collections.Generic;

public class PlayerSkillController : MonoBehaviour
{
    [Header("데이터 및 상태")]
    public ElementType currentElement = ElementType.None;

   
    public AttackModeData currentAttackMode;

    // 무기 4개를 넣
    public List<AttackModeData> availableAttackModes;

    [Header("UI 설정")]
    public GameObject elementSelectionUI;
    public GameObject attackSelectionUI; // E키는 즉시 변경이지만, 변수는 남겨둠

    public RaidalMenuManager menuManager;

    [Header("발사 위치 설정")]
    public Transform Balsa;

    private float nextActionTime = 0f;
    private bool isSelecting = false;
    private const float TimeScaleFactor = 0.1f;

    // 현재 몇 번째 무기인지 기억할 변수
    private int currentWeaponIndex = 0;

    private void Start()
    {
        // 게임 시작 시 첫 번째 무기로 초기화
        if (availableAttackModes.Count > 0)
        {
            EquipWeapon(0);
        }
    }

    void Update()
    {
        //HandleSelectionInput();



        // 선택 중이 아닐 때만 공격 가능
        bool menuOpen = menuManager != null && menuManager.IsMenuOpen();

        if(menuOpen)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && !isSelecting)
        {
            PerformAttack();
        }
    }

    /*private void HandleSelectionInput()
    {
        // Q키: 속성 선택 (누르고 있는 동안 UI 켜짐)
        if (Input.GetKeyDown(KeyCode.Q) && !isSelecting)
        {
            StartSelection(elementSelectionUI);
        }
        else if (Input.GetKeyUp(KeyCode.Q) && isSelecting && elementSelectionUI.activeSelf)
        {
            EndSelection();
        }

        // E키: 무기 변경 (누를 때마다 즉시 다음 무기로 교체)
        // 속성 선택 창이 켜져있지 않을 때만 가능
        if (Input.GetKeyDown(KeyCode.E) && !isSelecting)
        {
            SwitchToNextWeapon();
        }
    }

    private void SwitchToNextWeapon()
    {
        if (availableAttackModes.Count == 0) return;

        // 인덱스 순환 (0 -> 1 -> 2 -> 3 -> 0)
        currentWeaponIndex = (currentWeaponIndex + 1) % availableAttackModes.Count;

        // 해당 인덱스의 무기 장착
        EquipWeapon(currentWeaponIndex);
    }
    



    private void StartSelection(GameObject uiPanel)
    {
        isSelecting = true;
        Time.timeScale = TimeScaleFactor;
        if (uiPanel != null) uiPanel.SetActive(true);
    }

    public void EndSelection()
    {
        isSelecting = false;
        if (elementSelectionUI != null) elementSelectionUI.SetActive(false);
        if (attackSelectionUI != null) attackSelectionUI.SetActive(false);

        Time.timeScale = 1f;

        Debug.Log($"선택 종료. 현재 속성: {currentElement}");
    }

    // UI 버튼 등에서 호출할 함수
    public void SetElement(int elementIndex)
    {
        currentElement = (ElementType)elementIndex;
    }
    */

    private void EquipWeapon(int index)
    {
        currentAttackMode = availableAttackModes[index];

        Debug.Log($"무기 변경됨: {currentAttackMode.name}");
    }

    // UI 버튼 등에서 호출할 함수, 속성변경
    public void SetElement(ElementType element)
    {
        currentElement = element;
        Debug.Log("속성변경 호출됨");
    }

    // UI 버튼 등에서 호출할 함수
    public void SetAttackType(AttackType type)
    {
        
        if (availableAttackModes == null || availableAttackModes.Count == 0)
        {
            Debug.LogWarning("[PlayerSkill] availableAttackModes가 비어 있습니다.");
            return;
        }


        if(type == AttackType.Normal)
        {
            EquipWeapon(0);
            currentWeaponIndex = 0;
        }

        if(type == AttackType.Snipe)
        {
            EquipWeapon(1);
            currentWeaponIndex = 1;
        }

        if (type == AttackType.Melee)
        {
            EquipWeapon(2);
            currentWeaponIndex = 2;
        }

        if(type == AttackType.Area)
        {
            EquipWeapon(3);
            currentWeaponIndex = 3;
        }


        /*for (int i = 0; i < availableAttackModes.Count; i++)
        {
            AttackModeData mode = availableAttackModes[i];
            if (mode != null && mode.attackType == type)
            {
                // 인덱스 기억 + 실제 무기 장착
                currentWeaponIndex = i;
                EquipWeapon(i);

                Debug.Log($"무기변경 호출됨 : {type} -> {mode.name}");
                return;
            }
        }

        Debug.LogWarning($"[PlayerSkill] {type} 타입의 무기를 찾지 못했습니다.");
        */

        // 리스트에서 해당 AttackType을 가진 무기 데이터 찾기
        /*foreach (var mode in availableAttackModes)
        {
            if (mode != null && mode.attackType == type)
            {
                currentAttackMode = mode;
                Debug.Log("무기변경 호출됨");
                return;
            }
        }*/
    }

    public void PerformAttack()
    {
        bool menuOpen = menuManager != null && menuManager.IsMenuOpen();

        if (menuOpen)
        {
            return;
        }


        if (Time.time < nextActionTime) return;
        if (currentAttackMode == null) return;

        // 발사 위치 계산
        Vector3 spawnPosition = (Balsa != null) ? Balsa.position : transform.position;
        spawnPosition.z = 0f;

        // 마우스 방향 계산
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector3 direction = (mouseWorldPos - spawnPosition).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion spawnRotation = Quaternion.Euler(0, 0, angle);

        // 투사체 생성
        GameObject projectile = Instantiate(currentAttackMode.assetPrefab, spawnPosition, spawnRotation);

        // 데이터 전달
        SkillController skillScript = projectile.GetComponent<SkillController>();
        if (skillScript != null)
        {
            // 무기 데이터 + 현재 속성값도 같이 전달 (필요하다면)
            skillScript.Init(currentAttackMode.projectileSpeed, currentAttackMode.range, currentAttackMode.damage);
        }

        // 공격 속도(쿨타임) 적용
        nextActionTime = Time.time + currentAttackMode.attackInterval;
    }
}