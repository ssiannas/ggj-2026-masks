using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ggj_2026_masks
{
    public class GameController : MonoBehaviour
    {
        public UnityEvent onAllPlayersDied;
        private HashSet<PlayerController> _players;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            // Find all players
            _players = new HashSet<PlayerController>(FindObjectsByType<PlayerController>(FindObjectsSortMode.None));
            // Setup player death handlers
            foreach (var player in _players) player.OnPlayerDeath.AddListener(() => HandlePlayerDeath(player));

            onAllPlayersDied.AddListener(HandleAllPlayersDied);
        }

        private void HandlePlayerDeath(PlayerController player)
        {
            Debug.Log($"{player.tag} died!");

            if (GameConditionsChecker.Instance.AllPlayersDead()) onAllPlayersDied.Invoke();
        }


        private void HandleAllPlayersDied()
        {
            Debug.Log("All players died!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}