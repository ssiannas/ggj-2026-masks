using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace ggj_2026_masks
{
    public class AbilityContext : MonoBehaviour
    {
        [field: SerializeField] [CanBeNull] public Transform RopeOrigin { get; private set; }
        public Rigidbody RigidBody { get; private set; }
        public Transform Transform { get; private set; }
        public Animator Animator { get; private set; }

        private void Awake()
        {
            RigidBody = GetComponent<Rigidbody>();
            Transform = GetComponent<Transform>();
            Animator = GetComponent<Animator>();
        }

        public void PerformCoroutine(IEnumerator routine)
        {
            StartCoroutine(routine);
        }
    }
}