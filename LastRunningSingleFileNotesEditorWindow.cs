using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class NotesEditorWindow :EditorWindow
{
   //User defined individualized settings object
   private static NotesSettings cachedSettings = null;

   // Editor window properties
   private Vector2 scrollPosition;
   private string notesCollectionFolderPath = "";
   private string[] notesCollectionPaths;
   private int selectedNotesCollectionIndex = 0;
   private string[] scriptFilePaths;

   // Collection being edited
   private NotesCollection currentNotesCollection;

   // UI state properties
   private bool allNotesExpanded = false;
   private PriorityLevel selectedPriorityFilter = PriorityLevel.Low;
   private NoteCategory? selectedCategoryFilter = null;

   // UI colors and icons
   private Dictionary<NoteCategory, Color> categoryColors;
   private Dictionary<PriorityLevel, Texture2D> priorityIcons;

   // Initialization of the window and loading of resources
   [MenuItem("Tools/Notes Editor (Window)")]
   public static void ShowWindow()
   {
      GetWindow<NotesEditorWindow>("Notes Editor");
   }

   private void OnEnable()
   {
      CacheNotesSettings();
      InitializeCategoryColors();
      InitializePriorityIcons();
   }

   private void CacheNotesSettings()
   {
      if ( cachedSettings != null )
         return;

      string filter = "t:NotesSettings";
      string[] guids = AssetDatabase.FindAssets(filter);

      foreach ( var guid in guids )
      {
         string path = AssetDatabase.GUIDToAssetPath(guid);
         cachedSettings = AssetDatabase.LoadAssetAtPath<NotesSettings>(path);
         if ( cachedSettings != null )
            break;
      }

      notesCollectionFolderPath = cachedSettings.notesFolderPath;
   }



   // Initialize the category color mapping
   private void InitializeCategoryColors()
   {
      categoryColors = new Dictionary<NoteCategory, Color>()
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
      if ( cachedSettings != null )
      {
         priorityIcons = new Dictionary<PriorityLevel, Texture2D>()
        {
            {PriorityLevel.Low, (cachedSettings.lowPriorityIcon) },
            { PriorityLevel.Medium, (cachedSettings.mediumPriorityIcon) },
            { PriorityLevel.High, (cachedSettings.highPriorityIcon) },
            { PriorityLevel.Critical, (cachedSettings.criticalPriorityIcon) }
        };
      }
   }

   // Main GUI rendering logic
   private void OnGUI()
   {
      RenderNotesCollectionField();
      RenderSelectNotesAndRefreshButton();
      RenderAddRemoveButtons();
      RenderFilterControls();
      RenderNotesList();
   }

   // Render the field for selecting or dragging the NotesCollection
   private void RenderNotesCollectionField()
   {
      GUILayout.Label("Notes Collection", EditorStyles.boldLabel);

   }


   // Render dropdown for selecting a NotesCollection
   private void RenderSelectNotesAndRefreshButton()
   {
      GUILayout.BeginHorizontal();

      if ( notesCollectionPaths != null && notesCollectionPaths.Length > 0 )
      {
         EditorGUI.BeginChangeCheck();
         selectedNotesCollectionIndex = EditorGUILayout.Popup("Select Notes", selectedNotesCollectionIndex, notesCollectionPaths);
         if ( EditorGUI.EndChangeCheck() )
         {
            currentNotesCollection = AssetDatabase.LoadAssetAtPath<NotesCollection>(notesCollectionPaths[selectedNotesCollectionIndex]);
         }
      }

      if ( GUILayout.Button("Refresh") )
      {
         RefreshNotesCollection();
      }

      currentNotesCollection = (NotesCollection)EditorGUILayout.ObjectField(currentNotesCollection, typeof(NotesCollection), false);

      GUILayout.EndHorizontal();
   }

   // Render buttons for adding new notes and removing completed notes
   private void RenderAddRemoveButtons()
   {
      GUILayout.BeginHorizontal();

      if ( GUILayout.Button("Add New Note") )
      {
         AddNewNote();
      }

      if ( GUILayout.Button("Remove Completed Notes") )
      {
         RemoveCompletedNotes();
      }

      GUILayout.EndHorizontal();
   }

   // Render the filter controls for priority and category
   private void RenderFilterControls()
   {
      GUILayout.BeginHorizontal();

      if ( GUILayout.Button(allNotesExpanded ? "Collapse All" : "Expand All", GUILayout.Width(150)) )
      {
         ToggleAllNotes();
      }

      if ( GUILayout.Button("Apply Filter:  ") )
      {
         ApplyFilterAndSort();
      }

      RenderPriorityFilter();
      RenderCategoryFilter();

      GUILayout.EndHorizontal();
   }

   // Render priority filter dropdown
   private void RenderPriorityFilter()
   {
      EditorGUILayout.LabelField("Priority", GUILayout.MaxWidth(110));
      selectedPriorityFilter = (PriorityLevel)EditorGUILayout.EnumPopup(selectedPriorityFilter, GUILayout.MaxWidth(100));
   }

   // Render category filter dropdown
   private void RenderCategoryFilter()
   {
      EditorGUILayout.LabelField("Category", GUILayout.MaxWidth(110));
      string[] categoriesWithAll = Enum.GetNames(typeof(NoteCategory)).Prepend("All").ToArray();
      int selectedCategoryIndex = selectedCategoryFilter.HasValue ? (int)selectedCategoryFilter.Value + 1 : 0;
      selectedCategoryIndex = EditorGUILayout.Popup(selectedCategoryIndex, categoriesWithAll, GUILayout.MaxWidth(100));
      selectedCategoryFilter = selectedCategoryIndex > 0 ? (NoteCategory?)(selectedCategoryIndex - 1) : null;
   }

   // Render the list of notes with foldouts and detailed editing fields
   private void RenderNotesList()
   {
      if ( currentNotesCollection == null )
         return;

      GUILayout.Label("Notes:", EditorStyles.boldLabel);
      scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

      for ( int i = 0; i < currentNotesCollection.notes.Count; i++ )
      {
         RenderNoteItem(currentNotesCollection.notes[i]);
      }

      EditorGUILayout.EndScrollView();
   }

   // Render a single note item
   private void RenderNoteItem( Note note )
   {
      GUILayout.BeginHorizontal(); // Begin a horizontal layout for the foldout and completed checkbox

      // Checkbox for "Completed" next to the foldout
      RenderCompletedCheckbox(note);

      // Foldout for the note title
      note.isExpanded = EditorGUILayout.Foldout(note.isExpanded, note.title ?? "New Note", true);

      GUILayout.EndHorizontal();

      if ( note.isExpanded )
      {
         RenderNoteDetails(note);
      }
   }

   // Render the checkbox for marking a note as completed
   private void RenderCompletedCheckbox( Note note )
   {
      bool completed = EditorGUILayout.Toggle(note.completed, GUILayout.Width(20));
      if ( completed != note.completed )
      {
         note.completed = completed;
         MarkNotesCollectionDirty();
      }
   }

   // Render the detailed fields for a note
   private void RenderNoteDetails( Note note )
   {
      EditorGUI.indentLevel++;
      GUILayout.BeginVertical("box");

      RenderNoteCategoryColor(note);
      RenderNoteTitleAndCategory(note);
      RenderPriorityDropdown(note);
      RenderFileNameField(note);
      RenderLineNumberFieldAndOpenButton(note);
      RenderNoteTextField(note);

      GUILayout.EndVertical();
      EditorGUI.indentLevel--;
   }

   // Render the category color bar for a note
   private void RenderNoteCategoryColor( Note note )
   {
      float rectWidth = 150f;
      Rect rect = GUILayoutUtility.GetRect(rectWidth, 2, GUILayout.ExpandWidth(false));
      EditorGUI.DrawRect(rect, categoryColors[note.category]);
      GUILayout.Space(5);
   }

   // Render the title and category fields for a note
   private void RenderNoteTitleAndCategory( Note note )
   {
      note.title = EditorGUILayout.TextField("Title", note.title);
      GUILayout.BeginHorizontal();
      note.category = (NoteCategory)EditorGUILayout.EnumPopup("Category", note.category);
      note.creationDate = EditorGUILayout.TextField("Creation Date", note.creationDate);
      GUILayout.EndHorizontal();
   }

   // Render the priority dropdown and icon for a note
   private void RenderPriorityDropdown( Note note )
   {
      GUILayout.BeginHorizontal();

      note.status = (NoteStatus)EditorGUILayout.EnumPopup("Status", note.status);


      note.priority = (PriorityLevel)EditorGUILayout.EnumPopup("Priority", note.priority);
      GUILayout.Label(priorityIcons[note.priority], GUILayout.Width(30), GUILayout.Height(20));
      GUILayout.EndHorizontal();
   }

   // Render the file selection and drag-and-drop field for a note
   private void RenderFileNameField( Note note )
   {
      GUILayout.BeginHorizontal();

      if ( scriptFilePaths == null )
      {
         scriptFilePaths = GetAllScriptFiles();
      }
      int selectedIndex = Array.IndexOf(scriptFilePaths, note.fileName);
      int newSelectedIndex = EditorGUILayout.Popup("File", selectedIndex, scriptFilePaths, GUILayout.ExpandWidth(true));
      if ( newSelectedIndex >= 0 && newSelectedIndex < scriptFilePaths.Length )
      {
         note.fileName = scriptFilePaths[newSelectedIndex];
      }

      // Drag and drop field
      UnityEngine.Object droppedObject = EditorGUILayout.ObjectField(
          AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(note.fileName),
          typeof(UnityEngine.Object),
          false,
          GUILayout.Width(200)
      );
      if ( droppedObject != null )
      {
         string droppedObjectPath = AssetDatabase.GetAssetPath(droppedObject);
         if ( droppedObjectPath.EndsWith(".cs") && scriptFilePaths.Contains(droppedObjectPath) )
         {
            note.fileName = droppedObjectPath;
         }
      }


      //GameObject drag drop field


      GameObject linkedGameObject = (GameObject)EditorGUILayout.ObjectField("Linked GameObject",
                               note.linkedGameObject, typeof(GameObject), true);

      if ( linkedGameObject != null )
      {
         note.linkedGameObjectPath = GetGameObjectPath(linkedGameObject);
         note.linkedSceneName = linkedGameObject.scene.name;
      }
      else
      {
         note.linkedGameObjectPath = null;
         note.linkedSceneName = null;
      }


      GUILayout.EndHorizontal();
   }



   // Render the line number field for a note
   private void RenderLineNumberFieldAndOpenButton( Note note )
   {
      GUILayout.BeginHorizontal();

      note.lineNumber = EditorGUILayout.IntField("Line Number", note.lineNumber);

      if ( GUILayout.Button("Open File") )
      {
         OpenScriptFile(note);

      }

      GUILayout.EndHorizontal();
   }

   // Render the text area for the note content
   private void RenderNoteTextField( Note note )
   {
      EditorGUILayout.LabelField("Note Text");
      note.text = EditorGUILayout.TextArea(note.text, GUILayout.MinHeight(60), GUILayout.ExpandHeight(true));
   }

   // Mark the NotesCollection asset as dirty to ensure changes are saved
   private void MarkNotesCollectionDirty()
   {
      if ( currentNotesCollection != null )
      {
         EditorUtility.SetDirty(currentNotesCollection);
      }
   }

   // Refresh the list of NotesCollection assets
   private void RefreshNotesCollection()
   {
      string[] guids = AssetDatabase.FindAssets("t:NotesCollection", new[] { notesCollectionFolderPath });
      notesCollectionPaths = guids.Select(AssetDatabase.GUIDToAssetPath)
                                  .Select(System.IO.Path.GetFileNameWithoutExtension)
                                  .ToArray();

      if ( notesCollectionPaths.Length > 0 )
      {
         currentNotesCollection = AssetDatabase.LoadAssetAtPath<NotesCollection>(AssetDatabase.GUIDToAssetPath(guids[0]));
      }
   }

   // Add a new note to the current collection
   private void AddNewNote()
   {
      if ( currentNotesCollection != null )
      {
         currentNotesCollection.notes.Add(new Note { creationDate = System.DateTime.Now.ToString("dd_MMM - HH:mm") });
         MarkNotesCollectionDirty();
      }
   }

   // Remove all completed notes from the collection
   private void RemoveCompletedNotes()
   {
      if ( currentNotesCollection != null )
      {
         currentNotesCollection.notes = currentNotesCollection.notes.Where(note => !note.completed).ToList();
         MarkNotesCollectionDirty();
      }
   }

   // Toggle the expansion state of all notes
   private void ToggleAllNotes()
   {
      if ( currentNotesCollection != null )
      {
         allNotesExpanded = !allNotesExpanded;
         foreach ( var note in currentNotesCollection.notes )
         {
            note.isExpanded = allNotesExpanded;
         }
      }
   }

   // Apply filtering and sorting to the notes collection
   private void ApplyFilterAndSort()
   {
      if ( currentNotesCollection == null )
         return;

      IEnumerable<Note> filteredNotes = FilterNotesByPriorityAndCategory();

      // Sort by priority
      filteredNotes = filteredNotes.OrderBy(note => note.priority);

      // Update the notes collection to show filtered and sorted notes on top
      UpdateNotesCollection(filteredNotes);

      // Reflect changes in the editor window
      Repaint();
   }

   // Filter notes based on the selected priority and category
   private IEnumerable<Note> FilterNotesByPriorityAndCategory()
   {
      IEnumerable<Note> filteredNotes = currentNotesCollection.notes;

      if ( selectedCategoryFilter.HasValue )
      {
         filteredNotes = filteredNotes.Where(note => note.category == selectedCategoryFilter.Value);
      }

      filteredNotes = filteredNotes.Where(note => note.priority == selectedPriorityFilter);

      return filteredNotes;
   }

   // Update the notes collection to reflect the filtered and sorted notes
   private void UpdateNotesCollection( IEnumerable<Note> filteredNotes )
   {
      var filteredNotesList = filteredNotes.ToList();
      foreach ( var note in filteredNotesList )
      {
         note.isExpanded = true; // Expand filtered notes
      }
      var remainingNotes = currentNotesCollection.notes.Except(filteredNotesList).ToList();
      foreach ( var note in remainingNotes )
      {
         note.isExpanded = false; // Collapse remaining notes
      }

      currentNotesCollection.notes = filteredNotesList.Concat(remainingNotes).ToList();
   }

   // Open a script file at a specific line number
   private void OpenScriptFile( Note note )
   {
      if ( !string.IsNullOrEmpty(note.fileName) )
      {
         UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(note.fileName);
         AssetDatabase.OpenAsset(asset, note.lineNumber);
      }

      if ( !string.IsNullOrEmpty(note.linkedGameObjectPath) && !string.IsNullOrEmpty(note.linkedSceneName) )
      {
         Scene scene = SceneManager.GetSceneByName(note.linkedSceneName);
         if ( scene.IsValid() )
         {
            GameObject linkedGameObject = FindGameObjectInScene(scene, note.linkedGameObjectPath);
            if ( linkedGameObject != null )
            {
               MonoBehaviour[] scripts = linkedGameObject.GetComponents<MonoBehaviour>();
               foreach ( var script in scripts )
               {
                  MonoScript monoScript = MonoScript.FromMonoBehaviour(script);
                  AssetDatabase.OpenAsset(monoScript);
               }
            }
         }
      }
   }

   // Get a list of all script files within the project
   private string[] GetAllScriptFiles()
   {
      string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
      return allAssetPaths.Where(path => path.StartsWith("Assets/") && path.EndsWith(".cs")).ToArray();
   }

   private string GetGameObjectPath( GameObject obj )
   {
      return obj != null ? AnimationUtility.CalculateTransformPath(obj.transform, null) : null;
   }

   private GameObject FindGameObjectInScene( Scene scene, string path )
   {
      foreach ( GameObject obj in scene.GetRootGameObjects() )
      {
         Transform foundTransform = obj.transform.Find(path);
         if ( foundTransform != null )
            return foundTransform.gameObject;
      }
      return null;
   }
}
