using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DroneStrikers.Game.Player
{
    public class GameInputHandler : MonoBehaviour
    {
        private static GameInputActions _inputActions;
        /// <summary>
        ///     Singleton instance of the game's input actions.
        /// </summary>
        public static GameInputActions InputActions => _inputActions ??= new GameInputActions();

        private void OnEnable()
        {
            InputActions.Game.Enable();
            InputActions.Game.Quit.started += OnQuitPressed;
        }

        private void OnDisable()
        {
            InputActions.Game.Disable();
            InputActions.Game.Quit.started -= OnQuitPressed;
        }

        private static void OnQuitPressed(InputAction.CallbackContext context)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}