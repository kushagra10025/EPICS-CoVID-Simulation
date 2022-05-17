﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.Linq;

namespace CSimHelper.Data
{
    public class StackedPlot
    {
        // The data and properties used to draw it
        public StackedPlotAttribute Attribute;
        public StackedData Data;

        // Current size (based on Inspector size)
        public float Width, Height;

        // Singleton
        static Material Material = null;



        

        public StackedPlot(StackedData data, StackedPlotAttribute attribute)
        {
            Data = data;
            Attribute = attribute;

            Initialise();
        }

        public void Initialise ()
        {
            Data.CalculateStatistics();

            if (Material == null)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                Material = new Material(shader);
            }
        }

        public void Destroy ()
        {
            Object.DestroyImmediate(Material);
        }

        

        // https://answers.unity.com/questions/1360515/how-do-i-draw-lines-in-a-custom-inspector.html
        // From Data to Rect
        // point:  [0, maxX]
        // vertex: [0, rect.width]
        private float GetX(float x)
        {
            return (x / Data.Max.x) * Width;
        }
        private float GetY(float y)
        {
            return Height - (y / Data.Max.y) * Height;
        }
        private Vector2 GetPoint(Vector2 point)
        {
            return new Vector2(GetX(point.x), GetY(point.y));
        }


        //public void OnInspectorGUI()
        public void OnGUI(Rect rect)
        {
            if (UnityEngine.Event.current.type != EventType.Repaint)
                return;

            if (Data == null || Data.Data.Count == 0)
                return;

            Data.CalculateStatistics(); // Not re-calculated if data is unchanged

            //Rect rect = GUILayoutUtility.GetRect(10, 500, Attribute.Height, Attribute.Height);
            //Width = rect.width;
            //Height = rect.height;


            Width = rect.width;
            Height = rect.height - StackedPlotDrawer.PropertyHeight;

            // Aspect ratio
            //Data.Max.y = Data.Max.x / (Attribute.Grid.x / Attribute.Grid.y);
            //Data.Max.x = Data.Max.y * (Attribute.Grid.x / Attribute.Grid.y);
            //GUI.Box(rect, "This is a box", "x");

            GUI.BeginClip(rect);
            GL.PushMatrix();

            GL.Clear(true, false, Color.black);
            Material.SetPass(0);

            // --- Background -----------------
            GL.Begin(GL.QUADS);

            GL.Color(Attribute.BackgroundColor);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(Width, 0, 0);
            GL.Vertex3(Width, Height, 0);
            GL.Vertex3(0, Height, 0);
            //GL.Vertex3(0, 0, 0);
            //GL.Vertex3(rect.width, 0, 0);
            //GL.Vertex3(rect.width, rect.height, 0);
            //GL.Vertex3(0, rect.height, 0);
            GL.End();
            // -----------------


            // --- Grid -----------------
            GL.Begin(GL.LINES);
            GL.Color(Attribute.GridColor.xA(0.25f));

            for (float x = 0; x <= Data.Max.x; x += Attribute.GridX)
                VerticalLine(x);
            for (float y = 0; y <= Data.Max.y; y += Attribute.GridY)
                HorizontalLine(y);

            VerticalLine(Data.Max.x);
            HorizontalLine(Data.Max.y);

            GL.End();
            // -----------------



            // --- Stats -----------------
            //GL.Begin(GL.LINES);
            // Median
            //GL.Color(Color.yellow.xA(0.5f));
            //GL.Color(Attribute.MedianColour.xA(0.5f));
            //VerticalLine(Data.Quartile2.x);
            //HorizontalLine(Data.Quartile2.y);

            /*
            // IQR
            GL.Color(new Color(1, 1, 0, 0.5f));
            verticalLine(quartile1X);
            verticalLine(quartile3X);

            GL.Color(new Color(1, 1, 0, 0.5f));
            horizontalLine(quartile1Y);
            horizontalLine(quartile3Y);
            */
            //GL.End();

            // --- IQR -----------------
            //GL.Begin(GL.QUADS);
            //GL.Color(Attribute.MedianColour.xA(0.05f));
            //GLRect(Data.Quartile1.x, 0, Data.Quartile3.x, Data.Max.y);
            //GLRect(0, Data.Quartile1.y, Data.Max.x, Data.Quartile3.y);
            //GL.End();
            // -----------------


            // --- Data (lines) -----------------
            // Loops through all of the lines (i)
            // [0] because all elements have the same lenght, so we take the first one
            for (int i = 0; i < Data.Data[0].Length; i++)
            {
                GL.Begin(GL.TRIANGLE_STRIP);
                //GL.Color(Color.white);
                GL.Color(Attribute.DataColours[i].xA(0.5f));
                for (int x = 0; x < Data.Data.Count; x++)
                {
                    Vector2 p0 = GetPoint(Data.GetStacked(x, i - 0));
                    Vector2 p1 = GetPoint(Data.GetStacked(x, i - 1));

                    GL.Vertex3(p0.x, p0.y, 0f);
                    GL.Vertex3(p1.x, p1.y, 0f);
                }
                GL.End();
            }
            /*
            for (int i = 0; i < Data.Stacked[0].Length; i ++) 
            {
                //if (Data.Data.Count > 1)
                //{
                GL.Begin(GL.LINES);
                //GL.Color(Color.white);
                //GL.Color(Attribute.DataColour.xA(0.5f));
                GL.Color(Attribute.DataColours[i].xA(0.5f));
                //foreach (Vector2 point in Data.Data)
                for (int x = 0; x < Data.Stacked.Count - 1; x++)
                {
                    GLLine(
                        GetPoint(new Vector2(x + 0, Data.Stacked[x + 0][i])),
                        GetPoint(new Vector2(x + 1, Data.Stacked[x + 1][i]))
                        );
                    //GLCross(GetPoint(point), 3);
                }
                GL.End();
                //}
            }
            */
            // -----------------

            /*
            // --- Data (crosses) -----------------
            GL.Begin(GL.LINES);
            //GL.Color(Color.white);
            GL.Color(Attribute.DataColour.xA(0.5f));
            foreach (Vector2 point in Data.Data)
                GLCross(GetPoint(point), 3);
            GL.End();
            // -----------------
            */


            GL.PopMatrix();
            GUI.EndClip();

            // --- Labels -----------------
            // Only for OnDrawInspectorGUI
            //GUI.contentColor = Color.yellow;
            //GUI.Label(new Rect(GetX(Data.Quartile2.x), 0, 100, 50), Data.Quartile2.x + " ticks", EditorStyles.boldLabel);
            //GUI.Label(new Rect(0, GetY(Data.Quartile2.y), 100, 50), Data.Quartile2.y + " points", EditorStyles.boldLabel);

            // Only for OnGUI
            //GUI.contentColor = Attribute.MedianColour;
            //if (Attribute.LabelX != null)
            //    EditorGUI.LabelField(new Rect(rect.x + GetX(Data.Quartile2.x), rect.y + 0, 100, 50), Data.Quartile2.x + " " + Attribute.LabelX, EditorStyles.boldLabel);
            //if (Attribute.LabelX != null)
            //    EditorGUI.LabelField(new Rect(rect.x + 0, rect.y + GetY(Data.Quartile2.y), 100, 50), Data.Quartile2.y + " " + Attribute.LabelY, EditorStyles.boldLabel);


            

            //GUISkin skin = new GUISkin();
            //skin.box.border = new RectOffset(10, 10, 10, 10);
            //GUI.Box(rect,"",skin.box);
            // -----------------
        }

