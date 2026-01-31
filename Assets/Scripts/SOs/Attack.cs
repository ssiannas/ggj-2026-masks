using System;
using UnityEngine;

namespace ggj_2026_masks
{
    [CreateAssetMenu(fileName = "Attack", menuName = "Scriptable Objects/Attack")]
    public class Attack : ScriptableObject
    {
        [field: SerializeField] public float Damage { get; private set; }
    }
}
