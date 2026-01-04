using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("怪物设置")]
    public GameObject monsterPrefab;
    public int maxMonsterCount = 5;
    public float spawnInterval = 2f;

    [Header("刷怪范围")]
    public float activeDistance = 20f;   // 玩家多近才激活

    private Transform player;
    private float timer;

    private bool isActive;

    public void Init(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        isActive = distance <= activeDistance;

        if (!isActive) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            TrySpawn();
            timer = 0f;
        }
    }

    private void TrySpawn()
    {
        

        GameObject obj = Instantiate(
            monsterPrefab,
            transform.position + Random.insideUnitSphere * 2f,
            Quaternion.identity
        );
        
        //刷怪
        
    }

    public void OnDead()
    {
      
    }

    // 方便在 Scene 里看到范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, activeDistance);
    }
}