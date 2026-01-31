using ggj_2026_masks;
using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] private HealthBarController healthBarController;

    public void SetHealthPercentage(float pct)
    {
        healthBarController.SetHealthPercentage(pct);
    }
}
