using UnityEngine;
using UnityEngine.Events;

namespace Custom.Utility
{
    public enum CauseState
    {
        PAUSE_BUTTON = 0,
        MAP_BUTTON = 1,
        MINI_GAME = 2,
        CUT_SCENE = 3,
        END_GAME = 4
    }

    public class GameState : MonoBehaviour
    {
        public static GameState Instance = null;

        public static bool m_crouchIsToggle = false;
        public static bool m_runIsToggle = false;   

        public void Awake()
        {
            Instance = this;

            PauseGameLock.Reset();

            IsGamePaused = false;
            Time.timeScale = 1;
        }


        #region PauseDetails
        public static bool IsGamePaused { get; private set; }

        /// <summary>
        /// Whether or not the game can be paused
        /// </summary>
        private static BitLock PauseGameLock = new BitLock();

        public UnityEvent m_onGamePaused;
        public UnityEvent m_onGameUnpaused;

        /// <summary>
        /// Pause the game (stop time) using the CauseState.PAUSE_BUTTON reason
        /// </summary>
        public void PauseGame()
        {
            PauseGame((int)CauseState.PAUSE_BUTTON);
        }
        /// <summary>
        /// Unpause the game (un-stop time) using the CauseState.PAUSE_BUTTON reason
        /// Will only unpause if paused by the same CauseState
        /// </summary>
        public void UnpauseGame()
        {
            UnpauseGame((int)CauseState.PAUSE_BUTTON);
        }
        /// <summary>
        /// Pauses the game and sets Time.timeScale
        /// </summary>
        /// <param name="pauseState">The cause for pausing</param>
        /// <param name="timeScale">The time scale to set the game at while paused in this manner</param>
        public static void PauseGame(CauseState pauseState, float timeScale = 0)
        {
            PauseGame((int)pauseState, timeScale);
        }
        /// <summary>
        /// Unpauses the game based on the CauseState given
        /// Will only unpause if the game is currently paused by the same CauseState
        /// </summary>
        /// <param name="pauseState">The reason for unpausing</param>
        public static void UnpauseGame(CauseState pauseState)
        {
            UnpauseGame((int)pauseState);
        }

        public static void PauseGame(int pauseIndex, float timeScale = 0)
        {
            OnLock(PauseGameLock, pauseIndex, () =>
            {
                IsGamePaused = true;
                Time.timeScale = timeScale;
                Instance?.m_onGamePaused.Invoke();
            });
        }
        public static void UnpauseGame(int pauseIndex)
        {
            OnUnlock(PauseGameLock, pauseIndex, () =>
            {
                IsGamePaused = false;
                Time.timeScale = 1;
                Instance?.m_onGameUnpaused.Invoke();
            });
        }
        #endregion

        /// <summary>
        /// Unlock the nth bit in a BitLock and then invoke an action if it is fully unlocked (if it actually started as locked by this bit)
        /// </summary>
        /// <param name="bitLock">BitLock to unlock</param>
        /// <param name="unlockBit">Bit to unlock</param>
        /// <param name="onUnlock">Action to perform if unlocked</param>
        private static void OnUnlock(BitLock bitLock, int unlockBit, System.Action onUnlock)
        {
            if (bitLock.IsBitSet(unlockBit))
            {
                bitLock.UnlockBit(unlockBit);

                if (!bitLock.IsLocked)
                {
                    onUnlock?.Invoke();
                }
            }
        }
        /// <summary>
        /// Locks a the nth bit of a BitLock and then performs an action, does a preliminary check to see if the BitLock is already locked or not
        /// </summary>
        /// <param name="bitLock">The BitLock to lock</param>
        /// <param name="lockBit">The nth bit to lock</param>
        /// <param name="onLock">Action to perform if locked successfully</param>
        private static void OnLock(BitLock bitLock, int lockBit, System.Action onLock)
        {
            if (!bitLock.IsLocked)
            {
                onLock?.Invoke();

                bitLock.LockBit(lockBit);
            }
        }
    }
}