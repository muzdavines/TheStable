﻿using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Utilities;
using com.ootii.Utilities.Debug;
using UnityEngine.Serialization;

namespace com.ootii.Data.Serializers
{
    /// <summary>
    /// Helper class that provides support for basic JSON serialization and
    /// deserialization.
    /// </summary>
    public class JSONSerializer
    {
        // ID to identify the root object
        public const string RootObjectID = "[OOTII_ROOT]";

        /// <summary>
        /// Root object that will be the reference point for serializing/deserializing GameObjects and
        /// Transforms. This is to help support prefabs.
        /// </summary>
        public static GameObject RootObject = null;

        /// <summary>
        /// Serialize the object into a basic JSON string. This function isnt meant to be
        /// super robust. It provides simple serialization.
        /// </summary>
        /// <param name="rObject">Object to serialize</param>
        /// <param name="rIncludeProperties">include the properties in the JSON string</param>
        /// <returns>String that represents the serialized version of the object</returns>
        public static string Serialize(object rObject, bool rIncludeProperties)
        {
            if (rObject == null) { return ""; }

            StringBuilder lJSON = new StringBuilder();
            lJSON.Append("{");
            lJSON.Append("__Type");
            lJSON.Append(" : ");
            lJSON.Append("\"");            
            lJSON.Append(rObject.GetType().AssemblyQualifiedName);            
            lJSON.Append("\"");

            if (ReflectionHelper.IsPrimitive(rObject.GetType()))
            {
                string lValueString = SerializeValue(rObject);
                lJSON.Append(", ");
                lJSON.Append("__Value");
                lJSON.Append(" : ");
                lJSON.Append(lValueString);
            }
            else if (rObject is string)
            {
                string lValueString = SerializeValue(rObject);
                lJSON.Append(", ");
                lJSON.Append("__Value");
                lJSON.Append(" : ");
                lJSON.Append(lValueString);
            }
            else
            {
                // We don't always include properties because sometimes this can cause
                // recursion. So, we'll only do this selectively.
                if (rIncludeProperties)
                {
                    // Cycle through all the properties and serialize what we know about
                    PropertyInfo[] lProperties = rObject.GetType().GetProperties();
                    foreach (PropertyInfo lProperty in lProperties)
                    {
                        if (!lProperty.CanRead) { continue; }
                        if (!lProperty.CanWrite) { continue; }

                        if (ReflectionHelper.IsDefined(lProperty, typeof(SerializationIgnoreAttribute))) { continue; }

                        // Store the field
                        string lName = lProperty.Name;

                        // Grab the value
                        object lValue = null;

                        try
                        {
                            lValue = lProperty.GetValue(rObject, null);
                            if (lValue == null) { continue; }
                        }
                        catch
                        {
                            lValue = rObject;
                        }

                        // Serialize the field and add it
                        string lValueString = SerializeValue(lValue);
                        lJSON.Append(", ");
                        lJSON.Append(lName);
                        lJSON.Append(" : ");
                        lJSON.Append(lValueString);
                    }
                }

                // Cycle through all the fields and serialize what we know about
                FieldInfo[] lFields = rObject.GetType().GetFields();
                foreach (FieldInfo lField in lFields)
                {
                    if (lField.IsInitOnly) { continue; }
                    if (lField.IsLiteral) { continue; }

                    //object[] lAttributes = lField.GetCustomAttributes(typeof(System.NonSerializedAttribute), true);
                    //if (lAttributes != null && lAttributes.Length > 0) { continue; }
                    if (ReflectionHelper.IsDefined(lField, typeof(NonSerializedAttribute))) { continue; }

                    // If we're not serializing, we need to move on.
                    // NOTE: None of this really matters right now since Unity isn't supporting BindingFlags 
                    // in the GetFields() call. If you add any flags, you always get back an empty array. So,
                    // any fields to be serialized must be public (for now).
                    if (!lField.IsPublic)
                    {
                        if (!ReflectionHelper.IsDefined(lField, typeof(SerializeField)))
                        {
                            if (!ReflectionHelper.IsDefined(lField, typeof(SerializableAttribute))) { continue; }
                        }
                    }

                    // Store the field
                    string lName = lField.Name;

                    // Grab the value
                    object lValue = lField.GetValue(rObject);
                    if (lValue == null) { continue; }

                    // Serialize the field and add it
                    string lValueString = SerializeValue(lValue);
                    lJSON.Append(", ");
                    lJSON.Append(lName);
                    lJSON.Append(" : ");
                    lJSON.Append(lValueString);
                }
            }

            lJSON.Append("}");

            return lJSON.ToString();
        }

