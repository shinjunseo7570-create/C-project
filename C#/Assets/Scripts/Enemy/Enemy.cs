using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public Rigidbody2D target;
    public int Power = 1;

    [Header("투사체 설정")]
    public bool useProjectile = false;                 
    public GameObject projectilePrefab;                // GolemProjectile 프리팹
    public Transform projectileSpawnPoint;             // 손/입 같은 발사 위치
    public float projectileSpeed = 5f;
    public int projectileDamage = 1;

    [Header("패턴용 투사체 설정")]               
    public GameObject patternprojectilePrefab;                // GolemProjectile 프리팹
    public Transform patternprojectileSpawnPoint;             // 손/입 같은 발사 위치
    public float patternprojectileSpeed = 5f;
    public int patternprojectileDamage = 1;



    public bool isBoss = false;

    public static Action<Enemy> OnEnemyDead;
    public static Action<Enemy> OnBossPhase75;

    public float attackRange;
    public float attackDelay = 1f;

    float attackTimer = 0f;

    bool isAttacking = false;
    public float attackAnimDuration = 0.5f;

    float spawnTime;
    public float spawnProtectTime = 0.3f;

    bool isLive = true;

    bool hasDealtDamageThisAttack = false;

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriter;

    Collider2D col;

    bool phase75 = false;
    bool phase50 = false;
    bool phase25 = false;

    bool isPatternMode = false;







    int typeId;

    float CalcElementMultiplier(ElementType elem)
    {
        // 데미지 연산
        float mult = 1f;

        if (elem == ElementType.Wind || elem == ElementType.Earth)
            mult = 0.5f;

        switch (typeId)
        {
            case 0: // 레드슬라임
                if (elem == ElementType.Fire)
                {
                    mult = 2f;
                }
                else if (elem == ElementType.Water)
                {
                    mult = 0f;
                }
                break;

            case 1: // 블루슬라임
                if (elem == ElementType.Fire)
                {
                    mult = 0f;
                }
                else if (elem == ElementType.Water)
                {
                    mult = 2f;
                }
                break;

            case 2: // 그린슬라임
                if (elem == ElementType.Fire || elem == ElementType.Water)
                {
                    mult = 2f;
                }
                break;
        }
        return mult;
    }

    float CalcAttackTypeMultiplier(AttackType atkType)
    {
        float mult = 1f;

        switch (typeId)
        {
            case 4: // 가재
                switch (atkType)
                {
                    case AttackType.Normal:
                        mult = 0.5f;
                        break;

                    case AttackType.Snipe:
                        mult = 2f;
                        break;

                    case AttackType.Melee:
                        mult = 0.5f;
                        break;

                    case AttackType.Area:
                        mult = 0.5f;
                        break;
                }
                break;

            case 7: // 골렘
                switch (atkType)
                {
                    case AttackType.Normal:
                        mult = 1f;
                        break;

                    case AttackType.Snipe:
                        mult = 2f;
                        break;

                    case AttackType.Melee:
                        mult = 1f;
                        break;

                    case AttackType.Area:
                        mult = 1f;
                        break;
                }
                break;

            case 8: // 강화 골렘
                switch (atkType)
                {
                    case AttackType.Normal:
                        mult = 0.5f;
                        break;

                    case AttackType.Snipe:
                        mult = 2f;
                        break;

                    case AttackType.Melee:
                        mult = 1f;
                        break;

                    case AttackType.Area:
                        mult = 0.5f;
                        break;
                }
                break;
        }
        return mult;
    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();

        col = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        if (!isLive || target == null)
            return;

        if(isPatternMode)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(target.position, rigid.position);
        attackTimer += Time.fixedDeltaTime;


        float currentSpeed = isAttacking ? speed * 1.1f : speed;

        Chase(currentSpeed);


        if (!isAttacking && distance <= attackRange && attackTimer >= attackDelay)
        {
            attackTimer = 0f;
            StartCoroutine(AttackRoutine());
        }
    }

    // 평소에 플레이어를 향해 날아가는 동작
    void Chase(float moveSpeed)
    {
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * moveSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);

        if (!isAttacking)
        {
            anim.SetBool("isAttack", false);
        }
    }

    void AttackPlayer()
    {
        if (!isLive || target == null)
            return;


        float distance = Vector2.Distance(target.position, rigid.position);

        //여유범위 약간 제공 테스트하면서 줄여봐야할듯
        if (distance <= attackRange + 0.5f)
        {
            Debug.Log("플레이어 공격!");

            //플레이어 체력감소 시키는함수(플레이어 안에)를 호출 해서 체력감소 진행
            PlayerInteract player = target.GetComponent<PlayerInteract>();

            if (player != null)
            {
                //여기에 몬스터 atk 변수 넣으면 가능하게 함수 오버로딩?
                player.TakeDamage(Power);
            }

        }
    }



    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        hasDealtDamageThisAttack = false;

        anim.SetBool("isAttack", true);

        yield return new WaitForSeconds(attackAnimDuration * 0.5f);

        if (useProjectile)
        {
            ShootProjectile();
        }

        yield return new WaitForSeconds(attackAnimDuration * 0.5f);

        anim.SetBool("isAttack", false);
        isAttacking = false;
    }

    void ShootProjectile()
    {
        if (!useProjectile) return;
        if (projectilePrefab == null || projectileSpawnPoint == null || target == null)
            return;

        Vector3 spawnPos = projectileSpawnPoint.position;
        if (spriter.flipX)
        {
            // 몬스터 기준으로 X 오프셋을 좌우 반전
            float offsetX = projectileSpawnPoint.position.x - transform.position.x;
            spawnPos.x = transform.position.x - offsetX;
        }

        Vector2 dir = (target.position - (Vector2)spawnPos).normalized;

        GameObject projObj = Instantiate(
            projectilePrefab,
            spawnPos,
            Quaternion.identity
        );

        SpriteRenderer sr = projObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.flipX = (dir.x < 0);

        GolemProjectile proj = projObj.GetComponent<GolemProjectile>();

        if (proj != null)
        {
            proj.damage = projectileDamage;

            proj.Launch(dir, projectileSpeed);

            Debug.Log($"발사 dir = {dir}");
        }



    }



    IEnumerator DieRoutine()
    {
        isAttacking = false;

        if (rigid != null)
        {
            rigid.linearVelocity = Vector2.zero;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        if (anim != null)
        {
            anim.SetBool("isAttack", false);
            anim.SetTrigger("Death");
        }

        yield return new WaitForSeconds(1f);

        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (!isLive || target == null)
            return;

        if (isPatternMode)
            return;

        // 플레이어의 X축 값과 적의 X축 값을 비교하여 작으면 true
        spriter.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        }
        else
        {
            target = null;
        }

        isLive = true;


        attackTimer = 0f;
        isAttacking = false;
        spawnTime = Time.time;

        if (col != null)
            col.enabled = true;

    }


    public void Init(SpawnData data)
    {
        typeId = data.spriteType;


        speed = data.Speed;
        maxHealth = data.Health;
        health = data.Health;
        attackRange = data.Range;
        Power = data.Attack;

        if (col != null)
            col.enabled = true;

        if (isBoss)
        {
            Enemy_Boss_Hp bosshpUI = FindAnyObjectByType<Enemy_Boss_Hp>();
            if (bosshpUI != null)
            {
                bosshpUI.gameObject.SetActive(true);
                bosshpUI.Setup(this);
            }
        }

        float spawnDist = Vector2.Distance(target.position, rigid.position);
        Debug.Log($"[Enemy.Init] spawnDist = {spawnDist}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLive) return;

        if (collision.CompareTag("Skill"))
        {
            SkillController skill = collision.GetComponent<SkillController>();

            if (skill == null)
                return;

            float elemMult = CalcElementMultiplier(skill.Element);

            float atkMult = CalcAttackTypeMultiplier(skill.AttackType);

            float finalDamage = skill.Damage * atkMult * elemMult;
            health -= finalDamage;

            Debug.Log($"[Enemy] type={typeId}, elem={skill.Element}, atkType={skill.AttackType}, elemMult={elemMult}, atkMult={atkMult}, dmg={skill.Damage} -> {finalDamage}");

            if (health > 0)
            {
                if (anim != null)
                {
                    anim.SetTrigger("Hit");
                }
                CheckBossPhase();
            }
            else
            {
                Dead();
            }
        }
    }

    void CheckBossPhase()
    {
        if (!isBoss) return;

        float hpPercent = health / maxHealth;

        if (!phase75 && hpPercent <= 0.75f)
        {
            phase75 = true;
            
            OnBossPhase75?.Invoke(this);
        }

        if (!phase50 && hpPercent <= 0.50f)
        {
            phase50 = true;
            StartCoroutine(Phase50Loop());
        }

        if(!phase25 && hpPercent <= 0.25f)
        {
            phase25 = true;
            StartCoroutine(Phase25Loop());
        }
    }

    // 50% 패턴
    IEnumerator Phase50Pattern(float rotateOffset = 0f)
    {
        Debug.Log("Boss 50% 패턴");

        int count = 32; // 방향 개수 (원형)
        float interval = 0f; // 발사 간격

        for (int i = 0; i < count; i++)
        {
            // 0도~360도 사이를 32등분
            float baseAngle = (360f / count) * i;
            float jjaksooAngle = baseAngle + rotateOffset;

            // 각도 → 방향 벡터 (cos, sin)
            Vector2 dir = new Vector2(
                Mathf.Cos(jjaksooAngle * Mathf.Deg2Rad),
                Mathf.Sin(jjaksooAngle * Mathf.Deg2Rad)
            ).normalized;

            // 방향으로 투사체 발사
            ShootCustomProjectile(dir);

            // 조금씩 텀을 두고 연속 발사하고 싶으면 사용
            yield return new WaitForSeconds(interval);
        }
    }

    // 50% 패턴 반복
    IEnumerator Phase50Loop()
    {
        isPatternMode = true;

        int repeatCount = 5;     
        float delayBetween = 0.5f;

        float offset = 5.625f; ;

        for (int i = 1; i <= repeatCount; i++)
        {
            if (i % 2 == 1)
            {
                // 홀수(1,3,5) → 그대로 발사
                Debug.Log($"[50% 패턴] {i}번째 발사 → rotateOffset = 0");
                yield return StartCoroutine(Phase50Pattern(0f));
            }
            else
            {
                // 짝수(2,4) → 방향을 약간 틀어서 발사
                Debug.Log($"[50% 패턴] {i}번째 발사 → rotateOffset = {offset}");
                yield return StartCoroutine(Phase50Pattern(offset));
            }

            yield return new WaitForSeconds(delayBetween);
        }

        isPatternMode = false;
    }

    // 25% 패턴 반복
    IEnumerator Phase25Loop()
    {
        isPatternMode = true;

        int repeat = 5;
        float delayBetween = 0f;

        int count = 64;
        float angleStep = 360f / count;
        float offset = angleStep * 0.5f;


        for (int i = 1; i <= repeat; i++)
        {
            if(i % 2 == 1)
            {
                yield return StartCoroutine(Phase25Pattern(0f));
            }
            else
            {
                yield return StartCoroutine(Phase25Pattern(offset));
            }
            

            yield return new WaitForSeconds(delayBetween);
        }

        isPatternMode = false;
    }

    // 25% 패턴
    IEnumerator Phase25Pattern(float rotateOffset = 0f)
    {
        Debug.Log("Boss 25% 패턴");



        int count = 64;           // 전체 발사 횟수
        float interval = 0.01f;   // 투사체 간격 (느리게 돌리려면 0.1~0.2 추천)
        float angle = rotateOffset;         // 초기 각도
        float angleStep = 360f / count;  // 매 발사마다 회전하는 각도

        for (int i = 0; i < count; i++)
        {
            // 현재 angle을 기준으로 방향 벡터 계산
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ).normalized;

            ShootCustomProjectile(dir);

            // 다음 발사를 위해 각도 증가
            angle += angleStep;

            yield return new WaitForSeconds(interval);
        }
    }

    void ShootCustomProjectile(Vector2 dir)
    {
        GameObject prefab = patternprojectilePrefab != null ? patternprojectilePrefab : projectilePrefab;

        if (prefab == null)
            return;

        Transform spawnT = patternprojectileSpawnPoint != null ? patternprojectileSpawnPoint : transform;

        Vector3 spawnPos = spawnT.position;

        GameObject projObj = Instantiate(
            prefab,
            spawnPos,  
            Quaternion.identity
        );

        GolemProjectile proj = projObj.GetComponent<GolemProjectile>();
        if (proj != null)
        {
            int dmg = (patternprojectileDamage > 0) ? patternprojectileDamage : projectileDamage;
            float speed = (patternprojectileSpeed > 0f) ? patternprojectileSpeed : projectileSpeed * 0.75f;

            proj.damage = dmg;
            
            proj.Launch(dir, speed * 0.75f);
        }
    }




    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && isAttacking)
        {
            Damaged();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && isAttacking)
        {
            Damaged();
        }
    }

    void Damaged()
    {
        if (hasDealtDamageThisAttack) return;

        hasDealtDamageThisAttack = true;

        PlayerInteract player = target.GetComponent<PlayerInteract>();
        if (player != null)
        {
            bool damaged = player.TakeDamage(Power);

            if (damaged)
            {
                hasDealtDamageThisAttack = true;
            }
        }
    }

    void Dead()
    {
        if (!isLive) return;

        isLive = false;

        OnEnemyDead?.Invoke(this);

        if (isBoss)
        {
            Enemy_Boss_Hp bosshpUI = FindAnyObjectByType<Enemy_Boss_Hp>();
            if (bosshpUI != null)
            {
                bosshpUI.gameObject.SetActive(false);
            }
        }

        StartCoroutine(DieRoutine());
    }
}
