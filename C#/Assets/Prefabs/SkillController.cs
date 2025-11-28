using UnityEngine;

public class SkillController : MonoBehaviour
{
    private float moveSpeed;
    private float maxRange;
    private float damage;

    private Vector3 startPosition; // 발사 시작 위치

    public float Damage => damage;
    public ElementType Element => element;

    private ElementType element = ElementType.Fire;

    public AttackType AttackType;
   
    private bool isAreaActive = false;

    public void Init(AttackModeData data)
    {
        this.moveSpeed = data.projectileSpeed;
        this.maxRange = data.range;
        this.damage = data.damage;
        this.element = data.elementType;

        this.AttackType = data.attackType;

        this.startPosition = transform.position;

        // Area 생성되자마자 고정
        if (this.AttackType == AttackType.Area)
        {
            isAreaActive = true; 
            Destroy(gameObject, 0.2f); // 생성 0.2초 후 파괴
        }
        else
        {
            // 다른 타입은 못 맞추면 20초 뒤 파괴
            Destroy(gameObject, 20f);
        }
    }

    public void Init(float _speed, float _range, float _damage)
    {
        this.moveSpeed = _speed;
        this.maxRange = _range;
        this.damage = _damage;

        // 시작 위치 저장 (사거리 계산용)


        this.startPosition = transform.position;

        // 사거리에 안 닿아도 10초 뒤엔 무조건 파괴
        Destroy(gameObject, 20f);
    }

    void Update()
    {
        if (isAreaActive) return;
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);

        // 사거리 체크
        float distance = Vector3.Distance(startPosition, transform.position);
        if (distance >= maxRange)
        {
            if (AttackType == AttackType.Area)
            {
                // Area 타입
                // 파괴되지 않고, 그 자리에서 멈춰서 0.2초간 유지됨
                isAreaActive = true;
                Destroy(gameObject, 0.2f);
            }
            else
            {
                // 나머지 타입
                // 사거리를 벗어나면 즉시 파괴
                Destroy(gameObject);
            }
        }
    }

    // 충돌
    void OnTriggerEnter2D(Collider2D other)
    {
        // 'Enemy' 태그를 가진 오브젝트와 부딪혔을 때
        if (other.CompareTag("Enemy"))
        {
            // (추후 구현) 몬스터에게 데미지 주기
            // other.GetComponent<EnemyScript>().TakeDamage(damage);
            Debug.Log($"몬스터 명중! 데미지: {damage}");

            switch (AttackType)
            {
                case AttackType.Melee:
                    // Melee 관통                 
                    // 최대 사거리에서 파괴
                    break;

                case AttackType.Area:
                    // Area 설치형                                    
                    break;

                default:
                    // Normal, Snipe
                    // 맞자마자 즉시 파괴
                    Destroy(gameObject);
                    break;
            }
        }
    }
}