        // Draws line
        private void VerticalLine(float x)
        {
            GL.Vertex3(GetX(x), 0, 0);
            GL.Vertex3(GetX(x), Height, 0);
        }
        private void HorizontalLine(float y)
        {
            GL.Vertex3(0, GetY(y), 0);
            GL.Vertex3(Width, GetY(y), 0);
        }

        // A box in GL
        // bottomLeft, topRight
        private void GLRect(float x0, float y0, float x1, float y1)
        {
            float px0 = GetX(x0);
            float py0 = GetY(y0);

            float px1 = GetX(x1);
            float py1 = GetY(y1);

            GL.Vertex3(px0, py0, 0);
            GL.Vertex3(px1, py0, 0);
            GL.Vertex3(px1, py1, 0);
            GL.Vertex3(px0, py1, 0);
        }

        /*
        void GLCross(Vector3 vertex, int radius)
        {
            GL.Vertex3(vertex.x, vertex.y - radius, 0);
            GL.Vertex3(vertex.x, vertex.y + radius, 0);

            GL.Vertex3(vertex.x - radius, vertex.y, 0);
            GL.Vertex3(vertex.x + radius, vertex.y, 0);
        }

        void GLLine(Vector3 pointA, Vector3 pointB)
        {
            GL.Vertex3(pointA.x, pointA.y, 0);
            GL.Vertex3(pointB.x, pointB.y, 0);
        }
        */
    }
}