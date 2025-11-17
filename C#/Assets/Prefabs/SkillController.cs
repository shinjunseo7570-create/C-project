using UnityEngine;

public class SkillController : MonoBehaviour
{
    private float moveSpeed;
    private float maxRange;
    private float damage;

    private Vector3 startPosition; // 발사 시작 위치

    public float Damage => damage;
    public ElementType Element => element;

    private ElementType element = ElementType.None;



    public void Init(float _speed, float _range, float _damage)
    {
        this.moveSpeed = _speed;
        this.maxRange = _range;
        this.damage = _damage;

        // 시작 위치 저장 (사거리 계산용)
        this.startPosition = transform.position;

        // 사거리에 안 닿아도 10초 뒤엔 무조건 파괴
        Destroy(gameObject, 10f);
    }

    public void Init(AttackModeData data)
    {
        this.moveSpeed = data.projectileSpeed;
        this.maxRange = data.range;
        this.damage = data.damage;
        this.element = data.element;

        this.startPosition = transform.position;

        Destroy(gameObject, 10f);
    }

    void Update()
    {
        
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);

        // 사거리 체크
        float distance = Vector3.Distance(startPosition, transform.position);
        if (distance >= maxRange)
        {
            Destroy(gameObject); // 사거리 벗어나면 파괴
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

            //자신을 파괴
            Destroy(gameObject);
        }
        /*else if (other.CompareTag("Wall")) // 벽에 부딪혀도 파괴하고 싶다면 추가
        {
            Destroy(gameObject);
        }*/
    }
}
