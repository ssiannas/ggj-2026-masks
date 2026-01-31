using JetBrains.Annotations;
using UnityEngine;

namespace ggj_2026_masks
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private float interactionRadius = 2.0f;

        [CanBeNull] private IInteractible _currentTarget;

        public bool HasTarget => _currentTarget != null;

        void Update()
        {
            _currentTarget = FindClosestInteractible();
        }

        [CanBeNull]
        private IInteractible FindClosestInteractible()
        {
            var colliders = Physics.OverlapSphere(transform.position, interactionRadius);

            IInteractible closest = null;
            float closestDistance = interactionRadius;

            foreach (var col in colliders)
            {
                if (col.TryGetComponent<IInteractible>(out var interactible))
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closest = interactible;
                        closestDistance = distance;
                    }
                }
            }

            return closest;
        }

        public void TryInteract()
        {
            _currentTarget?.TryInteract();
        }
    }
}