        public static string Serialize(object rObject)
        {
            if (rObject == null) { return ""; }

            StringBuilder lJSON = new StringBuilder();
            lJSON.Append("{");
            lJSON.Append("__Type : ");
            lJSON.Append("\"");
            lJSON.Append(rObject.GetType().AssemblyQualifiedName);
            lJSON.Append("\"");

            if (rObject is string || ReflectionHelper.IsPrimitive(rObject.GetType()))
            {
                var lValueString = SerializeValue(rObject);
                lJSON.Append(", ");
                lJSON.Append("__Value");
                lJSON.Append(" : ");
                lJSON.Append(lValueString);
                lJSON.Append("}");

                return lJSON.ToString();
            }

            // Cycle through all the fields and serialize what we know about
            FieldInfo[] lFields = rObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var lField in lFields)
            {
                // Don't serialize values which can only be set in the constructor body or at compile time
                if (lField.IsInitOnly) { continue; }
                if (lField.IsLiteral) { continue; }

                // Don't serialize a field marked with [NonSerialized]
                if (ReflectionHelper.IsDefined(lField, typeof(NonSerializedAttribute))) { continue; }


                // We want to serialize private/protected fields marked with [SerializeField]
                if (!lField.IsPublic)
                {
                    if (!ReflectionHelper.IsDefined(lField, typeof(SerializeField)))
                    {
                        continue;
                    }
                }

                // Serialize the field and its value
                string lValueString = SerializeValue(lField.GetValue(rObject));
                lJSON.Append(", ");
                lJSON.Append(lField.Name);
                lJSON.Append(" : ");
                lJSON.Append(lValueString);
            }

            lJSON.Append("}");

            return lJSON.ToString();
        }


        /// <summary>
        /// Serializes an object as an individual property. 
        /// </summary>
        /// <param name="rName">Name representing this property</param>
        /// <param name="rValue">Value of the property</param>
        /// <returns>JSON string representing the property</returns>
        public static string SerializeValue(string rName, object rValue)
        {
            if (rValue == null) { return ""; }

            StringBuilder lJSON = new StringBuilder();
            lJSON.Append("{");
            lJSON.Append(rName);
            lJSON.Append(" : ");
            lJSON.Append(SerializeValue(rValue));
            lJSON.Append("}");

            return lJSON.ToString();
        }

        /// <summary>
        /// Gets the type of object from the JSON string
        /// </summary>
        /// <param name="rJSON"></param>
        /// <returns></returns>
        public static Type GetType(string rJSON)
        {
            JSONNode lNode = JSONNode.Parse(rJSON);
            if (lNode == null) { return null; }

            return GetType(lNode);
        }

        /// <summary>
        /// Gets the type of object from the JSON string
        /// </summary>
        /// <param name="rNode"></param>
        /// <returns></returns>
        public static Type GetType(JSONNode rNode)
        {
            // First look for the type data using the newer format
            string lTypeString = rNode["__Type"].Value;
            if (string.IsNullOrEmpty(lTypeString))
            {
                // Then look for it using the older format
                lTypeString = rNode["Type"].Value;
                if (string.IsNullOrEmpty(lTypeString))
                {
                    return null;
                }
            }

            return AssemblyHelper.ResolveType(lTypeString);
        }

        /// <summary>
        /// Resolve the type of the serialized object from the JSON string
        /// </summary>
        /// <param name="rJSON"></param>
        /// <param name="rTypeKey"></param>
        /// <param name="rUpdateType"></param>
        /// <returns></returns>
        public static Type GetType(string rJSON, string rTypeKey, out bool rUpdateType)
        {
            rUpdateType = false;

            JSONNode lNode = JSONNode.Parse(rJSON);
            if (lNode == null) { return null; }

            string lTypeString = lNode[rTypeKey].Value;
            if (string.IsNullOrEmpty(lTypeString)) { return null; }

            return AssemblyHelper.ResolveType(lTypeString, out rUpdateType);            
        }

