// (C) MMOARgames, Inc. All Rights Reserved.

using UnityEditor;

namespace MMOARgames.Editor
{
    [InitializeOnLoad]
    public class EnforceEditorSettings
    {
        static EnforceEditorSettings()
        {
            #region Editor Settings

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                EditorSettings.serializationMode = SerializationMode.ForceText;
                UnityEngine.Debug.Log("Setting Force Text Serialization");
            }

            if (EditorSettings.externalVersionControl != "Visible Meta Files")
            {
                EditorSettings.externalVersionControl = "Visible Meta Files";
                UnityEngine.Debug.Log("Updated external version control mode: " + EditorSettings.externalVersionControl);
            }

            #endregion

            #region Player Settings

            if (!PlayerSettings.companyName.Equals("MMOARgames, Inc."))
            {
                PlayerSettings.companyName = "MMOARgames, Inc.";
                UnityEngine.Debug.Log("Updated Player Settings Company Name: " + PlayerSettings.companyName);
            }

            if (PlayerSettings.SplashScreen.show)
            {
                PlayerSettings.SplashScreen.show = false;
                UnityEngine.Debug.Log("Disabling Unity Spash Screen");
            }

            if (PlayerSettings.apiCompatibilityLevel != ApiCompatibilityLevel.NET_2_0)
            {
                PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
                UnityEngine.Debug.Log("Updated .NET compatibility to 2.0");
            }

            #endregion
        }
    }
}