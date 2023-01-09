using UnityEngine;
using twoloop;

public class OriginShiftDebugger : MonoBehaviour
{
    public void OnGUI()
    {
        if (!OriginShift.singleton.focus)
        {
            return;
        }
        var g = new GUIStyle();
        g.normal.textColor = Color.black;
        GUI.Label(new Rect(10, 370, 300, 20), "Player position: " + OriginShift.singleton.focus.transform.position, g);

        if (OriginShift.singleton)
        {
            GUI.Label(new Rect(10, 400, 1000, 20), "Local Offset (m):  " + OriginShift.LocalOffset.ToString(), g);
        }
    }
}