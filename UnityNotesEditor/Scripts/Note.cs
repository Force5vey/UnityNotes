using System.Collections.Generic;
using System;

using UnityEngine;

[System.Serializable]
public class Note
{
   public string creationDate; //This gets auto set, but can be modified.
   public string title; // A brief title for the note
   public NoteCategory category; // The category of the note based on the enum below.
   public bool completed; // Whether the note/task is completed, method used mark for deletion.
   public PriorityLevel priority; // Priority of the note/task based on below enum - replace images in Icons Folder to make your own.

   public NoteStatus status; // Current status of the note, according to the enum below.
   public string text; // The main content of the note
   
   //Linked Files
   public string fileName; // Use files from your unity project to enable opening from the editor window to the set line number.
   public int lineNumber; // Will open the linked file to this line number
   //Link a gameobject from the hierarchy to provide reference and opens all attached scripts.
   public GameObject linkedGameObject;
   public string linkedGameObjectPath;
   public string linkedSceneName;
   public List<string> linkedScriptPaths = new List<string>();

   public bool isExpanded;
   public bool isSelected;

   /// <summary>
   /// Edit string formatting if you want a different default date / time format.
   /// </summary>
   public Note()
   {
      creationDate = DateTime.Now.ToString("dd_MMM - HH: mm");
   }
}

public enum NoteCategory
{
   TODO,
   Bug,
   Feature,
   Improvement,
   Design,
   Testing,
   Documentation,
   Other
}

public enum PriorityLevel
{
   Low,
   Medium,
   High,
   Critical
}

public enum NoteStatus
{
   NotStarted,
   InProgress,
   OnHold,
   Completed,
   Abandoned
}
