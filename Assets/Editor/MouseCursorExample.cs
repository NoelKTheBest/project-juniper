//This script creates a new menu (“Examples”) and a menu item (“Mouse Cursor”). Click on this option. This displays a small window that has a color box in it.
//Hover over the colored box to cause a Link mouse cursor to appear.


using UnityEngine;
using UnityEditor;

public class MouseCursorExample : EditorWindow
{
    [MenuItem("Window/Examples/MouseCursorRect Example")]
    static void AddCursorRectExample()
    {
        MouseCursorExample window =
            EditorWindow.GetWindowWithRect<MouseCursorExample>(new Rect(0, 0, 180, 80));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(10, 10, 160, 60), new Color(0.5f, 0.5f, 0.85f));
        EditorGUI.DrawRect(new Rect(20, 20, 140, 40), new Color(0.9f, 0.9f, 0.9f));
        EditorGUIUtility.AddCursorRect(new Rect(20, 20, 140, 40), MouseCursor.Link);
    }
}
