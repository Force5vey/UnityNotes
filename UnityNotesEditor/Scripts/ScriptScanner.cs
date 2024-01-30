using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class ScriptScanner :EditorWindow
{
   //TODO: IMPORT: This is my tasky task

   private NotesCollection todoScanner;  // Collection for storing scanned TODO notes.
   private NotesCollection mainCollection;  // Reference to the main notes collection in NotesEditorWindow.

   public static NotesSettings CachedSettings { get; set; }

   // Layout settings
   private float labelWidth = 75f;
   private float categoryBarWidth = 150f;
   private float categoryBarHeight = 2;

   // Scroll position for the notes list scroll view.
   public Vector2 ScrollPosition { get; set; }

   // UI color mappings for note categories.
   public Dictionary<NoteCategory, Color> CategoryColors { get; set; }

   public bool AllNotesExpanded { get; set; }
   public bool AllNotesSelected { get; set; }

   public static void ShowWindow()
   {
      GetWindow<ScriptScanner>("TODO Scanner");
   }

   private void OnEnable()
   {
      todoScanner = ScriptableObject.CreateInstance<NotesCollection>();
      CacheNotesSettings();
      InitializeCategoryColors();
   }

   private void OnGUI()
   {
      RenderTopBar();
      RenderControlBar();
      RenderTodoNotesList();
   }

   public void SetMainCollection( NotesCollection collection )
   {
      mainCollection = collection;
   }

   /// <summary>
   /// Get cached NotesSettings for use in script.
   /// </summary>
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

   /// <summary>
   /// Iterates through all .cs files in project and retrieves comment lines starting with //TODO
   /// </summary>
   private void ScanForTodoComments()
   {
      var taggedComments = new List<TaggedComment>();

      foreach ( string tag in CachedSettings.commentTags )
      {
         var regex = new Regex(Regex.Escape(tag));

         string[] allCsFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
         foreach ( string file in allCsFiles )
         {
            string[] lines = File.ReadAllLines(file);
            for ( int i = 0; i < lines.Length; i++ )
            {
               if ( regex.IsMatch(lines[i]) )
               {
                  taggedComments.Add(new TaggedComment
                  {
                     FilePath = file,
                     LineNumber = i + 1,
                     TodoText = lines[i].Trim(),
                     TagUsed = tag
                  });
               }
            }
         }
      }
      PopulateTodoScanner(taggedComments);
   }


   /// <summary>
   /// Populate Todo Scanner with the found TODOs.
   /// </summary>
   /// <param name="todoItems"></param>
   private void PopulateTodoScanner( List<TaggedComment> taggedComments )
   {
      if ( taggedComments == null )
         return;

      todoScanner.notes.Clear(); // Clear existing items

      foreach ( var taggedComment in taggedComments )
      {
         string relativePath = taggedComment.FilePath.Replace(Application.dataPath, "Assets");

         Note newNote = new Note
         {
            title = $"{taggedComment.TagUsed}: {Path.GetFileName(taggedComment.FilePath)}: Line {taggedComment.LineNumber}",
            text = taggedComment.TodoText,
            fileName = relativePath,
            lineNumber = taggedComment.LineNumber,
            isSelected = false
            // Set other Note properties as needed
         };

         todoScanner.notes.Add(newNote);
      }

      EditorUtility.SetDirty(todoScanner); // Mark the collection as dirty to save changes
   }

   /// <summary>
   /// Import selected TODO's to the Main Notes Collection
   /// </summary>
   private void ImportSelectedTodoNotes()
   {
      if ( mainCollection == null )
      {
         Debug.LogError("MainCollection is null in TodoScannerWindow");
         return;
      }

      foreach ( var note in todoScanner.notes.Where(n => n.isSelected) )
      {
         mainCollection.notes.Add(note); // Add the note to the main collection
      }

      EditorUtility.SetDirty(mainCollection); // Mark the main collection as dirty to save changes
   }

   /// <summary>
   /// Initialize the category color mapping.
   /// </summary>
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

   /// <summary>
   /// Render the top bar displaying function buttons.
   /// </summary>
   private void RenderTopBar()
   {
      GUILayout.BeginHorizontal();
      if ( GUILayout.Button("Scan Scripts for Tags", GUILayout.Width(150)) )
      {
         ScanForTodoComments();
      }
      if ( GUILayout.Button("Import Selected", GUILayout.Width(150)) )
      {
         ImportSelectedTodoNotes();
      }

      EditorGUILayout.LabelField("   Settings:", GUILayout.Width(75));
      if ( GUILayout.Button("Edit Tags", GUILayout.Width(150)) )
      {
         OpenNotesSettings();
      }
      GUILayout.EndHorizontal();
   }

   private void RenderControlBar()
   {
      GUILayout.BeginHorizontal();

      if ( GUILayout.Button(AllNotesExpanded ? "Collapse" : "Expand", GUILayout.Width(75)) )
      {
         ToggleAllNotes();
      }

      if ( GUILayout.Button(AllNotesSelected ? "Deselect All" : "Select All", GUILayout.Width(100)) )
      {
         SetAllNotesSelected();
      }


      GUILayout.EndHorizontal();
   }


   /// <summary>
   /// Render the list of notes with foldouts and detailed editing fields.
   /// </summary>
   public void RenderTodoNotesList()
   {
      if ( todoScanner == null )
         return;

      GUILayout.Label("Notes:", EditorStyles.boldLabel);
      ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);

      for ( int i = 0; i < todoScanner.notes.Count; i++ )
      {
         RenderNoteItem(todoScanner.notes[i]);
      }

      EditorGUILayout.EndScrollView();
   }

   /// <summary>
   /// Iterates through the collection and renders each note item.
   /// </summary>
   /// <param name="note"></param>
   private void RenderNoteItem( Note note )
   {
      GUILayout.BeginHorizontal(); // Begin a horizontal layout for the foldout and completed checkbox

      // Checkbox for "Completed" next to the foldout
      RenderCompletedCheckbox(note);

      // Foldout for the note title
      note.isExpanded = EditorGUILayout.Foldout(note.isExpanded, note.title, true);

      GUILayout.EndHorizontal();

      if ( note.isExpanded )
      {
         RenderNoteDetails(note);
      }
   }

   // Render the checkbox for marking a note as completed
   private void RenderCompletedCheckbox( Note note )
   {
      bool isSelected = EditorGUILayout.Toggle(note.isSelected, GUILayout.Width(20));
      if ( isSelected != note.isSelected )
      {
         note.isSelected = isSelected;
         EditorUtility.SetDirty(todoScanner);
      }
   }

   // Render the detailed fields for a note
   private void RenderNoteDetails( Note note )
   {
      EditorGUI.indentLevel++;
      GUILayout.BeginVertical("box");

      RenderNoteCategoryColor(note);
      RenderNoteTitleRow(note);
      RenderCategoryAndDateRow(note);
      RenderStatusAndPriorityRow(note);
      RenderNoteTextField(note);

      GUILayout.EndVertical();
      EditorGUI.indentLevel--;
   }

   // Render the category color bar for a note
   private void RenderNoteCategoryColor( Note note )
   {
      float rectWidth = categoryBarWidth;
      Rect rect = GUILayoutUtility.GetRect(rectWidth, categoryBarHeight, GUILayout.ExpandWidth(false));
      EditorGUI.DrawRect(rect, CategoryColors[note.category]);
      GUILayout.Space(5);
   }

   ///<summary>
   /// Render the title field for a note
   /// </summary>
   private void RenderNoteTitleRow( Note note )
   {
      GUILayout.BeginHorizontal();

      EditorGUILayout.LabelField("Title", GUILayout.Width(labelWidth));
      note.title = EditorGUILayout.TextField(note.title);

      GUILayout.EndHorizontal();
   }

   /// <summary>
   /// Render the Category Drop-down and Date Field.
   /// </summary>
   private void RenderCategoryAndDateRow( Note note )
   {
      GUILayout.BeginHorizontal();

      EditorGUILayout.LabelField("Category", GUILayout.Width(labelWidth));
      note.category = (NoteCategory)EditorGUILayout.EnumPopup(note.category);

      EditorGUILayout.LabelField("Date", GUILayout.Width(50));
      note.creationDate = EditorGUILayout.TextField(note.creationDate);

      GUILayout.EndHorizontal();
   }

   /// <summary>
   /// Render the Status and Priority Drop-downs.
   /// </summary>
   private void RenderStatusAndPriorityRow( Note note )
   {
      GUILayout.BeginHorizontal();

      EditorGUILayout.LabelField("Status", GUILayout.Width(labelWidth));
      note.status = (NoteStatus)EditorGUILayout.EnumPopup(note.status);

      EditorGUILayout.LabelField("Priority", GUILayout.Width(labelWidth));
      note.priority = (PriorityLevel)EditorGUILayout.EnumPopup(note.priority);

      GUILayout.EndHorizontal();
   }

   /// <summary>
   /// Render Notes Area.
   /// </summary>
   private void RenderNoteTextField( Note note )
   {
      EditorGUILayout.LabelField("Note Text");

      GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea)
      {
         wordWrap = true
      };

      note.text = EditorGUILayout.TextArea(note.text, textAreaStyle, GUILayout.MinHeight(60), GUILayout.ExpandHeight(true));
   }

   private void SetAllNotesExpanded( bool expanded )
   {
      foreach ( var note in todoScanner.notes )
      {
         note.isExpanded = expanded;
      }
   }

   private void SetAllNotesSelected()
   {
      AllNotesSelected = !AllNotesSelected;
      foreach ( var note in todoScanner.notes )
      {
         note.isSelected = AllNotesSelected;
      }
   }

   /// <summary>
   /// Toggle the expansion state of all notes.
   /// </summary>
   public void ToggleAllNotes()
   {
      if ( todoScanner != null )
      {
         AllNotesExpanded = !AllNotesExpanded;
         foreach ( var note in todoScanner.notes )
         {
            note.isExpanded = AllNotesExpanded;
         }
      }
   }

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
