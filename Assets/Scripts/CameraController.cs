using Unity.Cinemachine;
using UnityEngine;

namespace ggj_2026_masks
{
    public class CameraController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            var targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

            foreach (var player in players)
            {
                targetGroup.AddMember(player.transform, 1f, 0f);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
