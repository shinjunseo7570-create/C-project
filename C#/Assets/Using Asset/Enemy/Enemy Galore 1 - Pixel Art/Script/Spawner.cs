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
    }

    void OnDisable()
    {
        Enemy.OnEnemyDead -= HandleEnemyDead;
    }

    void HandleEnemyDead(Enemy enemy)
    {
        aliveCount--;


        if (aliveCount <= 0 && bossSpawned)
        {
            currentRound++;
            ResetRoundState();
        }
    }

    void ResetRoundState()
    {
        spawnedCount = 0;
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
        if(maxCanSpawn <= 0)
        {
            return;
        }

        int groupCount = data.groupCount;
        if(groupCount > maxCanSpawn)
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

        for(int i = 0; i < groupCount; i++)
        {
            GameObject enemyObj = poolManager.Get(data.spriteType);
            Enemy enemy = enemyObj.GetComponent<Enemy>();

            Vector3 pos = basepos;

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

        aliveCount++;
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
    public int Speed;
    public float Range; // 사정거리
    public bool RangedType; // 적 공격유형 근접 false, 원거리 true
    public int groupCount = 1;
}

[System.Serializable]
public class RoundData
{
    public SpawnData[] mobSpawnDatas;
    public int mobCount = 100;
    public SpawnData bossSpawnData;
}

