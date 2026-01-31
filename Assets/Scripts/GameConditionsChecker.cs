using ggj_2026_masks.Enemies;
using UnityEngine;

namespace ggj_2026_masks
{
    public class GameConditionsChecker : MonoBehaviour
    {
        public static GameConditionsChecker Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public bool AllEnemiesDead()
        {
            var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            return enemies.Length == 0;
        }
    }
}