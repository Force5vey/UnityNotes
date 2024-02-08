using UnityEngine;
using System.Collections.Generic;

// ScriptableObject representing a collection of notes
//This creates the right click menu but you can use the tool bar menu created by CreateNotes Script for auto naming.
[CreateAssetMenu(fileName = "NewNotesCollection", menuName = "Notes/NoteCollection")]
public class NotesCollectionDefinition :ScriptableObject
{
   // List of notes in this collection
   public List<Note> notes = new List<Note>();
}

