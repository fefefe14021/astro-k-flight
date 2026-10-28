using UnityEngine;

namespace KANYE.Systems
{
    [CreateAssetMenu(fileName = "Achievement", menuName = "KANYE/Achievement")]
    public class AchievementData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public int targetAmount = 1;
    }
}