        /// <summary>
        /// Deserialize to an object of the specified type
        /// </summary>
        /// <param name="rJSON"></param>
        /// <returns></returns>
        public static T DeserializeValue<T>(string rJSON)
        {
            Type lType = typeof(T);

            if (string.IsNullOrEmpty(rJSON)) { return default(T); }

            JSONNode lNode = JSONNode.Parse(rJSON);
            if (lNode == null || lNode.Count == 0) { return default(T); }

            object lValue = DeserializeValue(lType, lNode[0]);
            if (lValue == null || lValue.GetType() != lType) { return default(T); }

            return (T)lValue;
        }

        /// <summary>
        /// Deserializes a basic JSON string into object form. It isnt mean to handle complex
        /// data types. This is just a simple helper.
        /// </summary>
        /// <typeparam name="T">Type (or base type) of object were deserializing to</typeparam>
        /// <param name="rJSON">Content that is being deserialized</param>
        /// <returns>Object that was deserialized</returns>
        public static T Deserialize<T>(string rJSON)
        {
            return (T)Deserialize(rJSON);
        }

        /// <summary>
        /// Deserializes a basic JSON string into object form. It isn't mean to handle complex
        /// data types. This is just a simple helper.
        /// </summary>
        /// <param name="rJSON">Content that is being deserialized</param>
        /// <returns>Object that was deserialized</returns>
        public static object Deserialize(string rJSON)
        {            
            JSONNode lNode = JSONNode.Parse(rJSON);
            if (lNode == null) { return null; }

            Type lType = GetType(lNode);
            if (lType == null) { return null; }

            if (ReflectionHelper.IsPrimitive(lType))
            {
                JSONNode lValueNode = lNode["__Value"];
                return DeserializeValue(lType, lValueNode);
            }

            if (lType == typeof(string))
            {
                JSONNode lValueNode = lNode["__Value"];
                return DeserializeValue(lType, lValueNode);
            }

            object lObject = null;

            try
            {
                lObject = Activator.CreateInstance(lType);
            }
            catch (Exception lException)
            {
                Debug.Log("JSONSerializer.Deserialize() {" + lType.AssemblyQualifiedName + "} {" + lException.Message + "} {" + lException.StackTrace + "}");
            }

            if (lObject == null) { return null; }

            // Cycle through of all the public properties.
            PropertyInfo[] lProperties = lObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo lProperty in lProperties)
            {
                if (!lProperty.CanWrite) { continue; }

                if (ReflectionHelper.IsDefined(lProperty, typeof(SerializationIgnoreAttribute))) { continue; }

                JSONNode lValueNode = lNode[lProperty.Name];
                if (lValueNode == null)
                {
                    //// If the property name isn't found, check for any [FormerlySerializedAs("")] attributes and 
                    //// look for a match.
                    //var lFormerNames = lProperty.GetCustomAttributes<FormerlySerializedAsAttribute>();
                    //foreach (var lFormer in lFormerNames)
                    //{
                    //    lValueNode = lNode[lFormer.oldName];
                    //    if (lValueNode != null)
                    //    {
                    //        break;
                    //    }
                    //}
                }

                if (lValueNode != null)
                {
                    object lValue = DeserializeValue(lProperty.PropertyType, lValueNode);
                    if (lValue != null) { lProperty.SetValue(lObject, lValue, null); }
                }
            }

