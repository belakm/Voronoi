using Godot;
using System.Collections;
using System.Collections.Generic;
using DelaunatorSharp;
using DelaunatorSharp.Models;
using DelaunatorSharp.Interfaces;
using System;

namespace Voronoi.Map {

    // Class: Map
    // Used on a Godot Node to bind functionality to
    //
    // Generates a colored rectangle shape to prototype feasibility of using voronoi cells for map
    public class Map : Node2D
    {
       
        // Function: _Ready
        // Godot function that fires when bound Node is ready
        public override void _Ready()
        {
            Random rand = new Random();
            Vector2 topLeft = new Vector2(0 - 10,0 - 10);
            Vector2 bottomRight = new Vector2(1024 + 10, 600 + 10);
            float minimumDistance = 24;

            // Create a list of points and create a Voronoi diagram
            MapPointsList pointList = new MapPointsList(topLeft, bottomRight, minimumDistance);
            Delaunator delaunator = new Delaunator(pointList);

            // Iterate thru the points 
            foreach(VoronoiCell cell in delaunator.GetVoronoiCells()) {

                List<Vector2> points = new List<Vector2>();
                bool colorNotSet = true; 

                // Set color to green
                Color tileColor = new Color( // default land
                    ((float) rand.NextDouble() * .4f),
                    .72f - (float) rand.NextDouble() * .1f,
                    ((float) rand.NextDouble() * .2f)
                );

                // Set a blue color to edge tiles + some random
                foreach (Point p in cell.Points) {
                    if (colorNotSet) {
                        if (
                            (((p.X < 120f || p.X > 780f) || (p.Y < 120f || p.Y > 480f)) && rand.NextDouble() < 0.9d)
                            || rand.NextDouble() > 0.92d) { 

                            // Set color to blue
                            tileColor = new Color( 
                                ((float) rand.NextDouble() * .2f), 
                                ((float) rand.NextDouble() * .4f),
                                .9f - (float) rand.NextDouble() * .1f 
                            );
                        }   
                        colorNotSet = false;
                    }
                    points.Add(new Vector2((float) p.X, (float) p.Y));
                }

                // Color in the polygons
                Polygon2D poly = new Polygon2D();
                poly.Color = tileColor;
                poly.Polygon = points.ToArray();
                AddChild(poly);
            }
        }
    }

    /*
    
    Class: MapPointList
    An enumerable list that holds point data.

    Implements:
        IEnumerable<IPoint>

    */
    public class MapPointsList : IEnumerable<IPoint> {

        private List<Point> _points;

        
        /*  
        
        Constructor: MapPointList
        Initializes the list.
        
        Parameters:
            Vector2 topleft - top left boundary for a point
            Vector2 bottomRight - bottom right boundary for a point
            float minimumDistance - minimum distance between points
        
        */
        public MapPointsList(Vector2 topLeft, Vector2 bottomRight, float minimumDistance) {

            this._points = UniformPoissonDiskSampler.SampleRectangle(topLeft, bottomRight, minimumDistance, 1000).ConvertAll(
                new Converter<Vector2, Point>(Vector2ToPoint)
            );
        }

        /*
        Function: Vector2Point

        Creates a <Point> out of a Vector2.

        Parameters:
            Vector2 vector2 - input
        
        Returns: 
            <Point>
        */
        public Point Vector2ToPoint(Vector2 vector2) {
            return new Point(vector2.x, vector2.y);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IPoint> GetEnumerator()
        {
            for (int i = 0; i < _points.Count; i++) {
                yield return _points[i];
            }
        }
    }
}
