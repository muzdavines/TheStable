using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using com.ootii.Geometry;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Graphics
{
    /// <summary>
    /// Enumeration used to define where we will render content
    /// </summary>
    public enum RenderScope
    {
        ALL,
        EDITOR,
        GAME
    }

    /// <summary>
    /// Provides a way to render lines and overlay graphics into the run-time editor. This
    /// component needs to be attached to the camera GameObject.
    /// </summary>
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-5000)]
    public class GraphicsManager : MonoBehaviour
    {
        /// <summary>
        /// Provides access to the GraphicsManager instance
        /// </summary>
        public static GraphicsManager Instance = null;

        /// <summary>
        /// Material used to render
        /// </summary>
        private static Material mSimpleMaterial = null;
        public static Material SimpleMaterial
        {
            get { return mSimpleMaterial; }
        }

        /// <summary>
        /// Grabs the number of lines currently in the render list
        /// </summary>
        public static int LineCount
        {
            get { return mLines.Count + mSceneLines.Count; }
        }

        /// <summary>
        /// Grabs the number of triangles currently in the render list
        /// </summary>
        public static int TriangleCount
        {
            get { return mTriangles.Count + mSceneTriangles.Count; }
        }

        /// <summary>
        /// Grab the number of text entries currently in the render list
        /// </summary>
        public static int TextCount
        {
            get { return mText.Count + mSceneText.Count; }
        }

        /// <summary>
        /// Support vectors for drawing complex shapes
        /// </summary>
        private static List<Vector3> mVectors1 = new List<Vector3>();
        private static List<Vector3> mVectors2 = new List<Vector3>();

        /// <summary>
        /// Timer used to render in the editor while NOT playing
        /// </summary>
        private static Stopwatch mInternalTimer = new Stopwatch();
        private static float InternalTime
        {
            get
            {
                if (Application.isPlaying)
                {
#if UNITY_EDITOR
                    if (!EditorApplication.isPaused)
                    {
#endif
                        return Time.time;
#if UNITY_EDITOR
                    }
#endif
                }

                return mInternalTimer.ElapsedMilliseconds / 1000f;
            }
        }

        /// <summary>
        /// Lines we'll render
        /// </summary>
        private static List<Line> mLines = new List<Line>();
        private static List<Line> mSceneLines = new List<Line>();

        /// <summary>
        /// Triangles we'll render
        /// </summary>
        private static List<Triangle> mTriangles = new List<Triangle>();
        private static List<Triangle> mSceneTriangles = new List<Triangle>();

        /// <summary>
        /// Text we'll render
        /// </summary>
        private static List<Text> mText = new List<Text>();
        private static List<TextString> mSceneText = new List<TextString>();

        /// <summary>
        /// Default shader to use
        /// </summary>
        private static string mShader = "Hidden/GraphicsManagerUI";

        /// <summary>
        /// Default font to use
        /// </summary>
        private static Font mFont = null;

        /// <summary>
        /// Fonts and the extracted texture that we're using
        /// </summary>
        private static Dictionary<Font, TextFont> mFonts = new Dictionary<Font, TextFont>();

        /// <summary>
        /// Shape used over and over
        /// </summary>
        private static Octahedron mOctahedron = null;
        private static IcoSphere mIcoSphere = null;

        /// <summary>
        /// Tracks if we're currently in the update cycle
        /// </summary>
        public static bool IsInUpdate = false;

        /// <summary>
        /// Default shader to load and use
        /// </summary>
        public string _DefaultShader = "Hidden/GraphicsManagerUI";
        public string DefaultShader
        {
            get { return _DefaultShader; }
            set { _DefaultShader = value; }
        }

        /// <summary>
        /// Default font to load and use
        /// </summary>
        public Font _DefaultFont;
        public Font DefaultFont
        {
            get { return _DefaultFont; }
            set { _DefaultFont = value; }
        }

        /// <summary>
        /// Renders the graphics to the scene view
        /// </summary>
        public bool _DrawToSceneView = true;
        public bool DrawToSceneView
        {
            get { return _DrawToSceneView; }
            set { _DrawToSceneView = value; }
        }

        /// <summary>
        /// Renders the graphics to the game view
        /// </summary>
        public bool _DrawToGameView = true;
        public bool DrawToGameView
        {
            get { return _DrawToGameView; }
            set { _DrawToGameView = value; }
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        GraphicsManager()
        {
            mInternalTimer.Start();

#if UNITY_EDITOR

#if !(UNITY_5 || UNITY_2017 || UNITY_2018)
            // Attach to the scene view
            SceneView.duringSceneGui += OnSceneUpdate;
#endif 

#endif
        }

        /// <summary>
        /// Coroutine that will be used to render our lines at the end of the frame
        /// </summary>
        /// <returns></returns>
        private void Start()
        {
            GraphicsManager.Instance = this;

            // Initialize our materials
            GraphicsManager.CreateMaterials();

            // Load the shader
            mShader = _DefaultShader;

            // Load the font texture
            mFont = _DefaultFont;
            AddFont(mFont);
        }

        private void Update()
        {
            //com.ootii.Utilities.Debug.Log.FileWrite("\nGraphicsManager.Update() type:" + (Event.current == null ? "null" : Event.current.type.ToString()));
            //UnityEngine.Debug.Log("GraphicsManager.Update type:" + (Event.current == null ? "null" : Event.current.type.ToString()) + " t:" + GraphicsManager.mSceneTriangles.Count);

            GraphicsManager.IsInUpdate = true;

            // This works because we are forcing the Graphics Manager to Update() first using
            // the Script Execution Order
            GraphicsManager.ClearText();
            GraphicsManager.ClearGraphics();

            // This will clear scene graphics that are created during the last Update() loop.
            if (Application.isPlaying)
            {
                GraphicsManager.ClearSceneText();
                GraphicsManager.ClearSceneGraphics();
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// RENDERS TO SCENE VIEW ONLY - Renders the graphics to the scene view. Called AFTER WaitForEndOfFrame
        /// /// </summary>
        private void OnSceneUpdate(SceneView rSceneView)
        {
            if (Event.current.type.Equals(EventType.Repaint))
            {
                //com.ootii.Utilities.Debug.Log.FileWrite("GraphicsManager.OnSceneUpdate() type:" + (Event.current == null ? "null" : Event.current.type.ToString()));
                //UnityEngine.Debug.Log("GraphicsManager.OnScene lines:" + GraphicsManager.mSceneLines.Count + " event:" + Event.current.type);

                if (_DrawToSceneView)
                {
                    GraphicsManager.RenderSceneText();
                    GraphicsManager.RenderSceneLines();
                    GraphicsManager.RenderSceneTriangles();
                }

                // When we're not running, clear each scene view update
                if (!Application.isPlaying)
                {
                    GraphicsManager.ClearSceneText();
                    GraphicsManager.ClearSceneGraphics();
                }
                // If we're paused, we can't build up OnSceneGUI draws, but we'll lose Update draws
                else if (EditorApplication.isPaused)
                {
                    GraphicsManager.ClearSceneText(0);
                    GraphicsManager.ClearSceneGraphics(0);
                }
            }
        }

#endif

        /// <summary>
        /// RENDERS TO SCENE VIEW ONLY - Renders the graphics to the scene view. Called AFTER WaitForEndOfFrame
        /// - Happens before OnSceneGUI
        /// 
        /// EDIT-TIME
        /// 1. OnDrawGizmos (so, we have to force a repaint to make this happen again)
        /// 2. OnSceneGUI
        /// 3. OnDrawGizmos Recalled
        /// </summary>
        private void OnDrawGizmos()
        {
            //com.ootii.Utilities.Debug.Log.FileWrite("GraphicsManager.OnDrawGizmos() type:" + (Event.current == null ? "null" : Event.current.type.ToString()));
            //UnityEngine.Debug.Log("GraphicsManager.OnDrawGizmos time:" + GraphicsManager.InternalTime + " t:" + GraphicsManager.mSceneTriangles.Count + " event:" + Event.current.type);

            GraphicsManager.IsInUpdate = false;
        }

        /// <summary>
        /// RENDERS TO GAME VIEW ONLY - Renders the graphics to the game view
        /// </summary>
        private void OnPostRender()
        {
            //com.ootii.Utilities.Debug.Log.FileWrite("GraphicsManager.OnPostRender() type:" + (Event.current == null ? "null" : Event.current.type.ToString()));
            //UnityEngine.Debug.Log("GraphicsManager.OnPostRender type:" + (Event.current == null ? "null" : Event.current.type.ToString()) + " t:" + GraphicsManager.mTriangles.Count + " event:" + Event.current.type);

            if (_DrawToGameView)
            {
                GraphicsManager.RenderLines();
                GraphicsManager.RenderTriangles();
            }
        }

        /// <summary>
        /// RENDERS TO GAME VIEW ONLY - Renders text to the game view
        /// </summary>
        private void OnGUI()
        {
            //com.ootii.Utilities.Debug.Log.FileWrite("GraphicsManager.OnGUI() type:" + (Event.current == null ? "null" : Event.current.type.ToString()));
            //UnityEngine.Debug.Log("GraphicsManager.OnGUI time:" + GraphicsManager.InternalTime + " t:" + GraphicsManager.mTriangles.Count + " event:" + Event.current.type);

            if (Event.current.type.Equals(EventType.Repaint))
            {
                if (_DrawToGameView)
                {
                    GraphicsManager.RenderText();
                }
            }
        }

        /// <summary>
        /// Releases any graphics we've allocated
        /// </summary>
        public static void ClearGraphics(int rScope = -1)
        {
            float lTime = InternalTime;

            for (int i = mLines.Count - 1; i >= 0; i--)
            {
                if (rScope == -1 || mLines[i].Scope == rScope)
                {
                    if (mLines[i].ExpirationTime < lTime)
                    {
                        Line.Release(mLines[i]);
                        mLines.RemoveAt(i);
                    }
                }
            }

            for (int i = mTriangles.Count - 1; i >= 0; i--)
            {
                if (rScope == -1 || mTriangles[i].Scope == rScope)
                {
                    if (mTriangles[i].ExpirationTime < lTime)
                    {
                        Triangle.Release(mTriangles[i]);
                        mTriangles.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Releases any graphics we've allocated
        /// </summary>
        public static void ClearSceneGraphics(int rScope = -1)
        {
            float lTime = InternalTime;

            for (int i = mSceneLines.Count - 1; i >= 0; i--)
            {
                if (rScope == -1 || mSceneLines[i].Scope == rScope)
                {
                    if (mSceneLines[i].ExpirationTime < lTime)
                    {
                        Line.Release(mSceneLines[i]);
                        mSceneLines.RemoveAt(i);
                    }
                }
            }

            for (int i = mSceneTriangles.Count - 1; i >= 0; i--)
            {
                if (rScope == -1 || mSceneTriangles[i].Scope == rScope)
                {
                    if (mSceneTriangles[i].ExpirationTime < lTime)
                    {
                        Triangle.Release(mSceneTriangles[i]);
                        mSceneTriangles.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Release any graphics that are allocated
        /// </summary>
        public static void ClearText()
        {
            float lTime = InternalTime;

            for (int i = mText.Count - 1; i >= 0; i--)
            {
                if (mText[i].ExpirationTime < lTime)
                {
                    Text.Release(mText[i]);
                    mText.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Release any graphics that are allocated
        /// </summary>
        public static void ClearSceneText(int rScope = -1)
        {
            float lTime = InternalTime;

            for (int i = mSceneText.Count - 1; i >= 0; i--)
            {
                if (rScope == -1 || mSceneText[i].Scope == rScope)
                {
                    if (mSceneText[i].ExpirationTime < lTime)
                    {
                        TextString.Release(mSceneText[i]);
                        mSceneText.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Draws a line at the end of the frame using Unity's GL 
        /// </summary>
        /// <param name="rStart"></param>
        /// <param name="rEnd"></param>
        /// <param name="rColor"></param>
        public static void DrawLine(Vector3 rStart, Vector3 rEnd, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            Line lLine = Line.Allocate();
            lLine.Scope = (rScope == RenderScope.EDITOR ? (GraphicsManager.IsInUpdate ? 1 : 0) : (GraphicsManager.IsInUpdate ? 3 : 2));
            lLine.Transform = rTransform;
            lLine.Start = rStart;
            lLine.End = rEnd;
            lLine.Color = rColor;
            lLine.ExpirationTime = (rDuration <= 0f ? 0f : InternalTime + rDuration);

            if (rScope == RenderScope.ALL)
            {
                mLines.Add(lLine);

                Line lSceneLine = Line.Allocate(lLine);
                mSceneLines.Add(lSceneLine);
            }
            if (rScope == RenderScope.GAME)
            {
                mLines.Add(lLine);
            }
            else if (rScope == RenderScope.EDITOR)
            {
                mSceneLines.Add(lLine);
            }
        }

        /// <summary>
        /// Drawas a list of connected lines where i0->i1->i2 etc.
        /// </summary>
        /// <param name="rLines"></param>
        /// <param name="rColor"></param>
        public static void DrawLines(List<Vector3> rLines, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            for (int i = 1; i < rLines.Count; i++)
            {
                Line lLine = Line.Allocate();
                lLine.Scope = (rScope == RenderScope.EDITOR ? (GraphicsManager.IsInUpdate ? 1 : 0) : (GraphicsManager.IsInUpdate ? 3 : 2));
                lLine.Transform = rTransform;
                lLine.Start = rLines[i - 1];
                lLine.End = rLines[i];
                lLine.Color = rColor;
                lLine.ExpirationTime = (rDuration <= 0f ? 0f : InternalTime + rDuration);

                if (rScope == RenderScope.ALL)
                {
                    mLines.Add(lLine);

                    Line lSceneLine = Line.Allocate(lLine);
                    mSceneLines.Add(lSceneLine);
                }
                if (rScope == RenderScope.GAME)
                {
                    mLines.Add(lLine);
                }
                else if (rScope == RenderScope.EDITOR)
                {
                    mSceneLines.Add(lLine);
                }
            }
        }

        /// <summary>
        /// Draws a triangle at the end of the frame using Unity's GL
        /// </summary>
        /// <param name="rPoint1"></param>
        /// <param name="rPoint2"></param>
        /// <param name="rPoint3"></param>
        /// <param name="rColor"></param>
        /// <param name="rFill"></param>
        /// <param name="rTransform"></param>
        public static void DrawTriangle(Vector3 rPoint1, Vector3 rPoint2, Vector3 rPoint3, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            Triangle lTriangle = Triangle.Allocate();
            lTriangle.Scope = (rScope == RenderScope.EDITOR ? (GraphicsManager.IsInUpdate ? 1 : 0) : (GraphicsManager.IsInUpdate ? 3 : 2));
            lTriangle.Transform = rTransform;
            lTriangle.Point1 = rPoint1;
            lTriangle.Point2 = rPoint2;
            lTriangle.Point3 = rPoint3;
            lTriangle.Color = rColor;
            lTriangle.ExpirationTime = (rDuration <= 0f ? 0f : InternalTime + rDuration);

            if (rScope == RenderScope.ALL)
            {
                mTriangles.Add(lTriangle);

                Triangle lSceneTriangle = Triangle.Allocate(lTriangle);
                mSceneTriangles.Add(lSceneTriangle);
            }
            if (rScope == RenderScope.GAME)
            {
                mTriangles.Add(lTriangle);
            }
            else if (rScope == RenderScope.EDITOR)
            {
                mSceneTriangles.Add(lTriangle);
            }
        }

        /// <summary>
        /// Draws a triangle at the end of the frame using Unity's GL
        /// </summary>
        /// <param name="rPoint1"></param>
        /// <param name="rPoint2"></param>
        /// <param name="rPoint3"></param>
        /// <param name="rColor"></param>
        /// <param name="rFill"></param>
        /// <param name="rTransform"></param>
        public static void DrawTriangles(List<Vector3> rPoints, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            if (rPoints.Count == 0) { return; }
            if (rPoints.Count % 3 != 0) { return; }

            for (int i = 2; i < rPoints.Count; i += 3)
            {
                Triangle lTriangle = Triangle.Allocate();
                lTriangle.Scope = (rScope == RenderScope.EDITOR ? (GraphicsManager.IsInUpdate ? 1 : 0) : (GraphicsManager.IsInUpdate ? 3 : 2));
                lTriangle.Transform = rTransform;
                lTriangle.Point1 = rPoints[i - 2];
                lTriangle.Point2 = rPoints[i - 1];
                lTriangle.Point3 = rPoints[i];
                lTriangle.Color = rColor;
                lTriangle.ExpirationTime = (rDuration <= 0f ? 0f : InternalTime + rDuration);

                if (rScope == RenderScope.ALL)
                {
                    mTriangles.Add(lTriangle);

                    Triangle lSceneTriangle = Triangle.Allocate(lTriangle);
                    mSceneTriangles.Add(lSceneTriangle);
                }
                if (rScope == RenderScope.GAME)
                {
                    mTriangles.Add(lTriangle);
                }
                else if (rScope == RenderScope.EDITOR)
                {
                    mSceneTriangles.Add(lTriangle);
                }
            }
        }

        /// <summary>
        /// Draws a box given the specified dimensions
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rHeight"></param>
        /// <param name="rWidth"></param>
        /// <param name="rDepth"></param>
        /// <param name="rTransform"></param>
        /// <param name="rDuration"></param>
        public static void DrawBox(Vector3 rCenter, float rWidth, float rHeight, float rDepth, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            float lHalfWidth = rWidth * 0.5f;
            float lHalfHeight = rHeight * 0.5f;
            float lHalfDepth = rDepth * 0.5f;

            DrawLine(rCenter + new Vector3(lHalfWidth, lHalfHeight, lHalfDepth), rCenter + new Vector3(-lHalfWidth, lHalfHeight, lHalfDepth), rColor, rTransform, rDuration, rScope);
            DrawLine(rCenter + new Vector3(lHalfWidth, lHalfHeight, lHalfDepth), rCenter + new Vector3(lHalfWidth, -lHalfHeight, lHalfDepth), rColor, rTransform, rDuration, rScope);
            DrawLine(rCenter + new Vector3(lHalfWidth, lHalfHeight, lHalfDepth), rCenter + new Vector3(lHalfWidth, lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration, rScope);
            DrawLine(rCenter + new Vector3(-lHalfWidth, lHalfHeight, lHalfDepth), rCenter + new Vector3(-lHalfWidth, -lHalfHeight, lHalfDepth), rColor, rTransform, rDuration, rScope);
            DrawLine(rCenter + new Vector3(-lHalfWidth, lHalfHeight, lHalfDepth), rCenter + new Vector3(-lHalfWidth, lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration, rScope);
            DrawLine(rCenter + new Vector3(-lHalfWidth, lHalfHeight, -lHalfDepth), rCenter + new Vector3(lHalfWidth, lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration, rScope);
            DrawLine(rCenter + new Vector3(-lHalfWidth, lHalfHeight, -lHalfDepth), rCenter + new Vector3(-lHalfWidth, -lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration, rScope);
            DrawLine(rCenter + new Vector3(-lHalfWidth, -lHalfHeight, -lHalfDepth), rCenter + new Vector3(-lHalfWidth, -lHalfHeight, lHalfDepth), rColor, rTransform, rDuration, rScope);
            DrawLine(rCenter + new Vector3(-lHalfWidth, -lHalfHeight, -lHalfDepth), rCenter + new Vector3(lHalfWidth, -lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration, rScope);
            DrawLine(rCenter + new Vector3(lHalfWidth, -lHalfHeight, -lHalfDepth), rCenter + new Vector3(lHalfWidth, -lHalfHeight, lHalfDepth), rColor, rTransform, rDuration, rScope);
            DrawLine(rCenter + new Vector3(lHalfWidth, -lHalfHeight, -lHalfDepth), rCenter + new Vector3(lHalfWidth, lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration, rScope);
            DrawLine(rCenter + new Vector3(lHalfWidth, -lHalfHeight, lHalfDepth), rCenter + new Vector3(-lHalfWidth, -lHalfHeight, lHalfDepth), rColor, rTransform, rDuration, rScope);
        }

        /// <summary>
        /// Draws a bounding box given the coordinates
        /// </summary>
        /// <param name="rBounds"></param>
        /// <param name="rColor"></param>
        /// <param name="rTransform"></param>
        /// <param name="rDuration"></param>
        public static void DrawBox(Bounds rBounds, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            DrawBox(rBounds.center, rBounds.size.x, rBounds.size.y, rBounds.size.z, rColor, rTransform, rDuration, rScope);
        }

        /// <summary>
        /// Draws a bounding box given the coordinates
        /// </summary>
        /// <param name="rBounds"></param>
        /// <param name="rColor"></param>
        /// <param name="rTransform"></param>
        /// <param name="rDuration"></param>
        public static void DrawCollider(BoxCollider rColldier, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            if (rColldier == null) { return; }

            mVectors1.Clear();
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, 0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, 0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, 0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, 0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, -0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, -0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, -0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, -0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);

            for (int i = 0; i < mVectors1.Count; i++)
            {
                mVectors1[i] = rColldier.transform.TransformPoint(mVectors1[i]);
            }

            DrawLine(mVectors1[0], mVectors1[1], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[1], mVectors1[2], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[2], mVectors1[3], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[3], mVectors1[0], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[4], mVectors1[5], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[5], mVectors1[6], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[6], mVectors1[7], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[7], mVectors1[4], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[0], mVectors1[4], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[1], mVectors1[5], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[2], mVectors1[6], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[3], mVectors1[7], rColor, rTransform, rDuration, rScope);
        }

        /// <summary>
        /// Draws a bounding box given the coordinates
        /// </summary>
        /// <param name="rBounds"></param>
        /// <param name="rColor"></param>
        /// <param name="rTransform"></param>
        /// <param name="rDuration"></param>
        public static void DrawSolidCollider(BoxCollider rColldier, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            if (rColldier == null) { return; }

            mVectors1.Clear();
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, 0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, 0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, 0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, 0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, -0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, -0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, -0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, -0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);

            for (int i = 0; i < mVectors1.Count; i++)
            {
                mVectors1[i] = rColldier.transform.TransformPoint(mVectors1[i]);
            }

            Color lColor = new Color(rColor.r, rColor.g, rColor.b, 0.1f);
            DrawTriangle(mVectors1[0], mVectors1[1], mVectors1[2], lColor, rTransform, rDuration, rScope);
            DrawTriangle(mVectors1[0], mVectors1[2], mVectors1[3], lColor, rTransform, rDuration, rScope);

            DrawTriangle(mVectors1[0], mVectors1[1], mVectors1[5], lColor, rTransform, rDuration, rScope);
            DrawTriangle(mVectors1[0], mVectors1[5], mVectors1[4], lColor, rTransform, rDuration, rScope);

            DrawTriangle(mVectors1[0], mVectors1[3], mVectors1[7], lColor, rTransform, rDuration, rScope);
            DrawTriangle(mVectors1[0], mVectors1[7], mVectors1[4], lColor, rTransform, rDuration, rScope);

            DrawTriangle(mVectors1[6], mVectors1[5], mVectors1[1], lColor, rTransform, rDuration, rScope);
            DrawTriangle(mVectors1[6], mVectors1[1], mVectors1[2], lColor, rTransform, rDuration, rScope);

            DrawTriangle(mVectors1[6], mVectors1[7], mVectors1[4], lColor, rTransform, rDuration, rScope);
            DrawTriangle(mVectors1[6], mVectors1[4], mVectors1[5], lColor, rTransform, rDuration, rScope);

            DrawTriangle(mVectors1[6], mVectors1[2], mVectors1[3], lColor, rTransform, rDuration, rScope);
            DrawTriangle(mVectors1[6], mVectors1[3], mVectors1[7], lColor, rTransform, rDuration, rScope);

            DrawLine(mVectors1[0], mVectors1[1], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[1], mVectors1[2], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[2], mVectors1[3], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[3], mVectors1[0], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[4], mVectors1[5], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[5], mVectors1[6], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[6], mVectors1[7], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[7], mVectors1[4], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[0], mVectors1[4], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[1], mVectors1[5], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[2], mVectors1[6], rColor, rTransform, rDuration, rScope);
            DrawLine(mVectors1[3], mVectors1[7], rColor, rTransform, rDuration, rScope);
        }

        /// <summary>
        /// Renders a simple circle whose normal is Vector3.up
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawCircle(Vector3 rCenter, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            DrawCircle(rCenter, rRadius, rColor, Vector3.up, rTransform, rDuration, rScope);
        }

        /// <summary>
        /// Renders a simple circle whose normal is Vector3.up
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawCircle(Vector3 rCenter, float rRadius, Color rColor, Vector3 rNormal, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            int lCount = 36;
            mVectors1.Clear();

            Quaternion lRotation = Quaternion.AngleAxis(360f / (float)(lCount - 1), rNormal);

            Quaternion lToRotation = Quaternion.FromToRotation(Vector3.up, rNormal);

            Vector3 lSurfacePoint = (lToRotation * Vector3.forward) * rRadius;
            for (int i = 0; i < lCount; i++)
            {
                mVectors1[i] = rCenter + lSurfacePoint;
                lSurfacePoint = lRotation * lSurfacePoint;
            }

            for (int i = 1; i < lCount; i++)
            {
                DrawLine(mVectors1[i - 1], mVectors1[i], rColor, rTransform, rDuration, rScope);
            }
        }

        /// <summary>
        /// Renders a simple circle whose normal is Vector3.up
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidCircle(Vector3 rCenter, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            DrawSolidCircle(rCenter, rRadius, rColor, Vector3.up, rTransform, rDuration, rScope);
        }

        /// <summary>
        /// Renders a simple circle whose normal is Vector3.up
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidCircle(Vector3 rCenter, float rRadius, Color rColor, Vector3 rNormal, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            int lCount = 36;
            mVectors1.Clear();

            Quaternion lRotation = Quaternion.AngleAxis(360f / (float)(lCount - 1), rNormal);
            Vector3 lSurfacePoint = Vector3.forward * rRadius;
            for (int i = 0; i < lCount; i++)
            {
                mVectors1.Add(rCenter + lSurfacePoint);
                lSurfacePoint = lRotation * lSurfacePoint;
            }

            Color lColor = new Color(rColor.r, rColor.g, rColor.b, 0.1f);
            for (int i = 1; i < lCount; i++)
            {
                DrawLine(mVectors1[i - 1], mVectors1[i], rColor, rTransform, rDuration, rScope);
                DrawTriangle(rCenter, mVectors1[i - 1], mVectors1[i], lColor, rTransform, rDuration, rScope);
            }
        }

        /// <summary>
        /// Renders a simple circle whose normal is Vector3.up
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidCone(Vector3 rPosition, Vector3 rDirection, float rHeight, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            Vector3 lRight = Vector3.Cross(rDirection, Vector3.up);
            if (lRight == Vector3.zero) { lRight = Vector3.right; }

            Vector3 lPoint = rPosition + (rDirection * rHeight);
            Vector3 lBaseCenter = rPosition;
          
            int lCount = 36;
            mVectors1.Clear();

            Quaternion lRotation = Quaternion.AngleAxis(360f / (float)(lCount - 1), rDirection);
            Vector3 lSurfacePoint = lRight * rRadius;
            for (int i = 0; i < lCount; i++)
            {
                mVectors1.Add(lBaseCenter + lSurfacePoint);
                lSurfacePoint = lRotation * lSurfacePoint;
            }

            for (int i = 1; i < lCount; i++)
            {
                // End circle
                DrawLine(mVectors1[i - 1], mVectors1[i], rColor, rTransform, rDuration, rScope);
                DrawTriangle(lBaseCenter, mVectors1[i - 1], mVectors1[i], rColor, rTransform, rDuration, rScope);

                // Cone wall
                DrawTriangle(lPoint, mVectors1[i - 1], mVectors1[i], rColor, rTransform, rDuration, rScope);
            }
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawArc(Vector3 rCenter, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            DrawArc(rCenter, Vector3.up, rFrom, rAngle, rRadius, rColor, rTransform, rDuration, rScope);
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawArc(Vector3 rCenter, Vector3 rNormal, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            int lCount = 36;
            mVectors1.Clear();

            Quaternion lRotation = Quaternion.AngleAxis(rAngle / (float)(lCount - 1), rNormal);
            Vector3 lSurfacePoint = rFrom.normalized * rRadius;
            for (int i = 0; i < lCount; i++)
            {
                mVectors1.Add(rCenter + lSurfacePoint);
                lSurfacePoint = lRotation * lSurfacePoint;
            }

            for (int i = 1; i < lCount; i++)
            {
                DrawLine(mVectors1[i - 1], mVectors1[i], rColor, rTransform, rDuration, rScope);
            }
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidArc(Vector3 rCenter, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            DrawSolidArc(rCenter, Vector3.up, rFrom, rAngle, rRadius, rColor, rTransform, rDuration, rScope);
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidArc(Vector3 rCenter, Vector3 rNormal, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            int lCount = 36;
            mVectors1.Clear();

            Quaternion lRotation = Quaternion.AngleAxis(rAngle / (float)(lCount - 1), rNormal);
            Vector3 lSurfacePoint = rFrom.normalized * rRadius;
            for (int i = 0; i < lCount; i++)
            {
                mVectors1[i] = rCenter + lSurfacePoint;
                lSurfacePoint = lRotation * lSurfacePoint;
            }

            for (int i = 1; i < lCount; i++)
            {
                DrawTriangle(rCenter, mVectors1[i - 1], mVectors1[i], rColor, rTransform, rDuration, rScope);
            }
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidCenteredArc(Vector3 rCenter, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            DrawSolidCenteredArc(rCenter, Vector3.up, rFrom, rAngle, rRadius, rColor, rTransform, rDuration, rScope);
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidCenteredArc(Vector3 rCenter, Vector3 rNormal, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            int lCount = 36;
            mVectors1.Clear();

            Quaternion lRotation = Quaternion.AngleAxis(rAngle / (float)(lCount - 1), rNormal);

            Vector3 lSurfacePoint = Quaternion.AngleAxis(-rAngle * 0.5f, rNormal) * (rFrom.normalized * rRadius);
            for (int i = 0; i < lCount; i++)
            {
                mVectors1[i] = rCenter + lSurfacePoint;
                lSurfacePoint = lRotation * lSurfacePoint;
            }

            for (int i = 1; i < lCount; i++)
            {
                DrawTriangle(rCenter, mVectors1[i - 1], mVectors1[i], rColor, rTransform, rDuration, rScope);
            }
        }

        /// <summary>
        /// Draws an arrow from start to end (with end being the tip) at the end of the frame using Unity's GL 
        /// </summary>
        /// <param name="rStart"></param>
        /// <param name="rEnd"></param>
        /// <param name="rColor"></param>
        public static void DrawArrow(Vector3 rStart, Vector3 rEnd, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            DrawLine(rStart, rEnd, rColor, rTransform, rDuration, rScope);
            DrawPoint(rEnd, rColor, rTransform, rDuration, rScope);
        }

        /// <summary>
        /// Draws a wire frustum
        /// </summary>
        /// <param name="rPosition"></param>
        /// <param name="rRotation"></param>
        /// <param name="rHAngle"></param>
        /// <param name="rVAngle"></param>
        /// <param name="rDistance"></param>
        /// <param name="rColor"></param>
        public static void DrawFrustum(Vector3 rPosition, Quaternion rRotation, float rHAngle, float rVAngle, float rMinDistance, float rMaxDistance, Color rColor, bool rIsSpherical = true, RenderScope rScope = RenderScope.EDITOR)
        {
            if (rHAngle == 0 || rVAngle == 0 || rMaxDistance == 0) { return; }

            int lSteps = 10;
            int lStepsPlus1 = lSteps + 1;

            float lHalfHAngle = rHAngle * 0.5f;
            float lHalfVAngle = rVAngle * 0.5f;
            Vector3 lPoint = Vector3.zero;

            mVectors1.Clear();
            mVectors2.Clear();
            for (float lVTheta = -lHalfVAngle; lVTheta <= lHalfVAngle; lVTheta += (rVAngle / lSteps))
            {
                float lTrueVTheta = -(lVTheta * Mathf.Deg2Rad);
                float lY = Mathf.Sin(lTrueVTheta);

                for (float lHTheta = -lHalfHAngle; lHTheta <= lHalfHAngle; lHTheta += (rHAngle / lSteps))
                {
                    float lTrueHTheta = -(lHTheta * Mathf.Deg2Rad) + 1.57079f;
                    float lX = Mathf.Cos(lTrueHTheta) * (rIsSpherical ? Mathf.Cos(lTrueVTheta) : 1f);
                    float lZ = Mathf.Sin(lTrueHTheta) * (rIsSpherical ? Mathf.Cos(lTrueVTheta) : 1f);

                    lPoint.x = rMinDistance * lX;
                    lPoint.y = rMinDistance * lY;
                    lPoint.z = rMinDistance * lZ;
                    mVectors1.Add(rPosition + (rRotation * lPoint));

                    lPoint.x = rMaxDistance * lX;
                    lPoint.y = rMaxDistance * lY;
                    lPoint.z = rMaxDistance * lZ;
                    mVectors2.Add(rPosition + (rRotation * lPoint));
                }
            }

            if (rVAngle < 360f)
            {
                // Draw the horizontal lines
                for (int x = 0; x < lSteps; x++)
                {
                    DrawLine(mVectors2[x], mVectors2[x + 1], rColor, null, 0f, rScope);
                    DrawLine(mVectors2[(lSteps * lStepsPlus1) + x], mVectors2[(lSteps * lStepsPlus1) + x + 1], rColor, null, 0f, rScope);
                    DrawLine(mVectors1[x], mVectors1[x + 1], rColor, null, 0f, rScope);
                    DrawLine(mVectors1[(lSteps * lStepsPlus1) + x], mVectors1[(lSteps * lStepsPlus1) + x + 1], rColor, null, 0f, rScope);
                }
            }

            if (rHAngle < 360f)
            {
                // Draw the vertical lines
                for (int y = 0; y < lSteps; y++)
                {
                    DrawLine(mVectors2[(y * lStepsPlus1) + 0], mVectors2[((y + 1) * lStepsPlus1) + 0], rColor, null, 0f, rScope);
                    DrawLine(mVectors2[(y * lStepsPlus1) + lSteps], mVectors2[((y + 1) * lStepsPlus1) + lSteps], rColor, null, 0f, rScope);
                    DrawLine(mVectors1[(y * lStepsPlus1) + 0], mVectors1[((y + 1) * lStepsPlus1) + 0], rColor, null, 0f, rScope);
                    DrawLine(mVectors1[(y * lStepsPlus1) + lSteps], mVectors1[((y + 1) * lStepsPlus1) + lSteps], rColor, null, 0f, rScope);

                    DrawLine(mVectors1[(y * lStepsPlus1) + (lSteps / 2)], mVectors1[((y + 1) * lStepsPlus1) + (lSteps / 2)], rColor, null, 0f, rScope);
                    DrawLine(mVectors2[(y * lStepsPlus1) + (lSteps / 2)], mVectors2[((y + 1) * lStepsPlus1) + (lSteps / 2)], rColor, null, 0f, rScope);
                }
            }

            if (rHAngle < 360f && rVAngle < 360f)
            {
                // Draw the corners
                DrawLine(mVectors1[0], mVectors2[0], rColor, null, 0f, rScope);
                DrawLine(mVectors1[lSteps], mVectors2[lSteps], rColor, null, 0f, rScope);
                DrawLine(mVectors1[(lSteps * lStepsPlus1) + 0], mVectors2[(lSteps * lStepsPlus1) + 0], rColor, null, 0f, rScope);
                DrawLine(mVectors1[(lSteps * lStepsPlus1) + lSteps], mVectors2[(lSteps * lStepsPlus1) + lSteps], rColor, null, 0f, rScope);
            }
        }

        /// <summary>
        /// Draws a solid frustum
        /// </summary>
        /// <param name="rPosition"></param>
        /// <param name="rRotation"></param>
        /// <param name="rHAngle"></param>
        /// <param name="rVAngle"></param>
        /// <param name="rDistance"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidFrustum(Vector3 rPosition, Quaternion rRotation, float rHAngle, float rVAngle, float rMinDistance, float rMaxDistance, Color rColor, bool rIsSpherical = true, RenderScope rScope = RenderScope.EDITOR)
        {
            if (rHAngle == 0 || rVAngle == 0 || rMaxDistance == 0) { return; }

            int lSteps = 10;
            int lStepsPlus1 = lSteps + 1;

            float lHalfHAngle = rHAngle * 0.5f;
            float lHalfVAngle = rVAngle * 0.5f;
            Vector3 lPoint = Vector3.zero;

            mVectors1.Clear();
            mVectors2.Clear();
            for (float lVTheta = -lHalfVAngle; lVTheta <= lHalfVAngle; lVTheta += (rVAngle / lSteps))
            {
                float lTrueVTheta = -(lVTheta * Mathf.Deg2Rad);
                float lY = Mathf.Sin(lTrueVTheta);

                for (float lHTheta = -lHalfHAngle; lHTheta <= lHalfHAngle; lHTheta += (rHAngle / lSteps))
                {
                    float lTrueHTheta = -(lHTheta * Mathf.Deg2Rad) + 1.57079f;
                    float lX = Mathf.Cos(lTrueHTheta) * (rIsSpherical ? Mathf.Cos(lTrueVTheta) : 1f);
                    float lZ = Mathf.Sin(lTrueHTheta) * (rIsSpherical ? Mathf.Cos(lTrueVTheta) : 1f);

                    lPoint.x = rMinDistance * lX;
                    lPoint.y = rMinDistance * lY;
                    lPoint.z = rMinDistance * lZ;
                    mVectors1.Add(rPosition + (rRotation * lPoint));

                    lPoint.x = rMaxDistance * lX;
                    lPoint.y = rMaxDistance * lY;
                    lPoint.z = rMaxDistance * lZ;
                    mVectors2.Add(rPosition + (rRotation * lPoint));
                }
            }

            // Draw all the surfaces
            Color lColor = new Color(rColor.r, rColor.g, rColor.b, 0.1f);

            if (rMinDistance > 0f)
            {
                // Draw the near surface
                for (int y = 0; y < lSteps; y++)
                {
                    for (int x = 0; x < lSteps; x++)
                    {
                        DrawTriangle(mVectors1[((y + 1) * lStepsPlus1) + x], mVectors1[(y * lStepsPlus1) + x], mVectors1[((y + 1) * lStepsPlus1) + x + 1], lColor, null, 0f, rScope);
                        DrawTriangle(mVectors1[(y * lStepsPlus1) + x], mVectors1[(y * lStepsPlus1) + x + 1], mVectors1[((y + 1) * lStepsPlus1) + x + 1], lColor, null, 0f, rScope);
                    }
                }
            }

            // Draw the far surface
            for (int y = 0; y < lSteps; y++)
            {
                for (int x = 0; x < lSteps; x++)
                {
                    DrawTriangle(mVectors2[((y + 1) * lStepsPlus1) + x], mVectors2[(y * lStepsPlus1) + x], mVectors2[((y + 1) * lStepsPlus1) + x + 1], lColor, null, 0f, rScope);
                    DrawTriangle(mVectors2[(y * lStepsPlus1) + x], mVectors2[(y * lStepsPlus1) + x + 1], mVectors2[((y + 1) * lStepsPlus1) + x + 1], lColor, null, 0f, rScope);
                }
            }

            if (rVAngle < 360f)
            {
                // Draw the top surface
                for (int x = 0; x < lSteps; x++)
                {
                    DrawTriangle(mVectors2[x], mVectors1[x], mVectors2[x + 1], lColor, null, 0f, rScope);
                    DrawTriangle(mVectors1[x], mVectors1[x + 1], mVectors2[x + 1], lColor, null, 0f, rScope);
                }

                // Draw the bottom surface
                for (int x = 0; x < lSteps; x++)
                {
                    DrawTriangle(mVectors2[(lSteps * lStepsPlus1) + x], mVectors1[(lSteps * lStepsPlus1) + x], mVectors2[(lSteps * lStepsPlus1) + x + 1], lColor, null, 0f, rScope);
                    DrawTriangle(mVectors1[(lSteps * lStepsPlus1) + x], mVectors1[(lSteps * lStepsPlus1) + x + 1], mVectors2[(lSteps * lStepsPlus1) + x + 1], lColor, null, 0f, rScope);
                }
            }

            if (rHAngle < 360f)
            {
                // Draw the left surface
                for (int y = 0; y < lSteps; y++)
                {
                    DrawTriangle(mVectors2[((y + 1) * lStepsPlus1) + 0], mVectors2[(y * lStepsPlus1) + 0], mVectors1[((y + 1) * lStepsPlus1) + 0], lColor, null, 0f, rScope);
                    DrawTriangle(mVectors2[(y * lStepsPlus1) + 0], mVectors1[(y * lStepsPlus1) + 0], mVectors1[((y + 1) * lStepsPlus1) + 0], lColor, null, 0f, rScope);
                }

                // Draw the right surface
                for (int y = 0; y < lSteps; y++)
                {
                    DrawTriangle(mVectors2[((y + 1) * lStepsPlus1) + lSteps], mVectors2[(y * lStepsPlus1) + lSteps], mVectors1[((y + 1) * lStepsPlus1) + lSteps], lColor, null, 0f, rScope);
                    DrawTriangle(mVectors2[(y * lStepsPlus1) + lSteps], mVectors1[(y * lStepsPlus1) + lSteps], mVectors1[((y + 1) * lStepsPlus1) + lSteps], lColor, null, 0f, rScope);
                }
            }

            if (rVAngle < 360f)
            {
                // Draw the horizontal lines
                for (int x = 0; x < lSteps; x++)
                {
                    DrawLine(mVectors2[x], mVectors2[x + 1], rColor, null, 0f, rScope);
                    DrawLine(mVectors2[(lSteps * lStepsPlus1) + x], mVectors2[(lSteps * lStepsPlus1) + x + 1], rColor, null, 0f, rScope);
                    DrawLine(mVectors1[x], mVectors1[x + 1], rColor, null, 0f, rScope);
                    DrawLine(mVectors1[(lSteps * lStepsPlus1) + x], mVectors1[(lSteps * lStepsPlus1) + x + 1], rColor, null, 0f, rScope);
                }
            }

            if (rHAngle < 360f)
            {
                // Draw the vertical lines
                for (int y = 0; y < lSteps; y++)
                {
                    DrawLine(mVectors2[(y * lStepsPlus1) + 0], mVectors2[((y + 1) * lStepsPlus1) + 0], rColor, null, 0f, rScope);
                    DrawLine(mVectors2[(y * lStepsPlus1) + lSteps], mVectors2[((y + 1) * lStepsPlus1) + lSteps], rColor, null, 0f, rScope);
                    DrawLine(mVectors1[(y * lStepsPlus1) + 0], mVectors1[((y + 1) * lStepsPlus1) + 0], rColor, null, 0f, rScope);
                    DrawLine(mVectors1[(y * lStepsPlus1) + lSteps], mVectors1[((y + 1) * lStepsPlus1) + lSteps], rColor, null, 0f, rScope);
                }
            }

            if (rHAngle < 360f && rVAngle < 360f)
            {
                // Draw the corners
                DrawLine(mVectors1[0], mVectors2[0], rColor, null, 0f, rScope);
                DrawLine(mVectors1[lSteps], mVectors2[lSteps], rColor, null, 0f, rScope);
                DrawLine(mVectors1[(lSteps * lStepsPlus1) + 0], mVectors2[(lSteps * lStepsPlus1) + 0], rColor, null, 0f, rScope);
                DrawLine(mVectors1[(lSteps * lStepsPlus1) + lSteps], mVectors2[(lSteps * lStepsPlus1) + lSteps], rColor, null, 0f, rScope);
            }
        }

        /// <summary>
        /// Draws a diamond that represents a single point.
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rColor"></param>
        public static void DrawPoint(Vector3 rCenter, Color rColor, Transform rTransform = null, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            if (mOctahedron == null) { mOctahedron = new Octahedron(); }

            float lScale = 0.075f;
            for (int i = 0; i < mOctahedron.Triangles.Length; i = i + 3)
            {
                DrawTriangle(rCenter + (mOctahedron.Vertices[mOctahedron.Triangles[i]] * lScale), rCenter + (mOctahedron.Vertices[mOctahedron.Triangles[i + 1]] * lScale), rCenter + (mOctahedron.Vertices[mOctahedron.Triangles[i + 2]] * lScale), rColor, rTransform, rDuration, rScope);
            }
        }

        /// <summary>
        /// Draws a diamond that represents a single point.
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rColor"></param>
        public static void DrawQuaternion(Vector3 rCenter, Quaternion rRotation, float rScale = 1f, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            DrawLine(rCenter, rCenter + rRotation.Forward() * rScale, Color.blue, null, rDuration, rScope);
            DrawLine(rCenter, rCenter + rRotation.Right() * rScale, Color.red, null, rDuration, rScope);
            DrawLine(rCenter, rCenter + rRotation.Up() * rScale, Color.green, null, rDuration, rScope);
        }

        /// <summary>
        /// Draws a wire capsule
        /// </summary>
        public static void DrawCapsule(Vector3 rStart, Vector3 rEnd, float rRadius, Color rColor, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            Vector3 lDirection = (rEnd - rStart).normalized;
            Quaternion lRotation = (lDirection.sqrMagnitude == 0f ? Quaternion.identity : Quaternion.LookRotation(lDirection, Vector3.up));

            Vector3 lForward = lRotation * Vector3.forward;
            Vector3 lRight = lRotation * Vector3.right;
            Vector3 lUp = lRotation * Vector3.up;

            DrawArc(rStart, lForward, lUp, 360f, rRadius, rColor, null, rDuration, rScope);
            DrawArc(rStart, lUp, lRight, 180f, rRadius, rColor, null, rDuration, rScope);
            DrawArc(rStart, lRight, -lUp, 180f, rRadius, rColor, null, rDuration, rScope);

            DrawArc(rEnd, lForward, lUp, 360f, rRadius, rColor, null, rDuration, rScope);
            DrawArc(rEnd, lUp, -lRight, 180f, rRadius, rColor, null, rDuration, rScope);
            DrawArc(rEnd, lRight, lUp, 180f, rRadius, rColor, null, rDuration, rScope);

            DrawLine(rStart + (lRight * rRadius), rEnd + (lRight * rRadius), rColor, null, rDuration, rScope);
            DrawLine(rStart + (-lRight * rRadius), rEnd + (-lRight * rRadius), rColor, null, rDuration, rScope);
            DrawLine(rStart + (lUp * rRadius), rEnd + (lUp * rRadius), rColor, null, rDuration, rScope);
            DrawLine(rStart + (-lUp * rRadius), rEnd + (-lUp * rRadius), rColor, null, rDuration, rScope);
        }

        /// <summary>
        /// Draws a wire frame sphere into the scene and game
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        /// <param name="rDuration"></param>
        public static void DrawSphere(Vector3 rCenter, float rRadius, Color rColor, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            Vector3 lForward = Vector3.forward;
            Vector3 lRight = Vector3.right;
            Vector3 lUp = Vector3.up;

            DrawArc(rCenter, lForward, lUp, 360f, rRadius, rColor, null, rDuration, rScope);
            DrawArc(rCenter, lUp, lRight, 360f, rRadius, rColor, null, rDuration, rScope);
            DrawArc(rCenter, lRight, -lUp, 360f, rRadius, rColor, null, rDuration, rScope);
        }

        /// <summary>
        /// Draws a solid shpere into the scene and game
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        /// <param name="rDuration"></param>
        /// <param name="rScope"></param>
        public static void DrawSolidSphere(Vector3 rCenter, float rRadius, Color rColor, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            if (mIcoSphere == null) 
            { 
                mIcoSphere = new IcoSphere();
                mIcoSphere.CreateSphere(4);
            }

            for (int i = 2; i < mIcoSphere.TriangleList.Count; i += 3)
            {
                DrawTriangle(rCenter + (mIcoSphere.TriangleList[i - 2] * rRadius), rCenter + (mIcoSphere.TriangleList[i - 1] * rRadius), rCenter + (mIcoSphere.TriangleList[i] * rRadius), rColor, null, rDuration, rScope);
            }
        }

        /// <summary>
        /// Renders a camera facing texture to the screen given the world position and size
        /// </summary>
        /// <param name="rTexture">Texture to render</param>
        /// <param name="rPosition">Center position in world space</param>
        /// <param name="rWidth">Size of the texture in pixels</param>
        /// <param name="rHeight">Size of the texture in pixels</param>
        public static void DrawTexture(Texture rTexture, Vector3 rPosition, float rWidth, float rHeight)
        {
            Vector2 lScreenPoint = Camera.main.WorldToScreenPoint(rPosition);
            lScreenPoint.x = Mathf.Floor(lScreenPoint.x);
            lScreenPoint.y = Mathf.Floor(lScreenPoint.y);

            Rect lScreenRect = new Rect(lScreenPoint.x - (rWidth * 0.5f), Screen.height - lScreenPoint.y - (rHeight * 0.5f), rWidth, rHeight);

            //UnityEngine.Graphics.DrawTexture(lScreenRect, rTexture);
            UnityEngine.GUI.DrawTexture(lScreenRect, rTexture);
        }

        /// <summary>
        /// Renders a camera facing texture to the screen given the screen position and size
        /// </summary>
        /// <param name="rTexture">Texture to render</param>
        /// <param name="rPosition">Center position in screen space</param>
        /// <param name="rWidth">Size of the texture in pixels</param>
        /// <param name="rHeight">Size of the texture in pixels</param>
        public static void DrawTexture(Texture rTexture, Vector2 rPosition, float rWidth, float rHeight)
        {
            rPosition.x = rPosition.x * Screen.width;
            rPosition.y = rPosition.y * Screen.height;
            Rect lScreenRect = new Rect(rPosition.x - (rWidth * 0.5f), Screen.height - rPosition.y - (rHeight * 0.5f), rWidth, rHeight);

            //UnityEngine.Graphics.DrawTexture(lScreenRect, rTexture);
            UnityEngine.GUI.DrawTexture(lScreenRect, rTexture);
        }

        /// <summary>
        /// Draws text on the scene
        /// </summary>
        /// <param name="rText"></param>
        /// <param name="rPosition"></param>
        /// <param name="rColor"></param>
        public static void DrawText(string rText, Vector3 rPosition, Color rColor, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            if (rScope == RenderScope.ALL || rScope == RenderScope.GAME)
            {
                DrawText(rText, rPosition, rColor, mFont, rDuration);
            }
            else if (rScope == RenderScope.ALL || rScope == RenderScope.EDITOR)
            {
                TextString lText = TextString.Allocate();
                lText.Scope = (rScope == RenderScope.EDITOR ? (GraphicsManager.IsInUpdate ? 1 : 0) : (GraphicsManager.IsInUpdate ? 3 : 2));
                lText.Value = rText;
                lText.Color = rColor;
                lText.Position = rPosition;
                lText.ExpirationTime = (rDuration <= 0f ? 0f : InternalTime + rDuration);

                mSceneText.Add(lText);
            }
        }

        /// <summary>
        /// Draws text on the screen
        /// </summary>
        /// <param name="sText"></param>
        /// <param name="rPosition"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="rFont"></param>
        /// <param name="fontSize"></param>
        public static void DrawText(string rText, Vector3 rPosition, Color rColor, Font rFont, float rDuration = 0f, RenderScope rScope = RenderScope.EDITOR)
        {
            // Extract the font texture if we haven't already
            if (!mFonts.ContainsKey(rFont))
            {
                if (!AddFont(rFont)) { return; }
            }

            TextFont lTextFont = mFonts[rFont];

            // Gather information about the text
            int lWidth = Mathf.Abs(lTextFont.MinX);
            CharacterInfo lInfo;

            char[] lString = rText.ToCharArray();
            for (int i = 0; i < lString.Length; i++)
            {
                rFont.GetCharacterInfo(lString[i], out lInfo);
                lWidth += Mathf.Max(lInfo.advance, lInfo.glyphWidth);
            }

            int lHeight = lTextFont.MaxY - lTextFont.MinY;

            // Create the destination texture and clear it
            Texture2D lTexture = new Texture2D(lWidth, lHeight, TextureFormat.ARGB32, false, true);
            Color32[] lColors = new Color32[lWidth * lHeight];
            for (int i = 0; i < lColors.Length; i++) { lColors[i] = new Color32(0, 0, 0, 0); }
            lTexture.SetPixels32(lColors);

            // Process each character in the text
            int lStartX = Mathf.Abs(lTextFont.MinX);
            int lStartY = Mathf.Abs(lTextFont.MinY);

            for (int i = 0; i < lString.Length; i++)
            {
                TextCharacter lCharacter = GetCharacterPixels(rFont, lString[i]);

                // Change color and set pixels if we're not dealing with a white space
                if (lCharacter.Pixels != null)
                {
                    for (int j = 0; j < lCharacter.Pixels.Length; j++)
                    {
                        rColor.a = lCharacter.Pixels[j].a;
                        lCharacter.Pixels[j] = rColor;
                    }

                    lTexture.SetPixels(lStartX + lCharacter.MinX, lStartY + lCharacter.MinY, lCharacter.Width, lCharacter.Height, lCharacter.Pixels);
                }

                // Move our cursor forward
                lStartX += (int)lCharacter.Advance;
            }

            // Apply all the changes
            lTexture.Apply();

            // The allocation is really just so we can destroy the texture at the end of the frame
            Text lText = Text.Allocate();
            lText.Position = rPosition;
            lText.Texture = lTexture;
            lText.ExpirationTime = (rDuration <= 0f ? 0f : InternalTime + rDuration);
            mText.Add(lText);
        }

        /// <summary>
        /// Draws a line immediately
        /// </summary>
        public static void ImmediateDrawLine(Vector3 rStart, Vector3 rEnd, Color rColor, Transform rTransform = null)
        {
            if (mSimpleMaterial == null) { CreateMaterials(); }
            mSimpleMaterial.SetPass(0);

            GL.PushMatrix();

            if (rTransform == null)
            {
                GL.MultMatrix(Matrix4x4.identity);
            }
            else
            {
                GL.MultMatrix(rTransform.localToWorldMatrix);
            }

            GL.Begin(GL.LINES);
            GL.Color(rColor);
            GL.Vertex3(rStart.x, rStart.y, rStart.z);
            GL.Vertex3(rEnd.x, rEnd.y, rEnd.z);
            GL.End();

            GL.PopMatrix();
        }

        /// <summary>
        /// Draws connected lines immediately
        /// </summary>
        public static void ImmediateDrawLines(List<Vector3> rLines, Color rColor, Transform rTransform = null)
        {
            if (mSimpleMaterial == null) { CreateMaterials(); }
            mSimpleMaterial.SetPass(0);

            GL.PushMatrix();

            if (rTransform == null)
            {
                GL.MultMatrix(Matrix4x4.identity);
            }
            else
            {
                GL.MultMatrix(rTransform.localToWorldMatrix);
            }

            GL.Begin(GL.LINES);
            GL.Color(rColor);

            for (int i = 1; i < rLines.Count; i++)
            {
                GL.Vertex3(rLines[i - 1].x, rLines[i - 1].y, rLines[i - 1].z);
                GL.Vertex3(rLines[i].x, rLines[i].y, rLines[i].z);
            }

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a triangle immediately using Unity's GL
        /// </summary>
        public static void ImmediateDrawTriangle(Vector3 rPoint1, Vector3 rPoint2, Vector3 rPoint3, Color rColor, Transform rTransform = null)
        {
            if (mSimpleMaterial == null) { CreateMaterials(); }
            mSimpleMaterial.SetPass(0);

            GL.PushMatrix();

            if (rTransform == null)
            {
                GL.MultMatrix(Matrix4x4.identity);
            }
            else
            {
                GL.MultMatrix(rTransform.localToWorldMatrix);
            }

            GL.Begin(GL.TRIANGLES);
            GL.Color(rColor);

            GL.Vertex3(rPoint1.x, rPoint1.y, rPoint1.z);
            GL.Vertex3(rPoint2.x, rPoint2.y, rPoint2.z);
            GL.Vertex3(rPoint3.x, rPoint3.y, rPoint3.z);

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws triangles immediately using Unity's GL
        /// </summary>
        public static void ImmediateDrawTriangles(List<Vector3> rPoints, Color rColor, Transform rTransform = null)
        {
            if (rPoints.Count == 0) { return; }
            if (rPoints.Count % 3 != 0) { return; }

            if (mSimpleMaterial == null) { CreateMaterials(); }
            mSimpleMaterial.SetPass(0);

            GL.PushMatrix();

            if (rTransform == null)
            {
                GL.MultMatrix(Matrix4x4.identity);
            }
            else
            {
                GL.MultMatrix(rTransform.localToWorldMatrix);
            }

            GL.Begin(GL.TRIANGLES);
            GL.Color(rColor);

            for (int i = 2; i < rPoints.Count; i += 3)
            {
                GL.Vertex3(rPoints[i - 2].x, rPoints[i - 2].y, rPoints[i - 2].z);
                GL.Vertex3(rPoints[i - 1].x, rPoints[i - 1].y, rPoints[i - 1].z);
                GL.Vertex3(rPoints[i].x, rPoints[i].y, rPoints[i].z);
            }

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Adds a font to the list of available fonts and extracts out the texture
        /// </summary>
        /// <param name="rFont"></param>
        public static bool AddFont(Font rFont)
        {
            if (rFont == null) { return false; }
            if (mFonts.ContainsKey(rFont)) { return true; }

            Texture2D lFontTexture = (Texture2D)rFont.material.mainTexture;
            byte[] lFontRawData = lFontTexture.GetRawTextureData();

            Texture2D lSourceTexture = new Texture2D(lFontTexture.width, lFontTexture.height, lFontTexture.format, false);
            lSourceTexture.LoadRawTextureData(lFontRawData);
            lSourceTexture.Apply();

            // Create the cache
            TextFont lTextFont = TextFont.Allocate();
            lTextFont.Font = rFont;
            lTextFont.Texture = lSourceTexture;

            // Find the MinY
            CharacterInfo lInfo;
            char[] lString = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.,?:;~!@#$%^&*()_+-=".ToCharArray();

            for (int i = 0; i < lString.Length; i++)
            {
                rFont.GetCharacterInfo(lString[i], out lInfo);
                if (lInfo.minX < lTextFont.MinX) { lTextFont.MinX = lInfo.minX; }
                if (lInfo.maxX > lTextFont.MaxX) { lTextFont.MaxX = lInfo.maxX; }
                if (lInfo.minY < lTextFont.MinY) { lTextFont.MinY = lInfo.minY; }
                if (lInfo.maxY > lTextFont.MaxY) { lTextFont.MaxY = lInfo.maxY; }
            }

            // Store the cache
            mFonts.Add(rFont, lTextFont);

            return true;
        }

        /// <summary>
        /// Draws any lines that need to be drawn
        /// </summary>
        private static void RenderLines()
        {
            if (mSimpleMaterial == null) { CreateMaterials(); }
            mSimpleMaterial.SetPass(0);

            for (int i = 0; i < mLines.Count; i++)
            {
                Line lLine = mLines[i];

                GL.PushMatrix();

                if (lLine.Transform == null)
                {
                    GL.MultMatrix(Matrix4x4.identity);
                }
                else
                {
                    GL.MultMatrix(lLine.Transform.localToWorldMatrix);
                }

                GL.Begin(GL.LINES);
                GL.Color(lLine.Color);
                GL.Vertex3(lLine.Start.x, lLine.Start.y, lLine.Start.z);
                GL.Vertex3(lLine.End.x, lLine.End.y, lLine.End.z);
                GL.End();

                GL.PopMatrix();
            }
        }

        /// <summary>
        /// Draws any triangles that need to be drawn
        /// </summary>
        private static void RenderTriangles()
        {
            if (mSimpleMaterial == null) { CreateMaterials(); }
            mSimpleMaterial.SetPass(0);

            for (int i = 0; i < mTriangles.Count; i++)
            {
                Triangle lTriangle = mTriangles[i];

                GL.PushMatrix();

                if (lTriangle.Transform == null)
                {
                    GL.MultMatrix(Matrix4x4.identity);
                }
                else
                {
                    GL.MultMatrix(lTriangle.Transform.localToWorldMatrix);
                }

                GL.Begin(GL.TRIANGLES);
                GL.Color(lTriangle.Color);
                GL.Vertex3(lTriangle.Point1.x, lTriangle.Point1.y, lTriangle.Point1.z);
                GL.Vertex3(lTriangle.Point2.x, lTriangle.Point2.y, lTriangle.Point2.z);
                GL.Vertex3(lTriangle.Point3.x, lTriangle.Point3.y, lTriangle.Point3.z);
                GL.End();

                GL.PopMatrix();
            }
        }

        /// <summary>
        /// Draws any text that needs to be draw
        /// </summary>
        private static void RenderText()
        {
            for (int i = 0; i < mText.Count; i++)
            {
                Text lText = mText[i];
                if (lText.Texture == null) { continue; }
                if (object.ReferenceEquals(lText.Texture, null)) { continue; }

                int lWidth = lText.Texture.width;
                int lHeight = lText.Texture.height;
                Vector2 lScreenPoint = Camera.main.WorldToScreenPoint(lText.Position);
                Rect lScreenRect = new Rect(lScreenPoint.x - (lWidth * 0.5f), Screen.height - lScreenPoint.y - (lHeight * 0.5f), lWidth, lHeight);

                //UnityEngine.Graphics.DrawTexture(lScreenRect, lText.Texture);
                UnityEngine.GUI.DrawTexture(lScreenRect, lText.Texture);
            }
        }

        /// <summary>
        /// Draws any lines that need to be drawn
        /// </summary>
        private static void RenderSceneLines()
        {
#if UNITY_EDITOR
            if (mSimpleMaterial == null) { CreateMaterials(); }
            mSimpleMaterial.SetPass(0);

            for (int i = 0; i < mSceneLines.Count; i++)
            {
                Line lLine = mSceneLines[i];

                GL.PushMatrix();

                if (lLine.Transform == null)
                {
                    GL.MultMatrix(Matrix4x4.identity);
                }
                else
                {
                    GL.MultMatrix(lLine.Transform.localToWorldMatrix);
                }

                GL.Begin(GL.LINES);
                GL.Color(lLine.Color);
                GL.Vertex3(lLine.Start.x, lLine.Start.y, lLine.Start.z);
                GL.Vertex3(lLine.End.x, lLine.End.y, lLine.End.z);
                GL.End();

                GL.PopMatrix();
            }
#endif
        }

        /// <summary>
        /// Draws any triangles that need to be drawn
        /// </summary>
        private static void RenderSceneTriangles()
        {
#if UNITY_EDITOR
            if (mSimpleMaterial == null) { CreateMaterials(); }
            mSimpleMaterial.SetPass(0);

            for (int i = 0; i < mSceneTriangles.Count; i++)
            {
                Triangle lTriangle = mSceneTriangles[i];

                GL.PushMatrix();

                if (lTriangle.Transform == null)
                {
                    GL.MultMatrix(Matrix4x4.identity);
                }
                else
                {
                    GL.MultMatrix(lTriangle.Transform.localToWorldMatrix);
                }

                GL.Begin(GL.TRIANGLES);
                GL.Color(lTriangle.Color);
                GL.Vertex3(lTriangle.Point1.x, lTriangle.Point1.y, lTriangle.Point1.z);
                GL.Vertex3(lTriangle.Point2.x, lTriangle.Point2.y, lTriangle.Point2.z);
                GL.Vertex3(lTriangle.Point3.x, lTriangle.Point3.y, lTriangle.Point3.z);
                GL.End();

                GL.PopMatrix();
            }
#endif
        }

        /// <summary>
        /// Draws any text that needs to be draw
        /// </summary>
        private static void RenderSceneText()
        {
#if UNITY_EDITOR
            for (int i = 0; i < mSceneText.Count; i++)
            {
                TextString lText = mSceneText[i];

                GUIStyle lStyle = new GUIStyle();
                lStyle.normal.textColor = lText.Color;

                Handles.Label(lText.Position, lText.Value, lStyle);
            }
#endif
        }

        /// <summary>
        /// Creates the material we'll render with
        /// </summary>
        private static void CreateMaterials()
        {
            if (mSimpleMaterial != null) { return; }

            // Unity has a built-in shader that is useful for drawing simple colored things.
            //Shader lShader = Shader.Find("Hidden/Internal-Colored");
            //Shader lShader = Shader.Find("Hidden/GraphicsManagerUI");
            Shader lShader = Shader.Find(mShader);
            if (lShader == null) { lShader = Shader.Find("Hidden/GraphicsManagerUI"); }

            mSimpleMaterial = new Material(lShader);
            mSimpleMaterial.hideFlags = HideFlags.HideAndDontSave;

            // Turn on alpha blending
            mSimpleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mSimpleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            mSimpleMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            mSimpleMaterial.SetInt("_ZWrite", 0);
        }

        /// <summary>
        /// If the pixels are cached, grab them. Otherwise, create the cache
        /// </summary>
        /// <param name="rFont"></param>
        /// <param name="rCharacter"></param>
        /// <returns></returns>
        private static TextCharacter GetCharacterPixels(Font rFont, char rCharacter)
        {
            // If we already grabbed the pixesl, this is easy
            if (!mFonts.ContainsKey(rFont)) { return null; }
            if (mFonts[rFont].Characters.ContainsKey(rCharacter))
            {
                return mFonts[rFont].Characters[rCharacter];
            }

            // Grab our font's source texture
            Texture2D lFontTexture = mFonts[rFont].Texture;

            // Process each character in the text
            int x, y, w, h;
            CharacterInfo lInfo;
            Vector2 lGlyphBottomLeft = Vector2.zero;

            Color[] lPixels = null;
            rFont.GetCharacterInfo(rCharacter, out lInfo);

            // Rotate 90-counter-clockwise needed
            if (lInfo.uvBottomLeft.x == lInfo.uvBottomRight.x)
            {
                // No flip needed
                if (lInfo.uvBottomLeft.y > lInfo.uvBottomRight.y)
                {
                    lGlyphBottomLeft = lInfo.uvBottomRight;
                }
                else
                {
                    lGlyphBottomLeft = lInfo.uvBottomLeft;
                }
            }
            // No rotate needed
            else
            {
                // Flip needed
                if (lInfo.uvBottomLeft.y > lInfo.uvTopLeft.y)
                {
                    lGlyphBottomLeft = lInfo.uvTopLeft;
                }
                // No flip needed
                else
                {
                    lGlyphBottomLeft = lInfo.uvBottomLeft;
                }
            }

            x = (int)((float)lFontTexture.width * lGlyphBottomLeft.x) + 0;
            y = (int)((float)lFontTexture.height * lGlyphBottomLeft.y) + 0;
            w = lInfo.glyphWidth;
            h = lInfo.glyphHeight;

            // We need to rotate the pixels
            if (lInfo.uvBottomLeft.x == lInfo.uvBottomRight.x)
            {
                if (lInfo.uvBottomLeft.y > lInfo.uvBottomRight.y)
                {
                    lPixels = lFontTexture.GetPixels(x, y, h, w);
                    lPixels = RotatePixelsLeft(lPixels, h, w);
                }
            }

            // We need to flip the array
            if (lInfo.uvBottomLeft.y > lInfo.uvTopLeft.y)
            {
                lPixels = lFontTexture.GetPixels(x, y, w, h);
                lPixels = FlipPixelsVertically(lPixels, w, h);
            }

            // We need to mirror the array
            if (lInfo.uvTopLeft.x > lInfo.uvTopRight.x)
            {
                lPixels = lFontTexture.GetPixels(x, y, w, h);
                lPixels = FlipPixelsHorizontally(lPixels, w, h);
            }

            // Add the character pixels since they didn't exist in the beginning
            TextCharacter lTextCharacter = TextCharacter.Allocate();
            lTextCharacter.Character = rCharacter;
            lTextCharacter.Pixels = lPixels;
            lTextCharacter.MinX = lInfo.minX;
            lTextCharacter.MinY = lInfo.minY;
            lTextCharacter.Width = w;
            lTextCharacter.Height = h;
            lTextCharacter.Advance = lInfo.advance;

            mFonts[rFont].Characters.Add(rCharacter, lTextCharacter);

            // Return the pixels
            return lTextCharacter;
        }

        /// <summary>
        /// Rotates the array 90-degrees counter-clockwise
        /// </summary>
        /// <param name="rArray"></param>
        /// <param name="rWidth"></param>
        /// <param name="rHeight"></param>
        /// <returns></returns>
        private static Color[] RotatePixelsLeft(Color[] rArray, int rWidth, int rHeight)
        {
            Color[] lNewArray = new Color[rArray.Length];

            for (int i = 0; i < rArray.Length; i++)
            {
                int lRow = i / rWidth;
                int lCol = i % rWidth;

                int lNewRow = lCol;
                int lNewCol = rHeight - lRow - 1;

                int lIndex = (lNewRow * rHeight) + lNewCol;
                lNewArray[lIndex] = rArray[i];
            }

            return lNewArray;
        }

        /// <summary>
        /// Flips the array horizontally, but not vertically
        /// </summary>
        /// <param name="rArray"></param>
        /// <param name="rWidth"></param>
        /// <param name="rHeight"></param>
        /// <returns></returns>
        private static Color[] FlipPixelsHorizontally(Color[] rArray, int rWidth, int rHeight)
        {
            Color lColor;
            Color[] lNewArray = new Color[rArray.Length];

            for (int y = 0; y < rHeight; y++)
            {
                for (int x = 0; x < rWidth; x++)
                {
                    lColor = rArray[y * rWidth + x];
                    lNewArray[(rWidth - 1 - x) * rHeight + y] = lColor;
                }
            }

            return lNewArray;
        }

        /// <summary>
        /// Flips the array vertically, but not horizontally
        /// </summary>
        /// <param name="rArray"></param>
        /// <param name="rWidth"></param>
        /// <param name="rHeight"></param>
        /// <returns></returns>
        private static Color[] FlipPixelsVertically(Color[] rArray, int rWidth, int rHeight)
        {
            int lRow;
            int lTargetRow;
            int lTargetRowStart;

            Color[] lNewArray = new Color[rArray.Length];

            for (int i = 0; i < rArray.Length;)
            {
                lRow = i / rWidth;
                lTargetRow = rHeight - lRow;
                lTargetRowStart = (lTargetRow - 1) * rWidth;

                for (int j = lTargetRowStart; j < lTargetRowStart + rWidth; j++, i++)
                {
                    lNewArray[j] = rArray[i];
                }
            }

            return lNewArray;
        }

        /// <summary>
        /// Support class for a 8 sided polygon
        /// </summary>
        private class Octahedron
        {
            public Vector3[] Vertices;
            public int[] Triangles;

            public Octahedron()
            {
                Vertices = CreateVertices();
                Triangles = CreateTriangles();

                // We want the edges of the polygon to look crisp. So,
                // we're going to create individual vertices for each index
                Vector3[] lNewVertices = new Vector3[Triangles.Length];
                for (int i = 0; i < Triangles.Length; i++)
                {
                    lNewVertices[i] = Vertices[Triangles[i]];
                    Triangles[i] = i;
                }

                Vertices = lNewVertices;
            }

            private Vector3[] CreateVertices()
            {
                int lStride = 3;

                float[] lVerticesFloat = new float[] { 0.000000f, 0.500000f, 0.000000f, 0.500000f, 0.000000f, 0.000000f, 0.000000f, 0.000000f, -0.500000f, -0.500000f, 0.000000f, 0.000000f, 0.000000f, -0.000000f, 0.500000f, 0.000000f, -0.500000f, -0.000000f };

                Vector3[] lVertices = new Vector3[lVerticesFloat.Length / lStride];
                for (int i = 0; i < lVerticesFloat.Length; i += lStride)
                {
                    lVertices[i / lStride] = new Vector3(lVerticesFloat[i], lVerticesFloat[i + 1], lVerticesFloat[i + 2]);
                }

                return lVertices;
            }

            private int[] CreateTriangles()
            {
                int[] lIndexes = { 1, 2, 0, 2, 3, 0, 3, 4, 0, 0, 4, 1, 5, 2, 1, 5, 3, 2, 5, 4, 3, 5, 1, 4 };
                return lIndexes;
            }
        }

        /// <summary>
        /// Class that is used to help us create spheres
        /// </summary>
        private class Icosahedron
        {
            public Vector3[] Vertices;
            public int[] Triangles;

            public List<Vector3> TriangleList;

            public Icosahedron()
            {
                Vertices = CreateVertices();
                Triangles = CreateTriangles();
            }

            public void Tessellate(int rSubdivisions)
            {

            }

            private Vector3[] CreateVertices()
            {
                Vector3[] vertices = new Vector3[12];

                float lHalfSize = 0.5f;
                float a = (lHalfSize + Mathf.Sqrt(5)) / 2.0f;

                vertices[0] = new Vector3(a, 0.0f, lHalfSize);
                vertices[9] = new Vector3(-a, 0.0f, lHalfSize);
                vertices[11] = new Vector3(-a, 0.0f, -lHalfSize);
                vertices[1] = new Vector3(a, 0.0f, -lHalfSize);
                vertices[2] = new Vector3(lHalfSize, a, 0.0f);
                vertices[5] = new Vector3(lHalfSize, -a, 0.0f);
                vertices[10] = new Vector3(-lHalfSize, -a, 0.0f);
                vertices[8] = new Vector3(-lHalfSize, a, 0.0f);
                vertices[3] = new Vector3(0.0f, lHalfSize, a);
                vertices[7] = new Vector3(0.0f, lHalfSize, -a);
                vertices[6] = new Vector3(0.0f, -lHalfSize, -a);
                vertices[4] = new Vector3(0.0f, -lHalfSize, a);

                for (int i = 0; i < 12; i++)
                {
                    vertices[i].Normalize();
                }

                return vertices;
            }

            private int[] CreateTriangles()
            {
                int[] lTriangles = {
                1,2,0,
                2,3,0,
                3,4,0,
                4,5,0,
                5,1,0,
                6,7,1,
                2,1,7,
                7,8,2,
                2,8,3,
                8,9,3,
                3,9,4,
                9,10,4,
                10,5,4,
                10,6,5,
                6,1,5,
                6,11,7,
                7,11,8,
                8,11,9,
                9,11,10,
                10,11,6
            };

                TriangleList = new List<Vector3>();
                for (int i = 0; i < lTriangles.Length; i++)
                {
                    TriangleList.Add(Vertices[lTriangles[i]]);
                }

                return lTriangles;
            }
        }

        // Author: Kevin Tritz (tritz at yahoo *spamfilter* com)
        // http://codescrib.blogspot.com/
        // copyright (c) 2014  
        // license: BSD style  
        // derived from python version: Icosphere.py  
        //  
        //         Author: William G.K. Martin (wgm2111@cu where cu=columbia.edu)  
        //         copyright (c) 2010  
        //         license: BSD style  
        //        https://code.google.com/p/mesh2d-mpl/source/browse/icosphere.py  
        private class IcoSphere
        {
            public Vector3[] Vertices;           // Vector3[M] array of verticies, M = 10*(num+1)^2 + 2  
            public int[] TriangleIndices;        // int[3*N] flat triangle index list for mesh, N = 20*(num+1)^2              
            public int[,] Triangles;                    // int[N,3] triangle verticies index list, N = 20*(num+1)^2  

            public List<Vector3> TriangleList = new List<Vector3>();

            public void CreateSphere(int rSubdivisions)
            {
                IcoSphere.Icosahedron lIcosahedron = new IcoSphere.Icosahedron();
                get_triangulation(rSubdivisions, lIcosahedron);

                TriangleList.Clear();
                for (int i = 0; i < TriangleIndices.Length; i++)
                {
                    TriangleList.Add(Vertices[TriangleIndices[i]]);
                }
            }

            private void get_triangulation(int num, Icosahedron ico)
            {
                Dictionary<Vector3, int> vertDict = new Dictionary<Vector3, int>();    // dict lookup to speed up vertex indexing  
                float[,] subdivision = getSubMatrix(num + 2);                            // vertex subdivision matrix calculation  
                Vector3 p1, p2, p3;
                int index = 0;
                int vertIndex;
                int len = subdivision.GetLength(0);
                int triNum = (num + 1) * (num + 1) * 20;            // number of triangle faces  
                Vertices = new Vector3[triNum / 2 + 2];        // allocate verticies, triangles, etc...  
                TriangleIndices = new int[triNum * 3];
                Triangles = new int[triNum, 3];
                Vector3[] tempVerts = new Vector3[len];        // temporary structures for subdividing each Icosahedron face  
                int[] tempIndices = new int[len];
                int[,] triIndices = triangulate(num);        // precalculate generic subdivided triangle indices  
                int triLength = triIndices.GetLength(0);
                for (int i = 0; i < 20; i++)                    // calculate subdivided vertices and triangles for each face  
                {
                    p1 = ico.Vertices[ico.Triangles[i * 3]];    // get 3 original vertex locations for each face  
                    p2 = ico.Vertices[ico.Triangles[i * 3 + 1]];
                    p3 = ico.Vertices[ico.Triangles[i * 3 + 2]];
                    for (int j = 0; j < len; j++)                // calculate new subdivided vertex locations  
                    {
                        tempVerts[j].x = subdivision[j, 0] * p1.x + subdivision[j, 1] * p2.x + subdivision[j, 2] * p3.x;
                        tempVerts[j].y = subdivision[j, 0] * p1.y + subdivision[j, 1] * p2.y + subdivision[j, 2] * p3.y;
                        tempVerts[j].z = subdivision[j, 0] * p1.z + subdivision[j, 1] * p2.z + subdivision[j, 2] * p3.z;
                        tempVerts[j].Normalize();
                        if (!vertDict.TryGetValue(tempVerts[j], out vertIndex))    // quick lookup to avoid vertex duplication  
                        {
                            vertDict[tempVerts[j]] = index;    // if vertex not in dict, add it to dictionary and final array  
                            vertIndex = index;
                            Vertices[index] = tempVerts[j];
                            index += 1;
                        }
                        tempIndices[j] = vertIndex;            // assemble vertex indices for triangle assignment  
                    }
                    for (int j = 0; j < triLength; j++)        // map precalculated subdivided triangle indices to vertex indices  
                    {
                        Triangles[triLength * i + j, 0] = tempIndices[triIndices[j, 0]];
                        Triangles[triLength * i + j, 1] = tempIndices[triIndices[j, 1]];
                        Triangles[triLength * i + j, 2] = tempIndices[triIndices[j, 2]];
                        TriangleIndices[3 * triLength * i + 3 * j] = tempIndices[triIndices[j, 0]];
                        TriangleIndices[3 * triLength * i + 3 * j + 1] = tempIndices[triIndices[j, 1]];
                        TriangleIndices[3 * triLength * i + 3 * j + 2] = tempIndices[triIndices[j, 2]];
                    }
                }
            }
            private int[,] triangulate(int num)    // fuction to precalculate generic triangle indices for subdivided vertices  
            {
                int n = num + 2;
                int[,] triangles = new int[(n - 1) * (n - 1), 3];
                int shift = 0;
                int ind = 0;
                for (int row = 0; row < n - 1; row++)
                {
                    triangles[ind, 0] = shift + 1;
                    triangles[ind, 1] = shift + n - row;
                    triangles[ind, 2] = shift;
                    ind += 1;
                    for (int col = 1; col < n - 1 - row; col++)
                    {
                        triangles[ind, 0] = shift + col;
                        triangles[ind, 1] = shift + n - row + col;
                        triangles[ind, 2] = shift + n - row + col - 1;
                        ind += 1;
                        triangles[ind, 0] = shift + col + 1;
                        triangles[ind, 1] = shift + n - row + col;
                        triangles[ind, 2] = shift + col;
                        ind += 1;
                    }
                    shift += n - row;
                }
                return triangles;
            }
            private Vector2[] getUV(Vector3[] vertices)    // standard Longitude/Latitude mapping to (0,1)/(0,1)  
            {
                int num = vertices.Length;
                float pi = (float)System.Math.PI;
                Vector2[] UV = new Vector2[num];
                for (int i = 0; i < num; i++)
                {
                    UV[i] = cartToLL(vertices[i]);
                    UV[i].x = (UV[i].x + pi) / (2.0f * pi);
                    UV[i].y = (UV[i].y + pi / 2.0f) / pi;
                }
                return UV;
            }
            private Vector2 cartToLL(Vector3 point)    // transform 3D cartesion coordinates to longitude, latitude  
            {
                Vector2 coord = new Vector2();
                float norm = point.magnitude;
                if (point.x != 0.0f || point.y != 0.0f)
                    coord.x = -(float)System.Math.Atan2(point.y, point.x);
                else
                    coord.x = 0.0f;
                if (norm > 0.0f)
                    coord.y = (float)System.Math.Asin(point.z / norm);
                else
                    coord.y = 0.0f;
                return coord;
            }
            private float[,] getSubMatrix(int num)    // vertex subdivision matrix, num=3 subdivides 1 triangle into 4  
            {
                int numrows = num * (num + 1) / 2;
                float[,] subdivision = new float[numrows, 3];
                float[] values = new float[num];
                int[] offsets = new int[num];
                int[] starts = new int[num];
                int[] stops = new int[num];
                int index;
                for (int i = 0; i < num; i++)
                {
                    values[i] = (float)i / (float)(num - 1);
                    offsets[i] = (num - i);
                    if (i > 0)
                        starts[i] = starts[i - 1] + offsets[i - 1];
                    else
                        starts[i] = 0;
                    stops[i] = starts[i] + offsets[i];
                }
                for (int i = 0; i < num; i++)
                {
                    for (int j = 0; j < offsets[i]; j++)
                    {
                        index = starts[i] + j;
                        subdivision[index, 0] = values[offsets[i] - 1 - j];
                        subdivision[index, 1] = values[j];
                        subdivision[index, 2] = values[i];
                    }
                }
                return subdivision;
            }

            private class Icosahedron
            {
                public Vector3[] Vertices;
                public int[] Triangles;

                public Icosahedron()
                {
                    Vertices = CreateVertices();
                    Triangles = CreateTriangles();
                }

                private Vector3[] CreateVertices()
                {
                    Vector3[] vertices = new Vector3[12];

                    float lHalfSize = 0.5f;
                    float a = (lHalfSize + Mathf.Sqrt(5)) / 2.0f;

                    vertices[0] = new Vector3(a, 0.0f, lHalfSize);
                    vertices[9] = new Vector3(-a, 0.0f, lHalfSize);
                    vertices[11] = new Vector3(-a, 0.0f, -lHalfSize);
                    vertices[1] = new Vector3(a, 0.0f, -lHalfSize);
                    vertices[2] = new Vector3(lHalfSize, a, 0.0f);
                    vertices[5] = new Vector3(lHalfSize, -a, 0.0f);
                    vertices[10] = new Vector3(-lHalfSize, -a, 0.0f);
                    vertices[8] = new Vector3(-lHalfSize, a, 0.0f);
                    vertices[3] = new Vector3(0.0f, lHalfSize, a);
                    vertices[7] = new Vector3(0.0f, lHalfSize, -a);
                    vertices[6] = new Vector3(0.0f, -lHalfSize, -a);
                    vertices[4] = new Vector3(0.0f, -lHalfSize, a);

                    for (int i = 0; i < 12; i++)
                    {
                        vertices[i].Normalize();
                    }

                    return vertices;
                }

                private int[] CreateTriangles()
                {
                    int[] lTriangles = {
                1,2,0,
                2,3,0,
                3,4,0,
                4,5,0,
                5,1,0,
                6,7,1,
                2,1,7,
                7,8,2,
                2,8,3,
                8,9,3,
                3,9,4,
                9,10,4,
                10,5,4,
                10,6,5,
                6,1,5,
                6,11,7,
                7,11,8,
                8,11,9,
                9,11,10,
                10,11,6,
            };

                    return lTriangles;
                }
            }
        }
    }
}