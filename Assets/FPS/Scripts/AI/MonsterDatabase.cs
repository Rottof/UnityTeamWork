using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.AI
{
    [System.Serializable]
    public class MonsterData
    {
        public int id;
        public GameObject prefab;
    }
    
    [System.Serializable]
    public class SpawnConfig
    {
        public int monsterId;
        public float spawnInterval = 2f;

        [HideInInspector]
        public float timer;
    }

    public class MonsterDatabase : MonoBehaviour
    {
        public static MonsterDatabase Instance;

        public List<MonsterData> monsters = new List<MonsterData>();
        private Dictionary<int, GameObject> monsterDict;

        private void Awake()
        {
            Instance = this;

            monsterDict = new Dictionary<int, GameObject>();
            foreach (var m in monsters)
            {
                if (!monsterDict.ContainsKey(m.id))
                    monsterDict.Add(m.id, m.prefab);
            }
        }

        public GameObject GetMonsterById(int id)
        {
            monsterDict.TryGetValue(id, out var prefab);
            return prefab;
        }
    }
}