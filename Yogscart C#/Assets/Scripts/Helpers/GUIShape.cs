using UnityEngine;
using System.Collections;

public static class GUIShape
{

	public static void RoundedRectangle(Rect rect, int radius, Color colour)
    {
        if (radius >= 0 && rect.height >= radius * 2f)
        {
            Color oldColour = GUI.color;
            GUI.color = colour;

            int twoRadius = radius * 2;

            //Draw centre Square          
            GUI.DrawTexture(new Rect(rect.x + radius, rect.y + radius, rect.width - twoRadius, rect.height - twoRadius), Resources.Load<Texture2D>("UI/Shapes/Square"));
            //Draw Side Edges
            GUI.DrawTexture(new Rect(rect.x, rect.y + radius, radius, rect.height - twoRadius), Resources.Load<Texture2D>("UI/Shapes/Square"));
            GUI.DrawTexture(new Rect(rect.x + rect.width - radius, rect.y + radius, radius, rect.height - twoRadius), Resources.Load<Texture2D>("UI/Shapes/Square"));
            //Draw Top Edges
            GUI.DrawTexture(new Rect(rect.x + radius, rect.y, rect.width - twoRadius, radius), Resources.Load<Texture2D>("UI/Shapes/Square"));
            GUI.DrawTexture(new Rect(rect.x + radius, rect.y + rect.height - radius, rect.width - twoRadius, radius), Resources.Load<Texture2D>("UI/Shapes/Square"));

            //Draw Corners
            GUI.DrawTexture(new Rect(rect.x, rect.y, radius, radius), Resources.Load<Texture2D>("UI/Shapes/CornerTL"));
            GUI.DrawTexture(new Rect(rect.x + rect.width - radius, rect.y, radius, radius), Resources.Load<Texture2D>("UI/Shapes/CornerTR"));
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - radius, radius, radius), Resources.Load<Texture2D>("UI/Shapes/CornerBL"));
            GUI.DrawTexture(new Rect(rect.x + rect.width - radius, rect.y + rect.height - radius, radius, radius), Resources.Load<Texture2D>("UI/Shapes/CornerBR"));


            GUI.color = oldColour;
        }
    }
}
