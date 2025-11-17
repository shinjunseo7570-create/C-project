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

        for(int index = 0; index < pools.Length; index++)
        {
            pools[index] = new List<GameObject>();
        }
    }

    // 게임 오브젝트를 반환하는 함수 선언
    public GameObject Get(int index)
    {
        GameObject select = null;

        // 선택한 풀의 놀고 있는 게임 오브젝트 접근
        

        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                // 발견하면 select 변수에 할당
                select = item;
                select.SetActive(true);
                break;
            }
        }

        // 못 찾으면 새롭게 생성하고 select 변수에 할당
        if (!select)
        {
            select = Instantiate(prefabs[index], transform);
            select.SetActive(true);
            pools[index].Add(select);
        }

        return select;
    }
}
