using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace CSimHelper.Data
{
    // Used for stacked area charts
    [Serializable]
    public class StackedData
    {
        // List of (value of all lines at that point)
        public List<float[]> Data = new List<float[]>();

        public void Add(params float [] xs)
        {
            Data.Add(xs);
            Dirty = true;
        }

        /*
        public Vector2 this[int x, int i]
        {
            get
            {
                return new Vector2(x, Data[x][i]);
            }
        }
        */

        // Get stacked coordinate in a format compatible with StackPlot drawer
        // x: time
        // i: line id
        // if i = -1, returns y value 0
        public Vector2 GetStacked (int x, int i)
        {
            return new Vector2
            (
                x,
                i == -1
                    ? 0
                    : Stacked[x][i]
            );
        }

        // Statistics
        [HideInInspector] public bool Dirty = true; // Statistics needs to be recalculated
        [HideInInspector] public float[] Sum;
        [HideInInspector] public Vector2 Max;


        [HideInInspector] public List<float[]> Stacked;
        //[HideInInspector] public Vector2 Quartile1; // 25%
        //[HideInInspector] public Vector2 Quartile2; // Median
        //[HideInInspector] public Vector2 Quartile3; // 75%

        // TODO: could be optimised by sorting it once
        public void CalculateStatistics()
        {
            if (!Dirty)
                return;
            if (Data.Count == 0)
                return;


            // Goodbye performance


            // Sum value
            Sum = Data
                .Select(points => points.Sum())
                .ToArray();

            // Max value 
            Max = new Vector2(Data.Count-1, Sum.Max());

            // Stacked data
            Stacked = Data
                .Select(points => points.CumulativeSum().ToArray())
                .ToList();

            /*
            Quartile1 = new Vector2
            (
                Data.Percentile(point => point.x, 1f / 4f),
                Data.Percentile(point => point.y, 1f / 4f)
            );
            Quartile2 = new Vector2
            (
                Data.Percentile(point => point.x, 2f / 4f),
                Data.Percentile(point => point.y, 2f / 4f)
            );
            Quartile3 = new Vector2
            (
                Data.Percentile(point => point.x, 3f / 4f),
                Data.Percentile(point => point.y, 3f / 4f)
            );
            */
            Dirty = false;
        }
    }
}