using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Settings
{
    public static class PlayerSettings
    {
        // Could also have strings -> use PlayerSettings.stringname
        //public const string MouseSensitivity = "MouseSensitivity";
        //public const string Key2 = "Key2";

        public enum FloatKeys
        {
            // Mouse settings
            MouseSensitivity,
            FireButton,
            AltFirebutton
        }

        public enum IntKeys
        {
            // Quality settings
            ResolutionWidth,
            ResolutionHeight
        }

        public static void SaveFloat(FloatKeys key, float value)
        {
            // Convert enum key to string
            string strKey = key.ToString();

            // Save to registry
            PlayerPrefs.SetFloat(strKey, value);
            PlayerPrefs.Save();
        }

        public static float GetFloat(FloatKeys key, float defaultValue)
        {
            // Convert enum key to string
            string strKey = key.ToString();

            return PlayerPrefs.GetFloat(strKey, defaultValue);
        }
    }
}


// Clean saved keys once done with testing?
//PlayerPrefs.DeleteAll();