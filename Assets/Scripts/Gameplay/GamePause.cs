using UnityEngine;

namespace KANYE.Gameplay
{
    public static class GamePause
    {
        public static bool Locked { get; private set; }

        public static void Lock()
        {
            Locked = true;
            Time.timeScale = 0f;
        }

        public static void Release()
        {
            Locked = false;
            Time.timeScale = 1f;
        }
    }
}
