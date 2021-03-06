﻿/*

	SketchUpForDynamo - Trimble(R) SketchUp(R) interface for Autodesk's(R) Dynamo 
	Copyright(C) 2015, Autor: Maximilian Thumfart

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software
    and associated documentation files (the "Software"), to deal in the Software without restriction, 
    including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
    and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
    subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
    INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;
using DSCoreNodesUI;
using Dynamo.Utilities;
using Dynamo.Models;
using SketchUpNET;

namespace SketchUpForDynamo
{

    public static class SketchUp
    {
        /// <summary>
        /// Load SketchUp Model
        /// </summary>
        /// <param name="path">Path to SketchUp file</param>
        [MultiReturn(new[] { "Surfaces", "Layers", "Instances", "Curves", "Edges", "Meshes" })]
        public static Dictionary<string, object> LoadModel(string path, bool includeMeshes = true)
        {
            List<Autodesk.DesignScript.Geometry.Surface> surfaces = new List<Autodesk.DesignScript.Geometry.Surface>();
            List<Autodesk.DesignScript.Geometry.Mesh> meshes = new List<Autodesk.DesignScript.Geometry.Mesh>();
            List<string> layers = new List<string>();
            List<Instance> Instances = new List<Instance>();
            List<List<Autodesk.DesignScript.Geometry.Line>> curves = new List<List<Autodesk.DesignScript.Geometry.Line>>();
            List<Autodesk.DesignScript.Geometry.Line> edges = new List<Autodesk.DesignScript.Geometry.Line>();

            SketchUpNET.SketchUp skp = new SketchUpNET.SketchUp();
            if (skp.LoadModel(path, includeMeshes))
            {

                foreach (Curve c in skp.Curves)
                    curves.Add(c.ToDSGeo());

                foreach (Surface srf in skp.Surfaces)
                {
                    surfaces.Add(srf.ToDSGeo());
                    if (srf.FaceMesh != null)
                        meshes.Add(srf.FaceMesh.ToDSGeo());
                }

                    foreach (Layer l in skp.Layers)
                        layers.Add(l.Name);

                    foreach (Instance i in skp.Instances)
                        Instances.Add(i);

                    foreach (Edge e in skp.Edges)
                        edges.Add(e.ToDSGeo());
                
                
            }

            return new Dictionary<string, object>
            {
                { "Surfaces", surfaces },
                { "Layers", layers },
                { "Instances", Instances },
                { "Curves", curves },
                { "Edges", edges },
                { "Meshes", meshes}
            };
        }

        /// <summary>
        /// SketchUp Component Instance Data
        /// </summary>
        /// <param name="instance">SketchUp Component Instance</param>
        [MultiReturn(new[] { "Surfaces","Curves","Meshes","Edges", "Position", "Scale", "Name", "Parent Name" })]
        public static Dictionary<string, object> GetInstance(Instance instance)
        {
            List<Autodesk.DesignScript.Geometry.Surface> surfaces = new List<Autodesk.DesignScript.Geometry.Surface>();
            List<List<Autodesk.DesignScript.Geometry.Line>> curves = new List<List<Autodesk.DesignScript.Geometry.Line>>();
            List<Autodesk.DesignScript.Geometry.Line> edges = new List<Autodesk.DesignScript.Geometry.Line>();
            List<Autodesk.DesignScript.Geometry.Mesh> meshes = new List<Autodesk.DesignScript.Geometry.Mesh>();

            Autodesk.DesignScript.Geometry.Point p = Autodesk.DesignScript.Geometry.Point.ByCoordinates(instance.Transformation.X, instance.Transformation.Y, instance.Transformation.Z);

            foreach (Surface srf in instance.Parent.Surfaces)
            {
                surfaces.Add(srf.ToDSGeo(instance.Transformation));
                if (srf.FaceMesh != null)
                    meshes.Add(srf.FaceMesh.ToDSGeo(instance.Transformation));
            }
            foreach (Curve c in instance.Parent.Curves)
                curves.Add(c.ToDSGeo(instance.Transformation));
            foreach (Edge e in instance.Parent.Edges)
                edges.Add(e.ToDSGeo(instance.Transformation));

            return new Dictionary<string, object>
            {
                { "Surfaces", surfaces },
                { "Curves", curves },
                { "Meshes", meshes },
                { "Edges", edges },
                { "Position", p },
                { "Scale", instance.Transformation.Scale },
                { "Name", instance.Name },
                { "Parent Name", instance.Parent.Name }

            };
        }

        /// <summary>
        /// Write SketchUp Model
        /// </summary>
        /// <param name="path">Path to SketchUp file</param>
        public static void WriteModel(string path, List<Autodesk.DesignScript.Geometry.Surface> surfaces = null, List<Autodesk.DesignScript.Geometry.Curve> curves = null)
        {
            SketchUpNET.SketchUp skp = new SketchUpNET.SketchUp();
            skp.Surfaces = new List<Surface>();
            skp.Edges = new List<Edge>();
            skp.Curves = new List<Curve>();

            if (curves != null)
            foreach (Autodesk.DesignScript.Geometry.Curve curve in curves)
            {
                if (curve.GetType() == typeof(Autodesk.DesignScript.Geometry.Line))
                {
                    Autodesk.DesignScript.Geometry.Line line = (Autodesk.DesignScript.Geometry.Line)curve;
                    skp.Edges.Add(line.ToSKPGeo());
                }
                else
                {
                    Curve skpcurve = new Curve();
                    skpcurve.Edges = new List<Edge>();
                    foreach (Autodesk.DesignScript.Geometry.Curve tesselated in curve.ApproximateWithArcAndLineSegments())
                    {                      
                        Edge e = new Edge(tesselated.StartPoint.ToSKPGeo(), tesselated.EndPoint.ToSKPGeo());
                        skpcurve.Edges.Add(e);
                    }
                    skp.Curves.Add(skpcurve);
                }
            }

            if (surfaces != null)
            foreach (Autodesk.DesignScript.Geometry.Surface surface in surfaces)
                skp.Surfaces.Add(surface.ToSKPGeo());

            if (System.IO.File.Exists(path))
                skp.AppendToModel(path);
            else 
                skp.WriteNewModel(path);

        }

    }

    [IsVisibleInDynamoLibrary(false)]
    public static class Geometry
    {
        [IsVisibleInDynamoLibrary(false)]
        public static SketchUpNET.Vertex ToSKPGeo(this Autodesk.DesignScript.Geometry.Point p)
        {
            return new Vertex(p.X, p.Y, p.Z);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static SketchUpNET.Edge ToSKPGeo(this Autodesk.DesignScript.Geometry.Line p)
        {
            return new Edge(p.StartPoint.ToSKPGeo(), p.EndPoint.ToSKPGeo());
        }

        [IsVisibleInDynamoLibrary(false)]
        public static SketchUpNET.Surface ToSKPGeo(this Autodesk.DesignScript.Geometry.Surface surface)
        {
            Surface srf = new Surface();
            srf.Vertices = new List<Vertex>();

            foreach (Autodesk.DesignScript.Geometry.Curve curve in surface.PerimeterCurves())
            {
                foreach (Autodesk.DesignScript.Geometry.Curve tesselated in curve.ApproximateWithArcAndLineSegments())
                {
                    srf.Vertices.Add(tesselated.StartPoint.ToSKPGeo());
                }
            }

            return srf;

        }




        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Point ToDSGeo(this SketchUpNET.Vertex v, Transform t)
        {

            if (t == null)
                return Autodesk.DesignScript.Geometry.Point.ByCoordinates(v.X, v.Y, v.Z);
            else
            {
                Vertex transformed = t.GetTransformed(v);
                return Autodesk.DesignScript.Geometry.Point.ByCoordinates(transformed.X, transformed.Y, transformed.Z);
            }
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Vector ToDSGeo(this SketchUpNET.Vector v)
        {
            return Autodesk.DesignScript.Geometry.Vector.ByCoordinates(v.X, v.Y, v.Z);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Line ToDSGeo(this SketchUpNET.Edge v, Transform t = null)
        {
            return Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(v.Start.ToDSGeo(t), v.End.ToDSGeo(t));
        }

        [IsVisibleInDynamoLibrary(false)]
        public static List<Autodesk.DesignScript.Geometry.Line> ToDSGeo(this SketchUpNET.Curve c, Transform t = null)
        {
            List<Autodesk.DesignScript.Geometry.Line> edges = new List<Autodesk.DesignScript.Geometry.Line>();
            foreach (Edge e in c.Edges)
                edges.Add(e.ToDSGeo(t));

            return edges;
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Mesh ToDSGeo(this SketchUpNET.Mesh mesh, Transform t = null)
        {
            List<Autodesk.DesignScript.Geometry.Point> points = new List<Autodesk.DesignScript.Geometry.Point>();
            foreach (var v in mesh.Vertices)
                points.Add(v.ToDSGeo(t));

            List<Autodesk.DesignScript.Geometry.IndexGroup> faces = new List<Autodesk.DesignScript.Geometry.IndexGroup>();
            foreach (var v in mesh.Faces)
                faces.Add(Autodesk.DesignScript.Geometry.IndexGroup.ByIndices(Convert.ToUInt32(v.A),Convert.ToUInt32(v.B),Convert.ToUInt32(v.C)));


            Autodesk.DesignScript.Geometry.Mesh m = Autodesk.DesignScript.Geometry.Mesh.ByPointsFaceIndices(points, faces);

            return m;
        }


        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Surface ToDSGeo(this SketchUpNET.Surface v, Transform t = null)
        {
            List<Autodesk.DesignScript.Geometry.Curve> curves = new List<Autodesk.DesignScript.Geometry.Curve>();
            foreach (Edge c in v.OuterEdges.Edges) curves.Add(c.ToDSGeo(t).ToNurbsCurve());
            int a = 0;
            Autodesk.DesignScript.Geometry.PolyCurve pc = Autodesk.DesignScript.Geometry.PolyCurve.ByJoinedCurves(curves);
            Autodesk.DesignScript.Geometry.Surface s = Autodesk.DesignScript.Geometry.Surface.ByPatch(pc);
            
             List<Autodesk.DesignScript.Geometry.Surface> inner = v.InnerLoops(t);

            foreach(Autodesk.DesignScript.Geometry.Surface srf in inner)
            {
                Autodesk.DesignScript.Geometry.Geometry[] geo = s.Split(srf);
                if (geo.Count() == 2) s = (Autodesk.DesignScript.Geometry.Surface)geo[0];

            }
            return s;
        }

        [IsVisibleInDynamoLibrary(false)]
        public static List<Autodesk.DesignScript.Geometry.Surface> InnerLoops(this SketchUpNET.Surface v, Transform t = null)
        {
            List<Autodesk.DesignScript.Geometry.Surface> surfaces = new List<Autodesk.DesignScript.Geometry.Surface>();

            foreach (Loop loop in v.InnerEdges)
            {
                List<Autodesk.DesignScript.Geometry.Curve> curves = new List<Autodesk.DesignScript.Geometry.Curve>();
                foreach (Edge c in loop.Edges) curves.Add(c.ToDSGeo(t).ToNurbsCurve());
                int a = 0;
                Autodesk.DesignScript.Geometry.PolyCurve pc = Autodesk.DesignScript.Geometry.PolyCurve.ByJoinedCurves(curves);
                Autodesk.DesignScript.Geometry.Surface s = Autodesk.DesignScript.Geometry.Surface.ByPatch(pc);
                surfaces.Add(s);
            }

            return surfaces;
        }
    }
}
