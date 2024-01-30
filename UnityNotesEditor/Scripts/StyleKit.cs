using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class StyleKit
{
   private static string _texturesPath;

   //// Toolbar Buttons (Tabs)
   //public static Color ToolbarButtonNormalText = new Color(0.6f, 0.75f, 1.0f); // Soft blue
   //public static Color ToolbarButtonHoverText = new Color(0.7f, 0.85f, 1.0f); // Brighter blue
   //public static Color ToolbarButtonActiveText = new Color(1.0f, 0.4f, 0.4f); // Vibrant red

   //// Header Buttons and Labels
   //public static Color HeaderButtonNormalText = new Color(0.5f, 0.65f, 0.8f); // Darker blue
   //public static Color HeaderButtonHoverText = new Color(0.6f, 0.75f, 0.9f); // Medium blue
   //public static Color HeaderButtonActiveText = new Color(0.8f, 0.9f, 1.0f); // Light blue
   //public static Color HeaderLabelText = new Color(0.7f, 0.85f, 1.0f); // Bright blue

   //// Note Buttons and Labels
   //public static Color NoteButtonNormalText = new Color(0.5f, 0.65f, 0.8f); // Darker blue
   //public static Color NoteButtonHoverText = new Color(0.65f, 0.8f, 0.95f); // Medium blue
   //public static Color NoteButtonActiveText = new Color(0.75f, 0.9f, 1.0f); // Light blue
   //public static Color NoteLabelText = new Color(0.6f, 0.75f, 0.9f); // Soft blue



   // Colors
   public static Color ToolbarButtonNormalText = new Color(0.8f, 0.8f, 0.8f); // Light gray
   public static Color ToolbarButtonHoverText = new Color(0.7f, 0.85f, 1.0f); // Brighter gray
   public static Color ToolbarButtonActiveText = new Color(1.0f, 0.5f, 0.2f); // Bright orange


   public static Color HeaderButtonNormalText = new Color(0.7f, 0.7f, 0.7f); // Medium gray
   public static Color HeaderButtonHoverText = new Color(0.75f, 0.75f, 0.75f); // Slightly brighter gray
   public static Color HeaderButtonActiveText = new Color(0.8f, 0.8f, 0.8f); // Bright gray

   public static Color HeaderLabelText = new Color(0.9f, 0.9f, 0.9f); // Light gray, slightly brighter than buttons


   public static Color NoteButtonNormalText = new Color(0.65f, 0.75f, 0.85f); // Soft blue-gray
   public static Color NoteButtonHoverText = new Color(0.7f, 0.8f, 0.9f); // Slightly brighter blue-gray
   public static Color NoteButtonActiveText = new Color(0.75f, 0.85f, 0.95f); // Even brighter blue-gray
   public static Color NoteLabelText = new Color(0.8f, 0.9f, 1.0f); // Lightest blue-gray


   // Custom Textures
   private static Texture2D toolbarButtonNormalTexture = null;
   private static Texture2D toolbarButtonHoverTexture = null;
   private static Texture2D toolbarButtonActiveTexture = null;

   private static Texture2D headerButtonNormalTexture = null;
   private static Texture2D headerButtonHoverTexture = null;
   private static Texture2D headerButtonActiveTexture = null;

   private static Texture2D noteButtonNormalTexture = null;
   private static Texture2D noteButtonHoverTexture = null;
   private static Texture2D noteButtonActiveTexture = null;


   // Font Sizes
   public static int FontSizeSmall = 10;
   public static int FontSizeMedium = 12;
   public static int FontSizeLarge = 14;

   // Button Sizes
   public static float ButtonWidthLarge = 150;
   public static float ButtonHeightLarge = 50;

   public static float ButtonWidthMedium = 100;
   public static float ButtonHeightMedium = 50;

   public static float ButtonWidthSmall = 50;
   public static float ButtonHeightSmall = 50;

   // Label Sizes
   public static float LabelWidthLarge = 100;
   public static float LabelHeightLarge = 50;

   public static float LabelWidthMedium = 75;
   public static float LabelHeightMedium = 50;

   public static float LabelWidthSmall = 40;
   public static float LabelHeightSmall = 50;

   // Enum Sizes
   public static float enumWidthLarge = 150f;
   public static float enumWidthMedium = 75;
   public static float enumWidthSmall = 50;

   // Bars / Lines
   public static float BarWidthLarge = 300f;
   public static float BarHeightLarge = 4f;

   public static float BarWidthMedium = 150f;
   public static float BarHeightMedium = 2;

   public static float BarWidthSmall = 75f;
   public static float BarHeightSmall = 1f;

   // Icon Sizes
   public static float IconWidthLarge = 70f;
   public static float IconHeightLarge = 70f;

   public static float IconWidthMedium = 50f;
   public static float IconHeightMedium = 50f;

   public static float IconWidthSmall = 30f;
   public static float IconHeightSmall = 30f;

   // Cached GUIStyle Fields
   private static GUIStyle _mainToolbarButton = null;

   private static GUIStyle _headerButton = null;
   private static GUIStyle _headerLabel = null;

   private static GUIStyle _noteItemButton = null;
   private static GUIStyle _noteItemLabel = null;


   // Main Toolbar Styles
   public static GUIStyle MainToolbarButton
   {
      get
      {
         if ( _mainToolbarButton == null )
         {
            _mainToolbarButton = new GUIStyle(GUI.skin.button);
            _mainToolbarButton.fontSize = FontSizeMedium;
            _mainToolbarButton.normal.textColor = ToolbarButtonNormalText;
            _mainToolbarButton.hover.textColor = ToolbarButtonHoverText;
            _mainToolbarButton.active.textColor = ToolbarButtonActiveText;
         }
         return _mainToolbarButton;
      }
   }


   // Header Item Styles
   public static GUIStyle HeaderButton
   {
      get
      {
         if ( _headerButton == null )
         {
            _headerButton = new GUIStyle(GUI.skin.button);
            _headerButton.fontSize = FontSizeMedium;
            //_headerButton.normal.background = Texture2D.grayTexture;
            _headerButton.normal.textColor = HeaderButtonNormalText;
            _headerButton.hover.textColor = HeaderButtonHoverText;
            _headerButton.active.textColor = HeaderButtonActiveText;
         }
         return _headerButton;
      }
   }
   public static GUIStyle HeaderLabel
   {
      get
      {
         if ( _headerLabel == null )
         {
            _headerLabel = new GUIStyle(GUI.skin.label);
            _headerLabel.fontSize = FontSizeSmall;
            _headerLabel.normal.textColor = HeaderLabelText;
           
         }
         return _headerLabel;
      }
   }

   // Note Item Styles
   public static GUIStyle NoteItemButton
   {
      get
      {
         if ( _noteItemButton == null )
         {
            _noteItemButton = new GUIStyle(GUI.skin.button);
            _noteItemButton.fontSize = FontSizeSmall;
            _noteItemButton.normal.background = Texture2D.whiteTexture;
            _noteItemButton.normal.textColor = NoteButtonNormalText;
            _noteItemButton.hover.textColor = NoteButtonHoverText;
            _noteItemButton.hover.textColor = NoteButtonActiveText;
         }
         return _noteItemButton;
      }
   }

   public static GUIStyle NoteItemLabel
   {
      get
      {
         if ( _noteItemLabel == null )
         {
            _noteItemLabel = new GUIStyle(GUI.skin.label);
            _noteItemLabel.fontSize = FontSizeMedium;
            _noteItemLabel.normal.textColor = NoteLabelText;
         }
         return _noteItemLabel;
      }
   }

   public static void Initialize( string notesFolderPath )
   {
      _texturesPath = System.IO.Path.Combine(notesFolderPath, "Textures");

      // Load textures for toolbar buttons
      LoadTextureWithFallback("toolbarButtonNormalTexture.png", ref toolbarButtonNormalTexture);
      LoadTextureWithFallback("toolbarButtonHoverTexture.png", ref toolbarButtonHoverTexture);
      LoadTextureWithFallback("toolbarButtonActiveTexture.png", ref toolbarButtonActiveTexture);

      // Load textures for header buttons
      LoadTextureWithFallback("headerButtonNormalTexture.png", ref headerButtonNormalTexture);
      LoadTextureWithFallback("headerButtonHoverTexture.png", ref headerButtonHoverTexture);
      LoadTextureWithFallback("headerButtonActiveTexture.png", ref headerButtonActiveTexture);

      // Load textures for note buttons
      LoadTextureWithFallback("noteButtonNormalTexture.png", ref noteButtonNormalTexture);
      LoadTextureWithFallback("noteButtonHoverTexture.png", ref noteButtonHoverTexture);
      LoadTextureWithFallback("noteButtonActiveTexture.png", ref noteButtonActiveTexture);
   }

   private static void LoadTextureWithFallback( string textureName, ref Texture2D textureVar )
   {
      if ( !LoadTexture(textureName, ref textureVar) )
      {
         // LoadTexture failed, assign a default texture
         textureVar = EditorGUIUtility.whiteTexture;
      }
   }


   private static bool LoadTexture( string textureName, ref Texture2D texture )
   {
      string fullPath = System.IO.Path.Combine(_texturesPath, textureName);
      texture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);

      if ( texture == null )
      {
         return false;
      }

      return true;
   }

   // Initialize the category color mapping
   public static Dictionary<NoteCategory, Color> InitializeCategoryColors()
   {
      Dictionary<NoteCategory, Color> categoryColors = new Dictionary<NoteCategory, Color>()
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

      return categoryColors;
   }
}