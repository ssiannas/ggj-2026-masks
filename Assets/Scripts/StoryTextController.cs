using UnityEngine;

namespace ggj_2026_masks
{
    public class StoryTextController : MonoBehaviour
    {
        private float timer = 5f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
