using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public RoundData[] rounds;

    public PoolManager poolManager;

    int currentRound = 0;

    float timer;
    public float spawnDelay = 2f;

    int spawnedCount = 0;
    int aliveCount = 0;

    bool bossSpawned = false;
    bool isSpawning = true;

    void Awake()
    {
        Transform[] points = GetComponentsInChildren<Transform>();

        spawnPoint = new Transform[points.Length - 1];

        for (int i = 1; i < points.Length; i++)
        {
            spawnPoint[i - 1] = points[i];
        }
    }

    void OnEnable()
    {
        Enemy.OnEnemyDead += HandleEnemyDead;
        Enemy.OnBossPhase75 += HandleBossPhase75;
    }

    void OnDisable()
    {
        Enemy.OnEnemyDead -= HandleEnemyDead;
        Enemy.OnBossPhase75 -= HandleBossPhase75;
    }

    void HandleBossPhase75(Enemy boss)
    {
        if (currentRound >= rounds.Length) return;
        RoundData round = rounds[currentRound];

        StartCoroutine(SpawnBossPhase75mob(round, boss));
    }

    // 75% 패턴
    IEnumerator SpawnBossPhase75mob(RoundData round, Enemy boss)
    {
        if (round.phase75data == null || round.phase75data.Length == 0 || round.phase75count <= 0)
        {
            Debug.Log("[Boss75] 설정 오류");
            yield break;
        }

        for(int i = 0; i < round.phase75count; i++)
        {
            int mobIndex = Random.Range(0, round.phase75data.Length);
            SpawnData data = round.phase75data[mobIndex];

            // 2) 보스 주변 랜덤 위치
            Vector3 basePos = boss.transform.position;
            basePos += new Vector3(Random.Range(-2f, 2f), -1f, 0f);

            

            // 3) 기존과 똑같이 PoolManager 통해 가져와서 Init
            GameObject enemyObj = poolManager.Get(data.spriteType);
            Enemy enemy = enemyObj.GetComponent<Enemy>();

            if (enemy == null)
            {
                Debug.LogError($"[Boss75 Spawn ERROR] Enemy 컴포넌트 없음! spriteType={data.spriteType}, objName={enemyObj.name}");
                enemyObj.SetActive(false);
                continue;
            }

            enemyObj.transform.position = basePos;

            enemy.isBoss = false;   
            enemy.Init(data);

            // 4) aliveCount 올려줘야 나중에 감소할 때 맞게 계산됨
            aliveCount++;

            yield return new WaitForSeconds(spawnDelay);
        }

    }

    void HandleEnemyDead(Enemy enemy)
    {
        
        aliveCount--;

        
        if (enemy.isBoss)
        {
            currentRound++;
            ResetRoundState();
        }
    }

    void ResetRoundState()
    {
        spawnedCount = 0;
        aliveCount = 0;
        bossSpawned = false;
        isSpawning = true;
        timer = 0f;
    }

    void Update()
    {
        if (currentRound >= rounds.Length)
        {
            Ending();
            return;
        }

        timer += Time.deltaTime;

        RoundData round = rounds[currentRound];

        // 1) 몹 소환 중
        if (isSpawning)
        {
            if (spawnedCount >= round.mobCount)
            {
                isSpawning = false;
                return;
            }

            if (timer >= spawnDelay)
            {
                timer = 0f;
                SpawnMob(round);
            }
        }
        // 2) 몹 다 소환했고, 아직 보스 안 나왔을 때
        else if (!bossSpawned)
        {
            if (aliveCount <= 0)
            {
                SpawnBoss(round);
            }
        }
    }

    void SpawnMob(RoundData round)
    {
        int mobIndex = Random.Range(0, round.mobSpawnDatas.Length);
        SpawnData data = round.mobSpawnDatas[mobIndex];

        int maxCanSpawn = round.mobCount - spawnedCount;
        if (maxCanSpawn <= 0)
        {
            return;
        }

        int groupCount = data.groupCount;
        if (groupCount > maxCanSpawn)
        {
            groupCount = maxCanSpawn;
        }



        int rand = Random.Range(0, spawnPoint.Length);
        Vector3 basepos = spawnPoint[rand].position;
        basepos.x = Mathf.Clamp(basepos.x, -8f, 8f);
        basepos.y = Mathf.Clamp(basepos.y, -4f, 4f);

        Vector3 playerPos = GameManager.instance.player.transform.position;
        float minDistance = 2f;
        if (Vector3.Distance(basepos, playerPos) < minDistance)
            return;

        for (int i = 0; i < groupCount; i++)
        {
            GameObject enemyObj = poolManager.Get(data.spriteType);
            Enemy enemy = enemyObj.GetComponent<Enemy>();

            if (enemy == null)
            {
                Debug.LogError($"[Spawn ERROR] Enemy 컴포넌트 없음! spriteType={data.spriteType}, objName={enemyObj.name}");
                enemyObj.SetActive(false); // 혹시 화면에 떠 버리면 끄기
                return;  // aliveCount / spawnedCount 올리지 말고 탈출
            }

            enemyObj.transform.position = basepos;

            enemy.isBoss = false;
            enemy.Init(data);

            aliveCount++;
            spawnedCount++;
        }








        //Debug.Log($"[SpawnMob] Round {currentRound}, spriteType = {round.mobSpawnData.spriteType}");
    }

    void SpawnBoss(RoundData round)
    {
        GameObject boss = GameManager.instance.pool.Get(round.bossSpawnData.spriteType);

        Vector3 pos = new Vector3(0f, 0f, 0f);
        boss.transform.position = pos;

        Enemy enemyComp = boss.GetComponent<Enemy>();
        enemyComp.isBoss = true;
        enemyComp.Init(round.bossSpawnData);

        
        bossSpawned = true;

        // Debug.Log($"[SpawnBoss] Round {currentRound}, spriteType = {round.bossSpawnData.spriteType}");
    }

    void Ending()
    {
        Debug.Log($"Game Clear!");
        return;
    }
}

[System.Serializable]
public class SpawnData
{
    public int spriteType;
    public int Health;
    public int Attack;
    public float Speed;
    public float Range; // 사정거리
    public int groupCount = 1;
}

[System.Serializable]
public class RoundData
{
    public SpawnData[] mobSpawnDatas;
    public int mobCount = 100;
    public SpawnData bossSpawnData;

    [Header("Boss 75% Phase")]
    public SpawnData[] phase75data;
    public int phase75count = 0;
}

