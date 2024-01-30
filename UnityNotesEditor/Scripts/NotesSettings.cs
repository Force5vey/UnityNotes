using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "NotesSettings", menuName = "Notes/Settings")]
public class NotesSettings :ScriptableObject
{
   [Tooltip("Working directory. Use for Notes Collections Location.")]
   public string notesFolderPath = "Assets/Editor/UnityNotesEditor/";

   [Tooltip("Custom Project Name. Blank uses Build Settings")]
   public string projectName;

   [Tooltip("Icon for Low Priority")]
   public Texture2D lowPriorityIcon;

   [Tooltip("Icon for Medium Priority")]
   public Texture2D mediumPriorityIcon;

   [Tooltip("Icon for High Priority")]
   public Texture2D highPriorityIcon;

   [Tooltip("Icon for Critical Priority")]
   public Texture2D criticalPriorityIcon;

   [Tooltip("One per Element, Tool scans for any instance of the tag, and pulls that line. \n Avoid escape and special characters.")]
   public List<string> commentTags = new List<string>() { "//TODO", "//UNITY NOTE" };

   [Tooltip("Stores the available NotesCollections to reference between sessions.")]
   public List<string> allNotesCollectionPaths;
   [Tooltip("Used to track last opened NotesCollection; you can change it if you want, it opens this index.")]
   public int currentCollectionIndex;

}

