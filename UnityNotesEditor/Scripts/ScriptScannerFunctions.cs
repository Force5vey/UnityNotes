using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class ScriptScannerFunctions
{
   private NotesEditor notesEditor;


   public ScriptScannerFunctions( NotesEditor notesEditor )
   {
      this.notesEditor = notesEditor;
   }

   /// <summary>
   /// Iterates through all .cs files in project and retrieves comment lines starting with //TODO
   /// </summary>
   public void ScanForTodoComments()
   {
      var taggedComments = new List<TaggedComment>();

      foreach ( string tag in NotesEditor.CachedSettings.commentTags )
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

      notesEditor.FoundTaggedCommentsCollection.notes.Clear(); // Clear existing items

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

         notesEditor.FoundTaggedCommentsCollection.notes.Add(newNote);
      }

      EditorUtility.SetDirty(notesEditor.FoundTaggedCommentsCollection); // Mark the collection as dirty to save changes
   }

   /// <summary>
   /// Import selected TODO's to the Main Notes Collection
   /// </summary>
   public void ImportSelectedTodoNotes()
   {
      if ( notesEditor.CurrentNotesCollection == null )
      {
         Debug.LogError("CurrentNotesCollection is null in TodoScannerWindow");
         return;
      }

      foreach ( var note in notesEditor.FoundTaggedCommentsCollection.notes.Where(n => n.isSelected) )
      {
        notesEditor.CurrentNotesCollection.notes.Add(note); // Add the note to the main collection
      }

      EditorUtility.SetDirty(notesEditor.CurrentNotesCollection); // Mark the main collection as dirty to save changes
   }
}
