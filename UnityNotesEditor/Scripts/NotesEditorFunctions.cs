using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class NotesEditorFunctions
{
   private NotesEditor editorWindow;

   public NotesEditorFunctions( NotesEditor editorWindow )
   {
      this.editorWindow = editorWindow;

   }

   // Mark the NotesCollection asset as dirty to ensure changes are saved
   public void MarkNotesCollectionDirty()
   {
      if ( editorWindow.CurrentNotesCollection != null )
      {
         EditorUtility.SetDirty(editorWindow.CurrentNotesCollection);
      }
   }
   //TODO: This is a new note I am adding .
   /// <summary>
   /// Add a new note to the current collection.
   /// </summary>
   public void AddNewNote()
   {
      if ( editorWindow.CurrentNotesCollection != null )
      {
         Note newNote = new Note
         {
            creationDate = System.DateTime.Now.ToString("dd_MMM - HH:mm"),
            isExpanded = true
         };

         editorWindow.CurrentNotesCollection.notes.Insert(0, newNote);
         MarkNotesCollectionDirty();
      }
   }

   /// <summary>
   /// Remove all completed notes from the collection.
   /// </summary>
   public void RemoveCompletedNotes()
   {
      if ( editorWindow.CurrentNotesCollection != null )
      {
         editorWindow.CurrentNotesCollection.notes = editorWindow.CurrentNotesCollection.notes.Where(
            note => !note.completed).ToList();
         MarkNotesCollectionDirty();
      }
   }

   /// <summary>
   /// Toggle the expansion state of all notes.
   /// </summary>
   public void ToggleAllNotes()
   {
      if ( editorWindow.CurrentNotesCollection != null )
      {
         editorWindow.AllNotesExpanded = !editorWindow.AllNotesExpanded;
         foreach ( var note in editorWindow.CurrentNotesCollection.notes )
         {
            note.isExpanded = editorWindow.AllNotesExpanded;
         }
      }
   }

   public void ApplyFilterAndSort()
   {
      if ( editorWindow.CurrentNotesCollection == null )
         return;

      IEnumerable<Note> filteredNotes = FilterNotesByPriorityCategoryAndStatus();

      // Sort by priority
      filteredNotes = filteredNotes.OrderBy(note => note.priority);

      // Update the notes collection to show filtered and sorted notes on top
      UpdateNotesCollection(filteredNotes);

      // Reflect changes in the editor window
      editorWindow.Repaint();
   }


   private IEnumerable<Note> FilterNotesByPriorityCategoryAndStatus()
   {
      IEnumerable<Note> filteredNotes = editorWindow.CurrentNotesCollection.notes;

      if ( editorWindow.SelectedCategoryFilter.HasValue )
      {
         filteredNotes = filteredNotes.Where(note => note.category == editorWindow.SelectedCategoryFilter.Value);
      }

      if ( editorWindow.SelectedPriorityFilter.HasValue )
      {
         filteredNotes = filteredNotes.Where(note => note.priority == editorWindow.SelectedPriorityFilter.Value);
      }

      if ( editorWindow.SelectedStatusFilter.HasValue )
      {
         filteredNotes = filteredNotes.Where(note => note.status == editorWindow.SelectedStatusFilter.Value);
      }

      return filteredNotes;
   }


   /// <summary>
   /// Update the notes collection to reflect the filtered and sorted notes.
   /// </summary>
   public void UpdateNotesCollection( IEnumerable<Note> filteredNotes )
   {
      var filteredNotesList = filteredNotes.ToList();
      foreach ( var note in filteredNotesList )
      {
         note.isExpanded = true; // Expand filtered notes
      }
      var remainingNotes = editorWindow.CurrentNotesCollection.notes.Except(filteredNotesList).ToList();
      foreach ( var note in remainingNotes )
      {
         note.isExpanded = false; // Collapse remaining notes
      }

      editorWindow.CurrentNotesCollection.notes = filteredNotesList.Concat(remainingNotes).ToList();
   }

   /// <summary>
   /// Open a script file at a specific line number.
   /// </summary>
   public void OpenScriptFile( Note note )
   {
      if ( !string.IsNullOrEmpty(note.fileName) )
      {
         UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(note.fileName);
         if ( asset != null )
         {
            AssetDatabase.OpenAsset(asset, note.lineNumber);
            Debug.Log("Opened script file: " + note.fileName + " at line " + note.lineNumber);
         }
         else
         {
            Debug.LogWarning("Failed to load script file at path: " + note.fileName);
         }
      }

   }

   /// <summary>
   /// Opens the list of files linked to the Linked Game Object.
   /// </summary>
   public void OpenLinkedScripts( Note note )
   {
      foreach ( var scriptPath in note.linkedScriptPaths )
      {
         var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
         if ( scriptAsset != null )
         {
            AssetDatabase.OpenAsset(scriptAsset);
            Debug.Log("Opened script: " + scriptPath);
         }
         else
         {
            Debug.LogWarning("Failed to load script at path: " + scriptPath);
         }
      }
   }


   /// <summary>
   /// Get a list of all script files within the project
   /// </summary>
   public string[] GetAllScriptFiles()
   {
      string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
      return allAssetPaths.Where(path => path.StartsWith("Assets/") && path.EndsWith(".cs")).ToArray();
   }

   public void UpdateLinkedScriptPaths( Note note, GameObject linkedGameObject )
   {
      note.linkedScriptPaths.Clear();

      if ( linkedGameObject != null )
      {
         var scripts = linkedGameObject.GetComponents<MonoBehaviour>();
         foreach ( var script in scripts )
         {
            if ( script != null )
            {
               var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(script));
               if ( !string.IsNullOrEmpty(scriptPath) )
               {
                  note.linkedScriptPaths.Add(scriptPath);
               }
            }
         }
         Debug.Log("Updated script paths for GameObject: " + linkedGameObject.name);
      }

      MarkNotesCollectionDirty(); // Mark the collection as dirty to save changes
   }

   public void LoadSelectedNotesCollection()
   {
      if ( editorWindow.NotesCollectionPaths.Length > editorWindow.SelectedNotesCollectionIndex )
      {
         string selectedPath = AssetDatabase.GUIDToAssetPath(
             AssetDatabase.FindAssets($"t:NotesCollection", new[] { NotesEditor.CachedSettings.notesFolderPath })[editorWindow.SelectedNotesCollectionIndex]);
         editorWindow.CurrentNotesCollection = AssetDatabase.LoadAssetAtPath<NotesCollection>(selectedPath);
      }
      editorWindow.UpdateNotesCollectionsList();
   }
}
