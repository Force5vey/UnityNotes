using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using System;
using System.Linq;

public class NotesEditorRenderer
{
   private NotesEditor notesEditor;

   //Layout settings
   private float labelWidth = 75f;
   private float filterLabelWidth = 40;
   private float enumWidth = 75f;

   private float categoryBarWidth = 150f;
   private float categoryBarHeight = 2;

   private float priorityIconWidth = 30f;
   private float priorityIconHeight = 20;



   public NotesEditorRenderer( NotesEditor notesEditor )
   {
      this.notesEditor = notesEditor;
   }

   #region //Main Window Rendering
   public void InitializeWindowRendering()
   {
      RenderNotesCollection();
      RenderNoteSelectionRow();
      RenderNoteModificationRow();
      RenderFilterRow();
      RenderNotesList();
   }

   public void RenderNotesCollection()
   {
      GUILayout.Label("Notes Collection", EditorStyles.boldLabel);
   }

   public void RenderNoteSelectionRow()
   {
      GUILayout.BeginHorizontal();

      if ( notesEditor.NotesCollectionPaths != null && notesEditor.NotesCollectionPaths.Length > 0 )
      {
         EditorGUILayout.LabelField("Note", GUILayout.Width(labelWidth));
         //notesEditor.SelectedNotesCollectionIndex = EditorGUILayout.Popup(
         //    notesEditor.SelectedNotesCollectionIndex,
         //    notesEditor.NotesCollectionPaths);
      }

      int newIndex = EditorGUILayout.Popup(notesEditor.SelectedNotesCollectionIndex, notesEditor.NotesCollectionPaths);

      if(newIndex != notesEditor.SelectedNotesCollectionIndex)
      {
         notesEditor.SelectedNotesCollectionIndex = newIndex;
        notesEditor.Functions.LoadSelectedNotesCollection();
      }

      if ( GUILayout.Button("Refresh", GUILayout.Width(75)) )
      {
         notesEditor.Functions.LoadSelectedNotesCollection();
      }

      notesEditor.CurrentNotesCollection = (NotesCollection)EditorGUILayout.ObjectField(
          notesEditor.CurrentNotesCollection,
          typeof(NotesCollection), false);

      GUILayout.EndHorizontal();
   }




   private void RenderNoteModificationRow()
   {
      GUILayout.BeginHorizontal();

      if ( GUILayout.Button(notesEditor.AllNotesExpanded ? "Collapse" : "Expand", GUILayout.Width(75)) )
      {
         notesEditor.Functions.ToggleAllNotes(notesEditor.CurrentNotesCollection);
      }

      if ( GUILayout.Button("New Note", GUILayout.Width(75)) )
      {
         notesEditor.Functions.AddNewNote();
      }


      if ( GUILayout.Button(new GUIContent("Delete", "Delete Checked / Completed Notes"), GUILayout.Width(75)) )
      {
         notesEditor.Functions.RemoveCompletedNotes();
      }

      // This used to open the window, should be good to get rid of it since the data
      // is handled in tabs and shares properties / references

      //EditorGUILayout.LabelField("    Tools:", GUILayout.Width(50) );
      //if ( GUILayout.Button("Import", GUILayout.Width(100)) )
      //{
      //   if ( !EditorWindow.HasOpenInstances<ScriptScanner>() )
      //   {
      //      ScriptScanner window = EditorWindow.GetWindow<ScriptScanner>("TODO Scanner");
      //      window.SetMainCollection(notesEditor.CurrentNotesCollection);
      //   }
      //   else
      //   {
      //      ScriptScanner window = (ScriptScanner)EditorWindow.GetWindow(typeof(ScriptScanner), false, "TODO Scanner");
      //      window.SetMainCollection(notesEditor.CurrentNotesCollection);
      //   }
      //}

      GUILayout.EndHorizontal();
   }


   // Render the filter controls for priority and category
   private void RenderFilterRow()
   {
      GUILayout.BeginHorizontal();

      if ( GUILayout.Button(new GUIContent("Filter:", "Apply Filter based on selected Priority, Category, and Status"
         ), GUILayout.Width(75)) )
      {
         notesEditor.Functions.ApplyFilterAndSort();
      }

      // Priority Filter
      EditorGUILayout.LabelField("Pri", GUILayout.MaxWidth(filterLabelWidth));
      var priorities = Enum.GetNames(typeof(PriorityLevel)).AsEnumerable().Prepend("All").ToArray();
      int selectedPriorityIndex = notesEditor.SelectedPriorityFilter.HasValue ? (int)notesEditor.SelectedPriorityFilter.Value + 1 : 0;
      selectedPriorityIndex = EditorGUILayout.Popup(selectedPriorityIndex, priorities, GUILayout.MaxWidth(enumWidth));
      notesEditor.SelectedPriorityFilter = selectedPriorityIndex > 0 ? (PriorityLevel?)(selectedPriorityIndex - 1) : null;

      //Category Filter
      EditorGUILayout.LabelField("Cat", GUILayout.MaxWidth(filterLabelWidth));
      var categories = Enum.GetNames(typeof(NoteCategory)).AsEnumerable().Prepend("All").ToArray();
      int selectedCategoryIndex = notesEditor.SelectedCategoryFilter.HasValue ? (int)notesEditor.SelectedCategoryFilter.Value + 1 : 0;
      selectedCategoryIndex = EditorGUILayout.Popup(selectedCategoryIndex, categories, GUILayout.MaxWidth(enumWidth));
      notesEditor.SelectedCategoryFilter = selectedCategoryIndex > 0 ? (NoteCategory?)(selectedCategoryIndex - 1) : null;

      //status
      EditorGUILayout.LabelField("Stat", GUILayout.MaxWidth(filterLabelWidth));
      var statuses = Enum.GetNames(typeof(NoteStatus)).AsEnumerable().Prepend("All").ToArray();
      int selectedStatusIndex = notesEditor.SelectedStatusFilter.HasValue ? (int)notesEditor.SelectedStatusFilter.Value + 1 : 0;
      selectedStatusIndex = EditorGUILayout.Popup(selectedStatusIndex, statuses, GUILayout.MaxWidth(enumWidth));
      notesEditor.SelectedStatusFilter = selectedStatusIndex > 0 ? (NoteStatus?)(selectedStatusIndex - 1) : null;

      GUILayout.EndHorizontal();
   }

