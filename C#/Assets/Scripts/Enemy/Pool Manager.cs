using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // 프리펩들을 보관할 변수
    public GameObject[] prefabs;

    // 풀을 담당할 리스트들
    List<GameObject>[] pools;

    void Awake()
    {
        pools = new List<GameObject>[prefabs.Length];

        for (int index = 0; index < pools.Length; index++)
        {
            pools[index] = new List<GameObject>();
        }
    }

    // 게임 오브젝트를 반환하는 함수 선언
    public GameObject Get(int index)
    {
        GameObject select = null;

        // 1) 이미 만들어진 것 중에서 비활성화 된 애 찾기
        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                select = item;
                break;
            }
        }

        // 2) 못 찾으면 새로 생성
        if (select == null)
        {
            select = Instantiate(prefabs[index], transform);
            pools[index].Add(select);
        }

        // 3) 공통 초기화
        select.SetActive(true);

        // 🔍 여기서 Enemy 체크 (디버그용)
        Enemy enemy = select.GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError($"[Pool ERROR] index {index} 프리팹에 Enemy 컴포넌트가 없음! prefabName={prefabs[index].name}, objName={select.name}");
        }

        return select;
    }
}
