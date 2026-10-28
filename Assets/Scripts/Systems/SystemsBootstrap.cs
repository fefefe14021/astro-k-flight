using UnityEngine;

namespace KANYE.Systems
{
    public static class SystemsBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            CreateService<SaveManager>();
            CreateService<AchievementManager>();
        }

        private static void CreateService<T>() where T : Component
        {
            if (Object.FindAnyObjectByType<T>() != null)
                return;

            var service = new GameObject(typeof(T).Name);
            service.AddComponent<T>();
        }
    }
}
