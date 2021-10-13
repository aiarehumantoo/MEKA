﻿using TMPro;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

using Utilities.DeveloperConsole.Commands;

namespace Utilities.DeveloperConsole
{
    public class DeveloperConsoleBehaviour : MonoBehaviour
    {
        [SerializeField] private string prefix = string.Empty;
        [SerializeField] private ConsoleCommand[] commands = new ConsoleCommand[0];

        [Header("UI")]
        [SerializeField] private GameObject uiCanvas = null;
        [SerializeField] private TMP_InputField inputField = null;

        private float pausedTimeScale;

        private static DeveloperConsoleBehaviour instance;
        private DeveloperConsole developerConsole; // Store
        private DeveloperConsole DeveloperConsole // Getter
        {
            get
            {
                // Create new if null
                if (developerConsole != null)
                {
                    return developerConsole;
                }
                return developerConsole = new DeveloperConsole(prefix, commands);
            }
        }

        private void Awake()
        {
            // Make sure only single instance exists
            if (instance != null && instance != this)
            {
                // ie. new scene -> Destroy new console
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Toggle(CallbackContext context)
        {
            // if (input.keydown) (old system)
            if (!context.action.triggered)
            {
                return;
            }

            if (uiCanvas.activeSelf)
            {
                Time.timeScale = pausedTimeScale;
                uiCanvas.SetActive(false);
            }
            else
            {
                pausedTimeScale = Time.timeScale;
                Time.timeScale = 0;
                uiCanvas.SetActive(true);
                inputField.ActivateInputField();
            }
        }

        public void ProcessCommand(string inputValue)
        {
            DeveloperConsole.ProcessCommand(inputValue);
            inputField.text = string.Empty;
        }
    }
}