            // Cycle through all of the instance fields
            FieldInfo[] lFields = lObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo lField in lFields)
            {
                if (lField.IsInitOnly) { continue; }
                if (lField.IsLiteral) { continue; }

                if (ReflectionHelper.IsDefined(lField, typeof(NonSerializedAttribute))) { continue; }

                JSONNode lValueNode = lNode[lField.Name];
                if (lValueNode == null)
                {
                    //// If the field name isn't found, check for any [FormerlySerializedAs("")] attributes and 
                    //// look for a match.
                    //var lFormerNames = lField.GetCustomAttributes<FormerlySerializedAsAttribute>();
                    //foreach (var lFormer in lFormerNames)
                    //{
                    //    lValueNode = lNode[lFormer.oldName];
                    //    if (lValueNode != null)
                    //    {
                    //        break;
                    //    }
                    //}
                }
                if (lValueNode != null)
                {
                    object lValue = DeserializeValue(lField.FieldType, lValueNode);
                    if (lValue != null) { lField.SetValue(lObject, lValue); }
                }
            }

            return lObject;
        }

        /// <summary>
        /// Allows us to deserialize into an existing object
        /// </summary>
        /// <param name="rJSON"></param>
        /// <param name="rObject"></param>
        public static void DeserializeInto(string rJSON, ref object rObject)
        {
            if (string.IsNullOrEmpty(rJSON)) { return; }

            JSONNode lNode = JSONNode.Parse(rJSON);
            if (lNode == null || lNode.Count == 0) { return; }

            // If the target is null, instantiate an object
            if (rObject == null)
            {
                Type lType = GetType(lNode);
                if (lType == null) { return; }
                try
                {
                    rObject = Activator.CreateInstance(lType);
                }
                catch (Exception lException)
                {
                    Debug.Log("JSONSerializer.DeserializeInto() {" + lType.AssemblyQualifiedName + "} {" + lException.Message + "} {" + lException.StackTrace + "}");
                }

                if (rObject == null) { return; }
            }

            // Cycle through all the properties. Unfortunately Binding flags dont seem to 
            // be working. So, we need to check them all
            FieldInfo[] lFields = rObject.GetType().GetFields();
            foreach (FieldInfo lField in lFields)
            {
                if (lField.IsInitOnly) { continue; }
                if (lField.IsLiteral) { continue; }
                
                if (ReflectionHelper.IsDefined(lField, typeof(NonSerializedAttribute))) { continue; }

                JSONNode lValueNode = lNode[lField.Name];
                if (lValueNode != null)
                {
                    object lValue = DeserializeValue(lField.FieldType, lValueNode);
                    if (lValue != null) { lField.SetValue(rObject, lValue); }
                }
            }

            PropertyInfo[] lProperties = rObject.GetType().GetProperties();
            foreach (PropertyInfo lProperty in lProperties)
            {
                if (!lProperty.CanWrite) { continue; }

                if (ReflectionHelper.IsDefined(lProperty, typeof(SerializationIgnoreAttribute))) { continue; }

                JSONNode lValueNode = lNode[lProperty.Name];
                if (lValueNode != null)
                {
                    object lValue = DeserializeValue(lProperty.PropertyType, lValueNode);
                    if (lValue != null) { lProperty.SetValue(rObject, lValue, null); }
                }
            }
        }

        /// <summary>
        /// Actually performs the property serialization. This way, we
        /// can abstract it a bit.
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        private static string SerializeValue(object rValue)
        {
            if (rValue == null) { return "\"\""; }

            StringBuilder lJSON = new StringBuilder("");

            Type lType = rValue.GetType();

            if (lType == typeof(string))
            {
                lJSON.Append("\"");
                lJSON.Append((string)rValue);
                lJSON.Append("\"");                
            }
            else if (lType == typeof(int))
            {
                lJSON.Append(((int)rValue).Serialize());                
            }
            else if (lType == typeof(float))
            {
                lJSON.Append(((float)rValue).Serialize());                
            }
            else if (lType == typeof(bool))
            {
                lJSON.Append(((bool)rValue).ToString());
            }
            else if (lType == typeof(Vector2))
            {
                lJSON.Append("\"");                
                lJSON.Append(((Vector2)rValue).Serialize());
                lJSON.Append("\"");                
            }
            else if (lType == typeof(Vector3))
            {
                lJSON.Append("\"");
                lJSON.Append(((Vector3)rValue).Serialize());
                lJSON.Append("\"");                
            }
            else if (lType == typeof(Vector4))
            {
                lJSON.Append("\"");
                lJSON.Append(((Vector4)rValue).Serialize());
                lJSON.Append("\"");                
            }
            else if (lType == typeof(Quaternion))
            {
                lJSON.Append("\"");
                lJSON.Append(((Quaternion)rValue).Serialize());
                lJSON.Append("\"");                
            }
            else if (lType == typeof(HumanBodyBones))
            {
                lJSON.Append(((int)rValue).ToString());                
            }
            else if (lType == typeof(Transform))
            {
                Transform lTransform = rValue as Transform;

                string lRootPath = (RootObject != null ? GetFullPath(RootObject.transform) : "");

                string lPath = GetFullPath(lTransform);
                if (lRootPath.Length > 0f)
                {
                    lPath = ReplaceFirst(lPath, lRootPath, RootObjectID);
                }

                lJSON.Append("\"");
                lJSON.Append(lPath);
                lJSON.Append("\"");
            }
            else if (lType == typeof(GameObject))
            {
                GameObject lGameObject = rValue as GameObject;

                string lRootPath = (RootObject != null ? GetFullPath(RootObject.transform) : "");

                string lPath = GetFullPath(lGameObject.transform);
                if (lRootPath.Length > 0f)
                {
                    lPath = ReplaceFirst(lPath, lRootPath, RootObjectID);
                }

                lJSON.Append("\"");
                lJSON.Append(lPath);
                lJSON.Append("\"");
                
            }
            else if (ReflectionHelper.IsAssignableFrom(typeof(Component), lType))
            {
                Component lComponent = rValue as Component;

                string lRootPath = (RootObject != null ? GetFullPath(RootObject.transform) : "");

                string lPath = GetFullPath(lComponent.transform);
                if (lRootPath.Length > 0f) { lPath = ReplaceFirst(lPath, lRootPath, RootObjectID); }

                lJSON.Append("\"");
                lJSON.Append(lPath);
                lJSON.Append("\"");
            }
            else if (typeof(IList).IsAssignableFrom(lType))
            {
                IList lList = rValue as IList;

                lJSON.Append("[");
                for (int i = 0; i < lList.Count; i++)
                {
                    if (i > 0) { lJSON.Append(","); }
                    lJSON.Append(SerializeValue(lList[i]));
                }
                lJSON.Append("]");
            }
            else if (typeof(IDictionary).IsAssignableFrom(lType))
            {
                IDictionary lDictionary = rValue as IDictionary;

                lJSON.Append("[");
                foreach (object lKey in lDictionary.Keys)
                {
                    string lKeyValue = SerializeValue(lKey);
                    string lItemValue = SerializeValue(lDictionary[lKey]);

                    lJSON.Append("{ ");
                    lJSON.Append(lKeyValue);
                    lJSON.Append(" : ");
                    lJSON.Append(lItemValue);
                    lJSON.Append(" }");
                }
                lJSON.Append("]");
            }
            else if (lType == typeof(AnimationCurve))
            {
                AnimationCurve lCurve = rValue as AnimationCurve;

                lJSON.Append("\"");
                lJSON.Append(lCurve.Serialize());
                lJSON.Append("\"");
            }
            else
            {
                lJSON.Append(Serialize(rValue, false));
            }

            return lJSON.ToString();
        }

        /// <summary>
        /// Performs the actual deserialization. We do it here so we can abstract it some
        /// </summary>
        /// <param name="rType"></param>
        /// <param name="rValue"></param>
        /// <returns></returns>
        private static object DeserializeValue(Type rType, JSONNode rValue)
        {
            if (rValue == null) 
            { 
                return ReflectionHelper.GetDefaultValue(rType); 
            }
            if (rType == typeof(string))
            {
                return rValue.Value;
            }
            if (rType == typeof(int))
            {
                return rValue.AsInt;
            }
            if (rType == typeof(float))
            {
                return rValue.AsFloat;
            }
            if (rType == typeof(bool))
            {
                return rValue.AsBool;
            }
            if (rType == typeof(Vector2))
            {
                Vector2 lValue = Vector2.zero;
                lValue = lValue.FromString(rValue.Value);

                return lValue;
            }
            if (rType == typeof(Vector3))
            {
                Vector3 lValue = Vector3.zero;
                lValue = lValue.FromString(rValue.Value);

                return lValue;
            }
            if (rType == typeof(Vector4))
            {
                Vector4 lValue = Vector4.zero;
                lValue = lValue.FromString(rValue.Value);

                return lValue;
            }
            if (rType == typeof(Quaternion))
            {
                Quaternion lValue = Quaternion.identity;
                lValue = lValue.FromString(rValue.Value);

                return lValue;
            }
            if (rType == typeof(HumanBodyBones))
            {
                return (HumanBodyBones)rValue.AsInt;
            }
            if (rType == typeof(Transform))
            {
                string lName = rValue.Value;

                Transform lValue = null;
                if (lName.Contains(RootObjectID) && RootObject != null)
                {
                    lName = rValue.Value.Replace(RootObjectID, "");
                    if (lName.Length > 0 && lName.Substring(0, 1) == "/") { lName = lName.Substring(1); }

                    lValue = (lName.Length == 0 ? RootObject.transform : RootObject.transform.Find(lName));
                }
                else
                {
                    GameObject lGameObject = GameObject.Find(lName);
                    if (lGameObject != null) { lValue = lGameObject.transform; }
                }

                if (lValue == null)
                {
                    Debug.LogWarning("ootii.JSONSerializer.DeserializeValue - Transform name '" + lName + "' not found, resulting in null");
                    return null;
                }

                return lValue;
            }
            if (rType == typeof(GameObject))
            {
                string lName = rValue.Value;

                Transform lValue = null;
                if (lName.Contains(RootObjectID) && RootObject != null)
                {
                    lName = rValue.Value.Replace(RootObjectID, "");
                    if (lName.Length > 0 && lName.Substring(0, 1) == "/") { lName = lName.Substring(1); }

                    lValue = (lName.Length == 0 ? RootObject.transform : RootObject.transform.Find(lName));
                }
                else
                {
                    GameObject lGameObject = GameObject.Find(lName);
                    if (lGameObject != null) { lValue = lGameObject.transform; }
                }

                if (lValue == null)
                {
                    Debug.LogWarning("ootii.JSONSerializer.DeserializeValue - GameObject name '" + lName + "' not found, resulting in null");
                    return null;
                }

                return lValue.gameObject;
            }
            if (ReflectionHelper.IsAssignableFrom(typeof(Component), rType))
            {
                string lName = rValue.Value;

                Transform lValue = null;
                if (lName.Contains(RootObjectID) && RootObject != null)
                {
                    lName = rValue.Value.Replace(RootObjectID, "");
                    if (lName.Length > 0 && lName.Substring(0, 1) == "/") { lName = lName.Substring(1); }

                    lValue = (lName.Length == 0 ? RootObject.transform : RootObject.transform.Find(lName));
                }
                else
                {
                    GameObject lGameObject = GameObject.Find(lName);
                    if (lGameObject != null) { lValue = lGameObject.transform; }
                }

                if (lValue == null)
                {
                    Debug.LogWarning("ootii.JSONSerializer.DeserializeValue - Component  name '" + lName + "' not found, resulting in null");
                    return null;
                }

                return lValue.gameObject.GetComponent(rType);
            }
            if (typeof(IList).IsAssignableFrom(rType))
            {
                IList lList = null;
                Type lItemType = rType;

                JSONArray lItems = rValue.AsArray;

                if (lItems == null)
                {
                    // Try converting from older-style list/array delimited with '|'
                    // to the standard JSON format for arrays 
                    var lStringItems = rValue.Value.Split('|');
                    if (lStringItems.Length > 0)
                    {
                        var lDefinition = new StringBuilder("{__Temp : ");
                        lDefinition.Append("[");

                        for (int i = 0; i < lStringItems.Length; i++)
                        {
                            if (i > 0)
                            {
                                lDefinition.Append(",");
                            }
                            lDefinition.Append("\"");
                            lDefinition.Append(lStringItems[i]);
                            lDefinition.Append("\"");
                        }

                        lDefinition.Append("]}");

                        var lTempNode = JSONNode.Parse(lDefinition.ToString());
                        
                        lItems = lTempNode["__Temp"].AsArray;
                    }
                }

                if (ReflectionHelper.IsGenericType(rType))
                {
                    lItemType = rType.GetGenericArguments()[0];
                    lList = Activator.CreateInstance(rType) as IList;
                }
                else if (rType.IsArray)
                {
                    lItemType = rType.GetElementType();
                    if (lItemType != null) { lList = Array.CreateInstance(lItemType, lItems.Count); }
                }

                if (lItems != null)
                {
                    for (int i = 0; i < lItems.Count; i++)
                    {
                        JSONNode lItem = lItems[i];
                        object lItemValue = DeserializeValue(lItemType, lItem);

                        if (lList != null)
                        {
                            if (lList.Count > i)
                            {
                                lList[i] = lItemValue;
                            }
                            else
                            {
                                lList.Add(lItemValue);
                            }
                        }
                    }
                }

                return lList;
            }
            if (typeof(IDictionary).IsAssignableFrom(rType))
            {
                //if (!rType.IsGenericType) { return null; }
                if (!ReflectionHelper.IsGenericType(rType)) { return null; }

                Type lKeyType = rType.GetGenericArguments()[0];
                Type lItemType = rType.GetGenericArguments()[1];

                IDictionary lDictionary = Activator.CreateInstance(rType) as IDictionary;
                if (lDictionary == null) { return null; }

                JSONArray lItems = rValue.AsArray;
                for (int i = 0; i < lItems.Count; i++)
                {
                    JSONNode lItem = lItems[i];

                    JSONClass lObject = lItem.AsObject;
                    foreach (string lKeyString in lObject.Dictionary.Keys)
                    {
                        object lKeyValue = DeserializeValue(lKeyType, lKeyString);
                        object lItemValue = DeserializeValue(lItemType, lItem[lKeyString]);

                        if (lDictionary.Contains(lKeyValue))
                        {
                            lDictionary[lKeyValue] = lItemValue;
                        }
                        else
                        {
                            lDictionary.Add(lKeyValue, lItemValue);
                        }
                    }
                }

                return lDictionary;
            }
            if (rType == typeof(AnimationCurve))
            {
                if (rValue.Value.Length > 0)
                {
                    return SerializationHelper.GetAnimationCurve(rValue.Value);                                        
                }
            }

            // As a default, simply try to deserialize the string
            return Deserialize(rValue.ToString());
        }

        /// <summary>
        /// Quick function to tell us if we're dealing with a simple type or a complex type
        /// </summary>
        /// <param name="rType"></param>
        /// <returns></returns>
        private static bool IsSimpleType(Type rType)
        {
            if (rType == typeof(string)) { return true; }
            if (rType == typeof(int)) { return true; }
            if (rType == typeof(float)) { return true; }
            if (rType == typeof(bool)) { return true; }
            if (rType == typeof(Vector2)) { return true; }
            if (rType == typeof(Vector3)) { return true; }
            if (rType == typeof(Vector4)) { return true; }
            if (rType == typeof(Quaternion)) { return true; }
            if (rType == typeof(HumanBodyBones)) { return true; }
            if (rType == typeof(Transform)) { return true; }
            return false;
        }

        /// <summary>
        /// Retrieves the full path to the transform
        /// </summary>
        /// <param name="rTransform">Transform we'll find the path for</param>
        /// <returns>Full path of the transform</returns>
        public static string GetFullPath(Transform rTransform)
        {
            string lPath = "";

            Transform lParent = rTransform;
            while (lParent != null)
            {
                if (lPath.Length > 0) { lPath = "/" + lPath; }
                lPath = lParent.name + lPath;

                lParent = lParent.parent;
            }

            return lPath;
        }

        /// <summary>
        /// Replaces the first instance of the search found
        /// </summary>
        /// <param name="rText"></param>
        /// <param name="rSearch"></param>
        /// <param name="rReplace"></param>
        /// <returns></returns>
        public static string ReplaceFirst(string rText, string rSearch, string rReplace)
        {
            int lIndex = rText.IndexOf(rSearch, StringComparison.Ordinal);
            if (lIndex < 0) { return rText; }

            return rText.Substring(0, lIndex) + rReplace + rText.Substring(lIndex + rSearch.Length);
        }
    }
}
