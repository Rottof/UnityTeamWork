using UnityEngine;

namespace FPS.Scripts.AI
{
    public class SpawnerManager : MonoBehaviour
    {
        public static SpawnerManager Instance;

        public Transform player;

        private SpawnPoint[] spawnPoints;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            spawnPoints = FindObjectsOfType<SpawnPoint>();

            foreach (var sp in spawnPoints)
            {
                sp.Init(player);
            }
        }
    }
}