using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public Rigidbody2D target;
    public int Power = 1;

    

    public bool isBoss = false;

    public static Action<Enemy> OnEnemyDead;

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

        switch(typeId)
        {
            case 4: // 가재
                switch(atkType)
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

        float distance = Vector2.Distance(target.position, rigid.position);
        attackTimer += Time.fixedDeltaTime;

        // 1) 공격 중이면: 자리에서 멈추고 애니메이션만 재생
        if (isAttacking)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        // 3) 기본 상태: 항상 플레이어 쪽으로 날아간다
        Chase();

        // 4) 사정거리 안 + 쿨타임 끝났으면 공격 시작
        if (distance <= attackRange && attackTimer >= attackDelay)
        {
            attackTimer = 0f;
            StartCoroutine(AttackRoutine());
        }
    }

    // 평소에 플레이어를 향해 날아가는 동작
    void Chase()
    {
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);

        // 평소 상태는 공격 아님
        anim.SetBool("isAttack", false);
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

        // 공격 시작: 이동 멈추고, 공격 애니메이션 ON
        rigid.linearVelocity = Vector2.zero;
        anim.SetBool("isAttack", true);


        // 공격 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(attackAnimDuration);

        // 공격 끝 → 다시 평상시 상태로
        anim.SetBool("isAttack", false);
        isAttacking = false;
    }

    IEnumerator DieRoutine()
    {
        isAttacking = false;

        if(rigid != null)
        {
            rigid.linearVelocity = Vector2.zero;
        }

        if(col != null)
        {
            col.enabled = false;
        }

        if(anim != null)
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
        maxHealth = health;  // 혹시 maxHealth가 따로 세팅되어 있으면 이 줄 조절
        health = maxHealth;

        attackTimer = 0f;
        isAttacking = false;
        spawnTime = Time.time;
    }

    public void Init(SpawnData data)
    {
        typeId = data.spriteType;


        speed = data.Speed;
        maxHealth = data.Health;
        health = data.Health;
        attackRange = data.Range;
        Power = data.Attack;

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

            if(health > 0)
            {
                if(anim != null)
                {
                    anim.SetTrigger("Hit");
                }
            }
            else
            {
                Dead();
            }
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

        PlayerInteract player = target.GetComponent<PlayerInteract>();
        if(player != null)
        {
            player.TakeDamage(Power);
        }
    }

    void Dead()
    {
        if (!isLive) return;

        isLive = false;

        OnEnemyDead?.Invoke(this);

        StartCoroutine(DieRoutine());
    }
}
