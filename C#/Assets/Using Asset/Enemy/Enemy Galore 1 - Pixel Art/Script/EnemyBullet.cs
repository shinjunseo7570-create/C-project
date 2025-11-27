using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 1;
    public float destroyDelay = 0.1f; // 폭발 애니메이션 재생 시간

    bool isHit = false; 

    Animator anim;
    Rigidbody2D rigid;
    Collider2D col;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 이미 폭발 중이거나 화면 밖이면 무시
        if (isHit) return;

        // 플레이어와 충돌
        if (collision.CompareTag("Player"))
        {
            PlayerInteract player = collision.GetComponent<PlayerInteract>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            // 폭발 시작
            StartExplosion();
        }
    }

    void StartExplosion()
    {
        isHit = true; // 중복 충돌 방지

       
        if (rigid != null)
        {
            rigid.linearVelocity = Vector2.zero;
            rigid.simulated = false;
        }

        
        if (col != null)
        {
            col.enabled = false;
        }

        
        if (anim != null)
        {
            anim.SetTrigger("Death");
            
        }


        Destroy(gameObject, destroyDelay);
    }

    // 화면 밖으로 나가면 폭발 없이 그냥 조용히 삭제
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}