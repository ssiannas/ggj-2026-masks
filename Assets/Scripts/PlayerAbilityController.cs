using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ggj_2026_masks
{
    public class PlayerAbilityController : MonoBehaviour
    {
        [SerializeField] List<MaskAbility> abilities;
        private Dictionary<MaskAbility, float> _abilityCooldowns = new();
        private int _currentAbility = 0;
        private void Awake()
        {
            foreach (var ability in abilities)
            {
                _abilityCooldowns[ability] = 0f;
            }
        }

        public MaskAbility GetFirst()
        {
            return abilities[0];
        }

        public MaskAbility GetNext()
        {
            var abilityIdx = _currentAbility + 1;
            if (abilityIdx >= abilities.Count)
            {
                abilityIdx = 0;
                return abilities[0];
            }
            return abilities[abilityIdx];
        }

        private void Update()
        {
            foreach (var ability in abilities)
            {
                if (_abilityCooldowns[ability] > 0f)
                    _abilityCooldowns[ability] -= Time.deltaTime;
            }
        }
         
        public bool IsReady(MaskAbility ability)
        {
            return _abilityCooldowns.TryGetValue(ability, out var cd) && cd <= 0f;
        }

        public bool TryExecute(MaskAbility ability, AbilityContext ctx)
        {
            if (!IsReady(ability)) return false;

            ability.Execute(ctx);
            _abilityCooldowns[ability] = ability.GetCooldown();
            return true;
        }

        public void ActivateAbility(MaskAbility ability)
        {
            var idx = abilities.IndexOf(ability);
        }
        
        public float GetCooldownRemaining(MaskAbility ability)
        {
            return _abilityCooldowns.TryGetValue(ability, out var cd) ? Mathf.Max(0f, cd) : 0f;
        }

        public float GetCooldownPercent(MaskAbility ability)
        {
            if (!_abilityCooldowns.TryGetValue(ability, out var cd)) return 1f;
            return 1f - Mathf.Clamp01(cd / ability.GetCooldown());
        }
        
    }
}
