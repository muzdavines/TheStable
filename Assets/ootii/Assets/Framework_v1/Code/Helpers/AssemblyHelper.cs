using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.ootii.Helpers
{
    /// <summary>
    /// Helper class to assist with obtaining types and info from assemblies
    /// </summary>
    public sealed class AssemblyHelper
    {
        private static readonly object mLock = new object();
        private static AssemblyHelper mInstance = null;

        /// <summary>
        /// Thread-safe access to an instance
        /// </summary>
        public static AssemblyHelper Instance
        {
            get
            {
                if (mInstance == null)
                {
                    lock (mLock)
                    {
                        if (mInstance == null)
                        {
                            mInstance = new AssemblyHelper();
                        }
                    }
                }
                return mInstance;
            }
        }

        private AssemblyHelper() { }
        static AssemblyHelper() { }

        /// <summary>
        /// Caches and returns the full name of the ootii assembly
        /// </summary>
        private string _AssemblyInfo;
        public string AssemblyInfo
        {
            get
            {
                if (_AssemblyInfo == null)
                {
                    var lAssembly = Assembly.GetAssembly(typeof(AssemblyHelper));
                    _AssemblyInfo = lAssembly.FullName;
                }
                return _AssemblyInfo;
            }
        }

        /// <summary>
        /// Get the Assembly Qualified Name of a class by searching all loaded assemblies
        /// </summary>
        /// <param name="rClassName"></param>
        /// <param name="rThisAssembly">Bypass the search and just use the ootii assembly</param>
        /// <returns></returns>
        public string GetAssemblyQualifiedName(string rClassName, bool rThisAssembly = true)
        {
            string lTypeString = rClassName + ", " + AssemblyInfo;
            if (rThisAssembly) { return lTypeString; }


            // Search all loaded assemblies
            foreach (var lType in FoundTypes)
            {
                if (lType.FullName == rClassName)
                {
                    return lType.AssemblyQualifiedName;
                }
            }

            return lTypeString;
        }

        /// <summary>
        /// Provides a cached list of Types in all loaded assemblies
        /// </summary>
        private List<Type> mFoundTypes;
        public List<Type> FoundTypes
        {
            get
            {
                if (mFoundTypes == null)
                {
                    // Using InterfaceHelper methods so that we can ensure Windows UWP support
                    var lFoundTypes = new List<Type>();
                    foreach (var lAssembly in InterfaceHelper.GetAssemblies())
                    {
                        lFoundTypes.AddRange(InterfaceHelper.GetTypes(lAssembly));
                    }

                    mFoundTypes = lFoundTypes.OrderBy(x => x.Name).ToList();
                }

                return mFoundTypes;
            }
        }

        /// <summary>
        /// Attempt to get the Type (from a string) even if the AssemblyQualifiedName doesn't match
        /// what was serialized. Does not inform the calling function if the AQN had to be changed in 
        /// the process.
        /// </summary>
        /// <param name="rTypeString"></param>
        /// <returns></returns>
        public static Type ResolveType(string rTypeString)
        {
            bool lTypeNameChanged = false;
            return ResolveType(rTypeString, out lTypeNameChanged);
        }

        /// <summary>
        /// Attempt to get the Type (from a string) even if the AssemblyQualifiedName doesn't match
        /// what was serialized
        /// </summary>
        /// <param name="rTypeString"></param>
        /// <param name="rNameChanged">out parameter; set to true if the AQN had to be changed in order
        /// to get the Type. This tells the calling function that the serialization should be updated.</param>
        /// <returns></returns>
        public static Type ResolveType(string rTypeString, out bool rNameChanged)
        {
            rNameChanged = false;
            Type lType = Type.GetType(rTypeString);
            if (lType != null)
            {
                // The type was found successfully, so we exit early.
                return lType;
            }

            // The most likely reason for GetType() to fail when we're deserializing and object is that the object
            // was serialized using a different AssemblyQualifiedName than where the type currently resides. 

            // This will happen if importing a prefab created with a previous version of the ootii assets,
            // as they will be have been serialized as belonging to the "Assembly-CSharp" assembly instead of the
            // "ootii" assembly. This will also happen if the "ootii" Assembly Definition File is renamed.


            // First, check that the type is using the AssemblyQualifiedName. It should look like:
            // "com.ootii.Actors.AnimationControllers.BasicIdle, ootii, Version=0.0.0.0, Culture = neutral, PublicKeyToken = null"
            if (rTypeString.Contains(",") && rTypeString.Contains("Version="))
            {
                // Remove everything after the type name (after the first comma)
                int lEndIndex = rTypeString.IndexOf(",", StringComparison.Ordinal);
                rTypeString = rTypeString.Substring(0, lEndIndex);
            }

            // Now we just have the type's FullName remaining
            string lFullName = rTypeString;
            rNameChanged = true;

            // First, we'll try the "ootii" assembly (if it has been renamed, the AssemblyInfo property has the current name)
            string lTypeString = lFullName + ", " + Instance.AssemblyInfo;
            lType = Type.GetType(lTypeString);
            if (lType != null) { return lType; }

            // If that failed, then we'll try looking in Assembly-CSharp (the default assembly for code not in an .asmdef)
            lTypeString = lFullName + ", Assembly - CSharp, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null";
            lType = Type.GetType(lTypeString);
            if (lType != null) { return lType; }

            // If this failed as well, then we'll start iterating through all loaded assemblies
            foreach (var lAssembly in InterfaceHelper.GetAssemblies())
            {
                // Skip ootii and Assembly-CSharp, as we've already checked those
                if (lAssembly.FullName == Instance.AssemblyInfo || lAssembly.FullName.Contains("Assembly-CSharp")) continue;

                lTypeString = lFullName + ", " + lAssembly.FullName;
                lType = Type.GetType(lTypeString);
                if (lType != null) { return lType; }
            }

            rNameChanged = false;
            return null;
        }

    }
}