using UnityEngine;
using com.ootii.Collections;

namespace com.ootii.Graphics
{
    /// <summary>
    /// Provides a way to store lines for our graphics rendering
    /// </summary>
    public class TextString
    {
        /// <summary>
        /// Defines where and when the line will display.
        /// 0 = scene view from editor
        /// 1 = scene view from game
        /// 2 = game view from editor
        /// 3 = game view from game
        /// </summary>
        public int Scope = 0;

        /// <summary>
        /// Transform that provides the reference for the positions
        /// </summary>
        public Transform Transform = null;

        /// <summary>
        /// Text to render
        /// </summary>
        public string Value = "";

        /// <summary>
        /// Point of the triangle
        /// </summary>
        public Vector3 Position = Vector3.zero;

        /// <summary>
        /// Color of the shape
        /// </summary>
        public Color Color = Color.white;

        /// <summary>
        /// GameTime that the triangle will expire
        /// </summary>
        public float ExpirationTime = 0f;

        // ******************************** OBJECT POOL ********************************

        /// <summary>
        /// Allows us to reuse objects without having to reallocate them over and over
        /// </summary>
        private static ObjectPool<TextString> sPool = new ObjectPool<TextString>(20, 5);

        /// <summary>
        /// Returns the number of items allocated
        /// </summary>
        /// <value>The allocated.</value>
        public static int Length
        {
            get { return sPool.Length; }
        }

        /// <summary>
        /// Pulls an object from the pool.
        /// </summary>
        /// <returns></returns>
        public static TextString Allocate()
        {
            // Grab the next available object
            TextString lInstance = sPool.Allocate();

            // Return the allocated instance (should be cleaned before
            // being put back into the pool)
            return lInstance;
        }

        /// <summary>
        /// Returns an element back to the pool.
        /// </summary>
        /// <param name="rEdge"></param>
        public static void Release(TextString rInstance)
        {
            if (object.ReferenceEquals(rInstance, null)) { return; }

            // Set values
            rInstance.Scope = 0;
            rInstance.Transform = null;
            rInstance.Value = "";
            rInstance.Position = Vector3.zero;
            rInstance.Color = Color.white;
            rInstance.ExpirationTime = 0f;

            // Make it available to others.
            sPool.Release(rInstance);
        }
    }
}
