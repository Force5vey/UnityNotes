using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class NotesEditor :EditorWindow
{
   // Supporting Classes
   public NotesEditorRenderer Renderer { get; private set; }
   public NotesEditorFunctions Functions { get; private set; }
   public ScriptScannerRenderer SSRenderer { get; private set; }
   public ScriptScannerFunctions SSFunctions { get; private set; }

   //Feature Sets
   public enum EditorTab
   {
      Notes,
      ScriptScanner,
      Settings
   }

   public EditorTab CurrentTab { get; set; }

   // NotesEditor Settings for user customization.
   public static NotesSettings CachedSettings { get; set; }

   // Editor window properties (public for accessibility by other classes)
   public Vector2 EditorScrollPosition { get; set; }
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

   private NotesCollection _foundTaggedCommentsCollection;
   public NotesCollection FoundTaggedCommentsCollection
   {
      get => _foundTaggedCommentsCollection;
      set
      {
         _foundTaggedCommentsCollection = value;
         Repaint();
      }
   }


   // UI state properties (public for accessibility)
   public bool AllNotesExpanded { get; set; }
   public bool AllNotesSelected { get; set; }

   //Display Filters.
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
      GetWindow<NotesEditor>("Notes Editor");
   }

   /// <summary>
   /// Primary Initialization that should happen once on opening.
   /// </summary>
   private void OnEnable()
   {
      // Utilize the Tool Classes
      Renderer = new NotesEditorRenderer(this);
      Functions = new NotesEditorFunctions(this);
      SSRenderer = new ScriptScannerRenderer(this);
      SSFunctions = new ScriptScannerFunctions(this);

      //Set current tab to defualt tab that opens
      this.CurrentTab = EditorTab.Notes;

      FoundTaggedCommentsCollection = ScriptableObject.CreateInstance<NotesCollection>();

      CacheNotesSettings();
      UpdateNotesCollectionsList();
      InitializeCategoryColors();
      InitializePriorityIcons();

      if ( CachedSettings != null && CachedSettings.currentCollectionIndex >= 0 && CachedSettings.currentCollectionIndex < NotesCollectionPaths.Length )
      {
         SelectedNotesCollectionIndex = CachedSettings.currentCollectionIndex;
         Functions.LoadSelectedNotesCollection();
      }
   }

   // Fires like Update();
   private void OnGUI()
   {
      // Get TabNames and Render the Tab Toolbar
      string[] tabNames = Enum.GetNames(typeof(EditorTab));
      CurrentTab = (EditorTab)GUILayout.Toolbar((int)CurrentTab, tabNames, GUILayout.Width(300));

      switch ( CurrentTab )
      {
         case EditorTab.Notes:
         Renderer.InitializeWindowRendering();
         break;
         case EditorTab.ScriptScanner:
         SSRenderer.InitialzieScriptScannerRendering();
         break;
         case EditorTab.Settings:

         break;
      }
   }

   private void OnDisable()
   {
      if ( CachedSettings != null && SelectedNotesCollectionIndex >= 0 )
      {
         CachedSettings.currentCollectionIndex = SelectedNotesCollectionIndex;
         EditorUtility.SetDirty(CachedSettings);
      }
   }

   #region // Initialization - OnEnable ...

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

      CachedSettings.allNotesCollectionPaths = new List<string>(NotesCollectionPaths);
      EditorUtility.SetDirty(CachedSettings);
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

   #endregion


   /* ------------------------------------------------------------------------------------------------------------- */




  

   

 




 

   private void SetAllNotesExpanded( bool expanded )
   {
      foreach ( var note in FoundTaggedCommentsCollection.notes )
      {
         note.isExpanded = expanded;
      }
   }



   /// <summary>
   /// Toggle the expansion state of all notes.
   /// </summary>
   public void ToggleAllNotes()
   {
      if ( FoundTaggedCommentsCollection != null )
      {
         AllNotesExpanded = !AllNotesExpanded;
         foreach ( var note in FoundTaggedCommentsCollection.notes )
         {
            note.isExpanded = AllNotesExpanded;
         }
      }
   }


   //NotesRefactor: Won't be needed until I implement settings tab
   private void OpenNotesSettings()
   {
      if ( CachedSettings != null )
      {
         Selection.activeObject = CachedSettings;
      }
      else
      {
         Debug.LogError("Notes settings not found.");
      }
   }


}
