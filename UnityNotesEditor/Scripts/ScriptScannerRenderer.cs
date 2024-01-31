using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class ScriptScannerRenderer
{
   private NotesEditor notesEditor;


   public ScriptScannerRenderer( NotesEditor notesEditor )
   {
      this.notesEditor = notesEditor;
   }

   public void InitializeScriptScannerRendering()
   {
      RenderTopBar();
      RenderControlBar();
      RenderFoundCommentsList();
   }

   /// <summary>
   /// Render the top bar displaying function buttons.
   /// </summary>
   private void RenderTopBar()
   {
      GUILayout.BeginHorizontal();
      if ( GUILayout.Button("Scan Scripts for Tags", GUILayout.Width(150)) )
      {
         notesEditor.SSFunctions.ScanForTodoComments();
      }
      if ( GUILayout.Button("Import Selected", GUILayout.Width(150)) )
      {
         notesEditor.SSFunctions.ImportSelectedTodoNotes();
      }


      GUILayout.EndHorizontal();
   }

   private void RenderControlBar()
   {
      GUILayout.BeginHorizontal();

      if ( GUILayout.Button(notesEditor.AllNotesExpanded ? "Collapse" : "Expand", GUILayout.Width(75)) )
      {
         notesEditor.Functions.ToggleAllNotes(notesEditor.FoundTaggedCommentsCollection);
      }

      if ( GUILayout.Button(notesEditor.AllNotesSelected ? "Deselect All" : "Select All", GUILayout.Width(100)) )
      {
         notesEditor.Functions.SetAllNotesSelected(notesEditor.FoundTaggedCommentsCollection);
      }
      GUILayout.EndHorizontal();
   }

   /// <summary>
   /// Render the list of notes with foldouts and detailed editing fields.
   /// </summary>
   public void RenderFoundCommentsList()
   {
      if ( notesEditor.FoundTaggedCommentsCollection == null )
         return;

      GUILayout.Label("Notes:", EditorStyles.boldLabel);
      notesEditor.EditorScrollPosition = EditorGUILayout.BeginScrollView(notesEditor.EditorScrollPosition);

      for ( int i = 0; i < notesEditor.FoundTaggedCommentsCollection.notes.Count; i++ )
      {
         RenderNoteItem(notesEditor.FoundTaggedCommentsCollection.notes[i]);
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
         EditorUtility.SetDirty(notesEditor.FoundTaggedCommentsCollection);
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

      Rect rect = GUILayoutUtility.GetRect(150, 2, GUILayout.ExpandWidth(false));
      EditorGUI.DrawRect(rect, notesEditor.CategoryColors[note.category]);
      GUILayout.Space(5);
   }

   ///<summary>
   /// Render the title field for a note
   /// </summary>
   private void RenderNoteTitleRow( Note note )
   {
      GUILayout.BeginHorizontal();

      EditorGUILayout.LabelField("Title", GUILayout.Width(150));
      note.title = EditorGUILayout.TextField(note.title);

      GUILayout.EndHorizontal();
   }

   /// <summary>
   /// Render the Category Drop-down and Date Field.
   /// </summary>
   private void RenderCategoryAndDateRow( Note note )
   {
      GUILayout.BeginHorizontal();

      EditorGUILayout.LabelField("Category", GUILayout.Width(150));
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

      EditorGUILayout.LabelField("Status", GUILayout.Width(150));
      note.status = (NoteStatus)EditorGUILayout.EnumPopup(note.status);

      EditorGUILayout.LabelField("Priority", GUILayout.Width(150));
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
}
