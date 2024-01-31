using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class NotesEditorFunctions
{
   private NotesEditor notesEditor;

   public NotesEditorFunctions( NotesEditor notesEditor )
   {
      this.notesEditor = notesEditor;

   }

   // Mark the NotesCollection asset as dirty to ensure changes are saved
   public void MarkNotesCollectionDirty()
   {
      if ( notesEditor.CurrentNotesCollection != null )
      {
         EditorUtility.SetDirty(notesEditor.CurrentNotesCollection);
      }
   }

   /// <summary>
   /// Add a new note to the current collection.
   /// </summary>
   public void AddNewNote()
   {
      if ( notesEditor.CurrentNotesCollection != null )
      {
         Note newNote = new Note
         {
            creationDate = System.DateTime.Now.ToString("dd_MMM - HH:mm"),
            isExpanded = true
         };

         notesEditor.CurrentNotesCollection.notes.Insert(0, newNote);
         MarkNotesCollectionDirty();
      }
   }

   /// <summary>
   /// Remove all completed notes from the collection.
   /// </summary>
   public void RemoveCompletedNotes()
   {
      if ( notesEditor.CurrentNotesCollection != null )
      {
         notesEditor.CurrentNotesCollection.notes = notesEditor.CurrentNotesCollection.notes.Where(
            note => !note.completed).ToList();
         MarkNotesCollectionDirty();
      }
   }

   /// <summary>
   /// Toggle the expansion state of all notes.
   /// </summary>
   public void ToggleAllNotes(NotesCollection collectionToToggle)
   {
      if ( collectionToToggle != null )
      {
         bool anyNoteExpanded = notesEditor.Functions.CheckIfAnyNoteIsExpanded(collectionToToggle);
         bool newExpansionState = !anyNoteExpanded;

         foreach ( var note in collectionToToggle.notes )
         {
            note.isExpanded = newExpansionState;
         }
         notesEditor.AllNotesExpanded = newExpansionState;
      }
   }

   /// <summary>
   /// Helper method to set Expand/Collapse button to correct state.
   /// </summary>
   /// <returns></returns>
   public bool CheckIfAnyNoteIsExpanded(NotesCollection collectionToCheck)
   {
      return collectionToCheck != null && collectionToCheck.notes.Any(note => note.isExpanded);
   }

   public void ApplyFilterAndSort()
   {
      if ( notesEditor.CurrentNotesCollection == null )
         return;

      IEnumerable<Note> filteredNotes = FilterNotesByPriorityCategoryAndStatus();

      // Sort by priority
      filteredNotes = filteredNotes.OrderBy(note => note.priority);

      // Update the notes collection to show filtered and sorted notes on top
      UpdateNotesCollection(filteredNotes);

      // Reflect changes in the editor window

      //TODO: I added repaint to ongui for ui effects, removed this to ensure I'm not doing too much repainting
      //if there is an issue with how filtered notes are displayed check here.
      //notesEditor.Repaint();
   }


   private IEnumerable<Note> FilterNotesByPriorityCategoryAndStatus()
   {
      IEnumerable<Note> filteredNotes = notesEditor.CurrentNotesCollection.notes;

      if ( notesEditor.SelectedCategoryFilter.HasValue )
      {
         filteredNotes = filteredNotes.Where(note => note.category == notesEditor.SelectedCategoryFilter.Value);
      }

      if ( notesEditor.SelectedPriorityFilter.HasValue )
      {
         filteredNotes = filteredNotes.Where(note => note.priority == notesEditor.SelectedPriorityFilter.Value);
      }

      if ( notesEditor.SelectedStatusFilter.HasValue )
      {
         filteredNotes = filteredNotes.Where(note => note.status == notesEditor.SelectedStatusFilter.Value);
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
      var remainingNotes = notesEditor.CurrentNotesCollection.notes.Except(filteredNotesList).ToList();
      foreach ( var note in remainingNotes )
      {
         note.isExpanded = false; // Collapse remaining notes
      }

      notesEditor.CurrentNotesCollection.notes = filteredNotesList.Concat(remainingNotes).ToList();
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
      if ( notesEditor.NotesCollectionPaths.Length > notesEditor.SelectedNotesCollectionIndex )
      {
         string selectedPath = AssetDatabase.GUIDToAssetPath(
             AssetDatabase.FindAssets($"t:NotesCollection", new[] { NotesEditor.CachedSettings.notesFolderPath })[notesEditor.SelectedNotesCollectionIndex]);
         notesEditor.CurrentNotesCollection = AssetDatabase.LoadAssetAtPath<NotesCollection>(selectedPath);
      }
      notesEditor.UpdateNotesCollectionsList();
   }

   /// <summary>
   /// Toggle the isSelected Property of a note in passed Collection
   /// </summary>
   /// <param name="notesCollection">The collection to work with</param>
   public void SetAllNotesSelected(NotesCollection notesCollection)
   {
      notesEditor.AllNotesSelected = !notesEditor.AllNotesSelected;
      foreach ( var note in notesCollection.notes )
      {
         note.isSelected = notesEditor.AllNotesSelected;
      }
   }
}
