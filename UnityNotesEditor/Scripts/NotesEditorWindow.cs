using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class NotesEditorWindow :EditorWindow
{
   // Supporting Classes
   public NotesEditorRenderer Renderer { get; private set; }
   public NotesEditorFunctions Functions { get; private set; }

   // User defined individualized settings object (public static for accessibility)
   public static NotesSettings CachedSettings { get; set; }

   // Editor window properties (public for accessibility by other classes)
   public Vector2 ScrollPosition { get; set; }
   public string[] NotesCollectionPaths { get; set; }
   public int SelectedNotesCollectionIndex { get; set; }
   public string[] ScriptFilePaths { get; set; }

   // Collection being edited (as a property to update the UI immediately)
   private NotesCollection _currentNotesCollection;
   public NotesCollection CurrentNotesCollection
   {
      get => _currentNotesCollection;
      set
      {
         _currentNotesCollection = value;
         Repaint(); // Refresh the UI if the collection changes
      }
   }

   // UI state properties (public for accessibility)
   public bool AllNotesExpanded { get; set; }
   public PriorityLevel? SelectedPriorityFilter { get; set; }
   public NoteCategory? SelectedCategoryFilter { get; set; }
   public NoteStatus? SelectedStatusFilter { get; set; }

   // UI colors and icons (public for accessibility and immediate update on change)
   public Dictionary<NoteCategory, Color> CategoryColors { get; set; }
   public Dictionary<PriorityLevel, Texture2D> PriorityIcons { get; set; }


   // Initialization of the window and loading of resources
   [MenuItem("Tools/Notes Editor (Window)")]
   public static void ShowWindow()
   {
      GetWindow<NotesEditorWindow>("Notes Editor");
   }

   //TODO: NOTES: This is my tasky task.

   private void OnEnable()
   {
      // Utilize the Tool Classes
      Renderer = new NotesEditorRenderer(this);
      Functions = new NotesEditorFunctions(this);

      CacheNotesSettings();
      UpdateNotesCollectionsList();
      InitializeCategoryColors();
      InitializePriorityIcons();
   }

   private void CacheNotesSettings()
   {
      if ( CachedSettings != null )
         return;

      string filter = "t:NotesSettings";
      string[] guids = AssetDatabase.FindAssets(filter);

      foreach ( var guid in guids )
      {
         string path = AssetDatabase.GUIDToAssetPath(guid);
         CachedSettings = AssetDatabase.LoadAssetAtPath<NotesSettings>(path);
         if ( CachedSettings != null )
            break;
      }
   }

   public void UpdateNotesCollectionsList()
   {
      if ( CachedSettings == null || string.IsNullOrEmpty(CachedSettings.notesFolderPath) )
      {
         Debug.LogWarning("CachedSettings is null or notesFolderPath is empty.");
         return;
      }

      string[] guids = AssetDatabase.FindAssets("t:NotesCollection", new[] { CachedSettings.notesFolderPath });
      NotesCollectionPaths = guids.Select(AssetDatabase.GUIDToAssetPath)
                                  .Select(System.IO.Path.GetFileNameWithoutExtension)
                                  .ToArray();
   }



   // Initialize the category color mapping
   private void InitializeCategoryColors()
   {
      CategoryColors = new Dictionary<NoteCategory, Color>()
        {
            { NoteCategory.TODO, Color.cyan },
            { NoteCategory.Bug, Color.red },
            { NoteCategory.Feature, Color.green },
            { NoteCategory.Improvement, Color.yellow },
            { NoteCategory.Design, Color.magenta },
            { NoteCategory.Testing, Color.grey },
            { NoteCategory.Documentation, Color.blue },
            { NoteCategory.Other, Color.white }
        };
   }

   // Load priority level icons from assets
   private void InitializePriorityIcons()
   {
      if ( CachedSettings != null )
      {
         PriorityIcons = new Dictionary<PriorityLevel, Texture2D>()
        {
            {PriorityLevel.Low, (CachedSettings.lowPriorityIcon) },
            { PriorityLevel.Medium, (CachedSettings.mediumPriorityIcon) },
            { PriorityLevel.High, (CachedSettings.highPriorityIcon) },
            { PriorityLevel.Critical, (CachedSettings.criticalPriorityIcon) }
        };
      }
   }

   // Main GUI rendering logic
   private void OnGUI()
   {
      Renderer.InitializeWindowRendering();
   }

}
