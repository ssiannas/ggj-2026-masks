using System.Collections;
using UnityEngine;

namespace ggj_2026_masks
{
    public class AbilityContext : MonoBehaviour
    {
        public Rigidbody RigidBody {get; private set;}
        public Transform Transform {get; private set;}
        public Animator Animator {get; private set;}

        private void Awake()
        {
            RigidBody = GetComponent<Rigidbody>();
            Transform = GetComponent<Transform>();
            Animator = GetComponent<Animator>();
        }
        
        public void StartCoroutine(IEnumerator routine)
        {
            ((MonoBehaviour)this).StartCoroutine(routine);
        } 
    }
}