   #endregion

   #region //Individual Notes Foldout

   // Render the list of notes with foldouts and detailed editing fields
   public void RenderNotesList()
   {
      if ( notesEditor.CurrentNotesCollection == null )
         return;

      GUILayout.Label("Notes:", EditorStyles.boldLabel);
      notesEditor.EditorScrollPosition = EditorGUILayout.BeginScrollView(notesEditor.EditorScrollPosition);

      for ( int i = 0; i < notesEditor.CurrentNotesCollection.notes.Count; i++ )
      {
         RenderNoteItem(notesEditor.CurrentNotesCollection.notes[i]);
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
      note.isExpanded = EditorGUILayout.Foldout(note.isExpanded, note.title ?? "...Enter Note Title", true);

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
         notesEditor.Functions.MarkNotesCollectionDirty();
      }
   }

   #endregion

   #region // Note Details

   // Render the detailed fields for a note
   private void RenderNoteDetails( Note note )
   {
      EditorGUI.indentLevel++;
      GUILayout.BeginVertical("box");

      RenderNoteCategoryColor(note);
      RenderNoteTitleRow(note);
      RenderCategoryAndDateRow(note);
      RenderStatusAndPriorityRow(note);
      RenderFileRow(note);
      RenderGameObjectRow(note);
      RenderNoteTextField(note);

      GUILayout.EndVertical();
      EditorGUI.indentLevel--;
   }

   // Render the category color bar for a note
   private void RenderNoteCategoryColor( Note note )
   {
      float rectWidth = categoryBarWidth;
      Rect rect = GUILayoutUtility.GetRect(rectWidth, categoryBarHeight, GUILayout.ExpandWidth(false));
      EditorGUI.DrawRect(rect, notesEditor.CategoryColors[note.category]);
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
      GUILayout.Label(notesEditor.PriorityIcons[note.priority], GUILayout.Width(priorityIconWidth), GUILayout.Height(priorityIconHeight));

      GUILayout.EndHorizontal();
   }

   /// <summary>
   /// Render the File Selection and Line Number
   /// </summary>
   private void RenderFileRow( Note note )
   {
      GUILayout.BeginHorizontal();

      if ( notesEditor.ScriptFilePaths == null )
      {
         notesEditor.ScriptFilePaths = notesEditor.Functions.GetAllScriptFiles();
      }
      int selectedIndex = Array.IndexOf(notesEditor.ScriptFilePaths, note.fileName);
      EditorGUILayout.LabelField("File", GUILayout.Width(labelWidth));

      if ( GUILayout.Button("Open", GUILayout.Width(100)) )
      {
         notesEditor.Functions.OpenScriptFile(note);
      }

      int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, notesEditor.ScriptFilePaths, GUILayout.Width(75));
      if ( newSelectedIndex >= 0 && newSelectedIndex < notesEditor.ScriptFilePaths.Length )
      {
         note.fileName = notesEditor.ScriptFilePaths[newSelectedIndex];
      }

      // Drag and drop field - CS File
      UnityEngine.Object droppedObject = EditorGUILayout.ObjectField(
          AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(note.fileName), typeof(UnityEngine.Object), false);

      if ( droppedObject != null )
      {
         string droppedObjectPath = AssetDatabase.GetAssetPath(droppedObject);
         if ( droppedObjectPath.EndsWith(".cs") && notesEditor.ScriptFilePaths.Contains(droppedObjectPath) )
         {
            note.fileName = droppedObjectPath;
         }
      }

      EditorGUILayout.LabelField("LN", GUILayout.Width(filterLabelWidth));
      note.lineNumber = EditorGUILayout.IntField(note.lineNumber,GUILayout.Width(60));

      GUILayout.EndHorizontal();

   }

   /// <summary>
   /// Render GameObject Field and Open File Button.
   /// </summary>
   private void RenderGameObjectRow( Note note )
   {
      GUILayout.BeginHorizontal();
      // GameObject drag drop field - GameObject
      EditorGUILayout.LabelField("G-Object", GUILayout.Width(labelWidth));

      if ( GUILayout.Button("Open File(s)", GUILayout.Width(100)) )
      {
         notesEditor.Functions.OpenLinkedScripts(note);
      }

      EditorGUI.BeginChangeCheck();

      GameObject linkedGameObject = (GameObject)EditorGUILayout.ObjectField(
          note.linkedGameObject, typeof(GameObject), true, GUILayout.Width(75));

      if ( EditorGUI.EndChangeCheck() )
      {
         note.linkedGameObject = linkedGameObject;
         notesEditor.Functions.UpdateLinkedScriptPaths(note, linkedGameObject);
      }

      // Display a dropdown list of the linked script paths
      if ( note.linkedScriptPaths.Count > 0 )
      {
         //EditorGUILayout.LabelField("Scripts", GUILayout.Width(labelWidth));
         int selectedScriptIndex = EditorGUILayout.Popup(0, note.linkedScriptPaths.ToArray());
      }

    

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

      note.text = EditorGUILayout.TextArea(note.text,textAreaStyle, GUILayout.MinHeight(60), GUILayout.ExpandHeight(true));
   }

   #endregion

   
}
