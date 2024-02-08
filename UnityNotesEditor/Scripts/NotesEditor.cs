using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System;
using Unity.VisualScripting;

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

   // Editor Tab properties
   public Vector2 EditorScrollPosition { get; set; }
   public string[] NotesCollectionPaths { get; set; }
   public int SelectedNotesCollectionIndex { get; set; }
   public string[] ScriptFilePaths { get; set; }

   // Script Scanner Tab properties
   public Vector2 ScriptScannerScrollPosition { get; set; }

   // Collection being edited
   private NotesCollectionDefinition _currentNotesCollection;
   public NotesCollectionDefinition CurrentNotesCollection
   {
      get => _currentNotesCollection;
      set
      {
         _currentNotesCollection = value;
         Repaint(); // Refresh the UI if the collection changes
      }
   }

   private NotesCollectionDefinition _foundTaggedCommentsCollection;
   public NotesCollectionDefinition FoundTaggedCommentsCollection
   {
      get => _foundTaggedCommentsCollection;
      set
      {
         _foundTaggedCommentsCollection = value;
         Repaint();
      }
   }

   // UI state properties
   public bool AllNotesExpanded { get; set; }
   public bool AllNotesSelected { get; set; }

   //Display Filters.
   public PriorityLevel? SelectedPriorityFilter { get; set; }
   public NoteCategory? SelectedCategoryFilter { get; set; }
   public NoteStatus? SelectedStatusFilter { get; set; }

   // UI colors and icons
   public Dictionary<NoteCategory, Color> CategoryColors { get; set; }
   public Dictionary<PriorityLevel, Texture2D> PriorityIcons { get; set; }


   // Initialization of the window and loading of resources
   [MenuItem("Window/Notes Editor")]
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

      FoundTaggedCommentsCollection = ScriptableObject.CreateInstance<NotesCollectionDefinition>();

      CacheNotesSettings();
      UpdateNotesCollectionsList();
      CategoryColors = StyleKit.InitializeCategoryColors();
      InitializePriorityIcons();

      if ( CachedSettings != null && CachedSettings.currentCollectionIndex >= 0 && CachedSettings.currentCollectionIndex < NotesCollectionPaths.Length )
      {
         SelectedNotesCollectionIndex = CachedSettings.currentCollectionIndex;
         Functions.LoadSelectedNotesCollection();
      }
   }

   private void OnGUI()
   {
      //RenderTabButtonRow();

      // Get TabNames
      string[] tabNames = Enum.GetNames(typeof(EditorTab));
      GUILayout.Space(5);
      Rect rect = GUILayoutUtility.GetRect(1, StyleKit.BarHeightSmall, GUILayout.ExpandWidth(true));
      EditorGUI.DrawRect(rect, Color.black);


      GUILayout.BeginHorizontal();

      for ( int i = 0; i < tabNames.Length; i++ )
      {
         GUIStyle buttonStyle = (CurrentTab == (EditorTab)i) ? StyleKit.MainToolbarButtonSelected : StyleKit.MainToolbarButton;
         if ( GUILayout.Button(tabNames[i], buttonStyle, GUILayout.Width(StyleKit.ButtonWidthLarge)) )
         {
            CurrentTab = (EditorTab)i;
         }
      }
      GUILayout.EndHorizontal();

      //rect = GUILayoutUtility.GetRect(1, StyleKit.BarHeightMedium, GUILayout.ExpandWidth(true));
      //EditorGUI.DrawRect(rect, Color.black);
      GUILayout.Space(5);

      // Handle the rendering based on the current tab
      switch ( CurrentTab )
      {
         case EditorTab.Notes:
         Renderer.InitializeWindowRendering();
         break;
         case EditorTab.ScriptScanner:
         SSRenderer.InitializeScriptScannerRendering();
         break;
         case EditorTab.Settings:

         // Render Settings Tab Content
         break;
      }

      //Required (as of now) for mouse hover effects.
      Repaint();


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
      StyleKit.Initialize(CachedSettings.notesFolderPath);
   }

   public void UpdateNotesCollectionsList()
   {
      if ( CachedSettings == null || string.IsNullOrEmpty(CachedSettings.notesFolderPath) )
      {
         Debug.LogWarning("CachedSettings is null or notesFolderPath is empty.");
         return;
      }

      string[] guids = AssetDatabase.FindAssets("t:NotesCollectionDefinition", new[] { CachedSettings.notesFolderPath });
      NotesCollectionPaths = guids.Select(AssetDatabase.GUIDToAssetPath)
                                  .Select(System.IO.Path.GetFileNameWithoutExtension)
                                  .ToArray();

      CachedSettings.allNotesCollectionPaths = new List<string>(NotesCollectionPaths);
      EditorUtility.SetDirty(CachedSettings);
   }

   // Load priority level icons from assets
   private void InitializePriorityIcons()
   {
      PriorityIcons = new Dictionary<PriorityLevel, Texture2D>()
        {
            {PriorityLevel.Low, (CachedSettings.lowPriorityIcon) },
            { PriorityLevel.Medium, (CachedSettings.mediumPriorityIcon) },
            { PriorityLevel.High, (CachedSettings.highPriorityIcon) },
            { PriorityLevel.Critical, (CachedSettings.criticalPriorityIcon) }
        };
   }

   #endregion

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

  //TODO: move stuff fron ONGUI to here for the tab row
   private void RenderTabButtonRow()
   {
      // Get TabNames
      string[] tabNames = Enum.GetNames(typeof(EditorTab));
      GUILayout.Space(5);
      Rect rect = GUILayoutUtility.GetRect(1, StyleKit.BarHeightSmall, GUILayout.ExpandWidth(true));
      EditorGUI.DrawRect(rect, Color.black);


      GUILayout.BeginHorizontal();
      for ( int i = 0; i < tabNames.Length; i++ )
      {
         GUIStyle buttonStyle = (CurrentTab == (EditorTab)i) ? StyleKit.MainToolbarButtonSelected : StyleKit.MainToolbarButton;
         if ( GUILayout.Button(tabNames[i], StyleKit.MainToolbarButton, GUILayout.Width(StyleKit.ButtonWidthLarge)) )
         {
            CurrentTab = (EditorTab)i;
         }
      }
      GUILayout.EndHorizontal();

      //rect = GUILayoutUtility.GetRect(1, StyleKit.BarHeightMedium, GUILayout.ExpandWidth(true));
      //EditorGUI.DrawRect(rect, Color.black);
      GUILayout.Space(5);
   }

}
