using UnityEngine;
using UnityEditor;
//TODO: NOTE: This is a new one to test.
// Utility class for creating new NotesCollection assets with a specific naming convention
public static class CreateNotesCollection
{
   private static NotesSettings cachedSettings = null;

   [MenuItem("Tools/Create/Note Collection")]
   public static void CreateMyAsset()
   {
      // Create a new instance of NotesCollection
      NotesCollection asset = ScriptableObject.CreateInstance<NotesCollection>();

      // Generate the path and file name for the new asset
      string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{GetFolderPath()}/{GetAssetName()}.asset");

      // Create and save the new asset
      AssetDatabase.CreateAsset(asset, assetPathAndName);
      AssetDatabase.SaveAssets();
      EditorUtility.FocusProjectWindow();
      Selection.activeObject = asset;
   }

   // Get the project-specific folder path from NotesSettings
   private static string GetFolderPath()
   {
      NotesSettings settings = GetNotesSettings();
      return settings != null ? settings.notesFolderPath : "Assets/Editor/UnityNotesEditor/";
   }

   // Utility method to get the NotesSettings instance
   private static NotesSettings GetNotesSettings()
   {
      if ( cachedSettings != null )
         return cachedSettings;

      string filter = "t:NotesSettings";
      string[] guids = AssetDatabase.FindAssets(filter);
      foreach ( var guid in guids )
      {
         string path = AssetDatabase.GUIDToAssetPath(guid);
         cachedSettings = AssetDatabase.LoadAssetAtPath<NotesSettings>(path);
         if ( cachedSettings != null )
            return cachedSettings;
      }

      return null;
   }

   // Generate a unique file name based on the current date and project name
   private static string GetAssetName()
   {
      NotesSettings settings = GetNotesSettings();
      string projectName = settings != null && !string.IsNullOrEmpty(settings.projectName)
         ? settings.projectName
         : PlayerSettings.productName;

      if(settings != null && string.IsNullOrEmpty(settings.projectName))
      {
         settings.projectName = projectName;
         EditorUtility.SetDirty(settings);
      }

      return $"{projectName}_Notes";
   }
}
