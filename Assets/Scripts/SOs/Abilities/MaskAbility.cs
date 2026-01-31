using UnityEngine;

namespace ggj_2026_masks
{
    public abstract class MaskAbility : ScriptableObject
    {
        public Sprite activeSprite;
        public Sprite inactiveSprite;
        
        public abstract void Execute(AbilityContext context);
        public abstract float GetCooldown();
    }
}
