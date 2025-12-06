using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    

    

    

    [SerializeField] float runSpeed = 10;
    Vector2 moveInput;
    Animator myAnimator;
    Rigidbody2D myRigidbody;

    public int maxHp = 10;
    [SerializeField]  public int Hp = 10;

    public GameManager gameManager;

    [Header("Sprint")]
    [SerializeField] float sprintDistance = 10f; // 사거리
    [SerializeField] float sprintDuration = 0.1f; // 무적 판정
    [SerializeField] float sprintCooltime = 2f; // 쿨타임 2초


    bool isSprinting = false;
    public bool IsInvincible { get; private set; } // 무적 판정 확인
    float lastSprintTime = -999f;

    Vector2 lastMoveDir = Vector2.right;


    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        Run();
        FlipSprite();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        /*Debug.Log(moveInput);*/
    }

    void Run()
    {
        if(isSprinting)
        {
            return;
        }

        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, moveInput.y * runSpeed);
        myRigidbody.linearVelocity = playerVelocity;

        bool isMoving = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon || Mathf.Abs(myRigidbody.linearVelocity.y) > Mathf.Epsilon;
        myAnimator.SetBool("IsRunning", isMoving);

        if(isMoving)
        {
            lastMoveDir = moveInput.normalized;
        }
    }

    void OnSprint(InputValue value)
    {
        if (!value.isPressed) return;

        if (isSprinting)
        {
            return;
        }

        if (Time.time < lastSprintTime + sprintCooltime)
        {
            return;
        }

        StartCoroutine(SprintRoutine());
    }

    IEnumerator SprintRoutine()
    {
        isSprinting = true;
        IsInvincible = true;
        lastSprintTime = Time.time;

        Vector2 dir = lastMoveDir;
        if (dir.sqrMagnitude < 0.01f)
        {
            dir = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
        }

        float sprintSpeed = sprintDistance / sprintDuration; // 거=속/시
        float elapsed = 0f;

        myAnimator.SetBool("IsRunning", true);

        while (elapsed < sprintDuration)
        {
            myRigidbody.linearVelocity = dir * sprintSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        isSprinting = false;
        IsInvincible = false;

        
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, moveInput.y * runSpeed);
        myRigidbody.linearVelocity = playerVelocity;
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.linearVelocity.x), 1f);
        }
    }

    public bool TakeDamage(int damage)
    {
        if(IsInvincible)
        {
            return false;
        }

        Hp -= damage;
        Debug.Log("플레이어 데미지!");

        if(Hp <= 0)
        {
            Hp = 0;
            Die();
        }
        return true;
    }

    void Die()
    {
        gameManager.GameOver();
        Debug.Log("GameOver");
    }
}
