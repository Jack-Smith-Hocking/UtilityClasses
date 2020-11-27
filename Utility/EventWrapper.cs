using UnityEngine;
using UnityEngine.SceneManagement;

namespace Custom.Utility
{
    public class EventWrapper : MonoBehaviour
    {
        public void SetTmeScale(float timeScale)
        {
            Time.timeScale = timeScale;
        }

        public void EndGame()
        {
            GameState.PauseGame(CauseState.END_GAME);
        }
        public void PauseGame()
        {
            GameState.PauseGame(CauseState.PAUSE_BUTTON);
        }
        public void UnpauseGame()
        {
            GameState.UnpauseGame(CauseState.PAUSE_BUTTON);
        }

        public void ToggleScript(MonoBehaviour mono)
        {
            if (mono != null)
            {
                mono.enabled = !mono.enabled;
            }
        }

        public void ToggleTarget(GameObject target)
        {
            if (GeneralUtil.IsValid(target))
            {
                target.SetActive(!target.activeInHierarchy);
            }
        }
        public void ToggleSelf()
        {
            ToggleTarget(gameObject);
        }

        public void LoadSceneIndex(int index)
        {
            if (index > 0 && index <= SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(index);
            }
        }
        public void LoadSceneName(string sceneName)
        {
            if (sceneName.Length == 0) return;

            SceneManager.LoadScene(sceneName);
        }

        public void ReloadScene() 
        {
            GameState.UnpauseGame(CauseState.PAUSE_BUTTON);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void DestroyTarget(GameObject target)
        {
            if (GeneralUtil.IsValid(target))
            {
                Destroy(target);
            }
        }

        public void DestroySelf()
        {
            DestroyTarget(gameObject);
        }

        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        public void ConfineCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        public void ResetCursor()
        {
            Cursor.lockState = CursorLockMode.None;
        }
        public void SetCursorVisibility(bool isVisible)
        {
            Cursor.visible = isVisible;
        }
    }
}