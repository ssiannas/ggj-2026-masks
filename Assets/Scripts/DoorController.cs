using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ggj_2026_masks
{
    public class DoorController : MonoBehaviour, IInteractible
    {
        // Triggers when door opens
        public UnityEvent onOpen;
        
        // State
        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            private set
            {
                _isOpen = value;
                onOpen.Invoke();
            }
        }

        private void Awake()
        {
            onOpen.AddListener(HandleOpenChanged);
        }

        // Check and apply interact
        public void TryInteract()
        {
            var shouldOpen = GameConditionsChecker.Instance.AllEnemiesDead();

            if (shouldOpen)
            {
                IsOpen = true;
            }
            else
            {
                Debug.Log($"Opening door: {gameObject.name} failed!");
            }
        }

        private void HandleOpenChanged()
        {
            Debug.Log($"Interacted with: {gameObject.name}");
        }
    }
}