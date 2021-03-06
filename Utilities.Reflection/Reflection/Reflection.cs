﻿/*
Copyright (c) 2011 <a href="http://www.gutgames.com">James Craig</a>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

#region Usings
using System;
using System.Collections;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using Utilities.DataTypes;
using Utilities.DataTypes.ExtensionMethods;
using Utilities.IO;
#endregion

namespace Utilities.Reflection
{
    /// <summary>
    /// Utility class that handles various
    /// functions dealing with reflection.
    /// </summary>
    public static class Reflection
    {
        #region Public Static Functions

        #region CallMethod

        /// <summary>
        /// Calls a method on an object
        /// </summary>
        /// <param name="MethodName">Method name</param>
        /// <param name="Object">Object to call the method on</param>
        /// <param name="InputVariables">(Optional)input variables for the method</param>
        /// <returns>The returned value of the method</returns>
        public static object CallMethod(string MethodName, object Object, params object[] InputVariables)
        {
            if (string.IsNullOrEmpty(MethodName) || Object == null)
                return null;
            Type ObjectType = Object.GetType();
            MethodInfo Method = null;
            if (InputVariables != null)
            {
                Type[] MethodInputTypes = new Type[InputVariables.Length];
                for (int x = 0; x < InputVariables.Length; ++x)
                {
                    MethodInputTypes[x] = InputVariables[x].GetType();
                }
                Method = ObjectType.GetMethod(MethodName, MethodInputTypes);
                if (Method != null)
                {
                    return Method.Invoke(Object, InputVariables);
                }
            }
            Method = ObjectType.GetMethod(MethodName);
            if (Method != null)
            {
                return Method.Invoke(Object, null);
            }
            return null;
        }

        #endregion

        #region DumpAllAssembliesAndProperties

        /// <summary>
        /// Returns all assemblies and their properties
        /// </summary>
        /// <returns>An HTML formatted string that contains the assemblies and their information</returns>
        public static string DumpAllAssembliesAndProperties()
        {
            StringBuilder Builder = new StringBuilder();
            Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly Assembly in Assemblies)
            {
                Builder.Append("<strong>").Append(Assembly.GetName().Name).Append("</strong><br />");
                Builder.Append(DumpProperties(Assembly)).Append("<br /><br />");
            }
            return Builder.ToString();
        }

        #endregion

        #region DumpProperties

        /// <summary>
        /// Dumps the properties names and current values
        /// from an object
        /// </summary>
        /// <param name="Object">Object to dunp</param>
        /// <returns>An HTML formatted table containing the information about the object</returns>
        public static string DumpProperties(object Object)
        {
            if (Object == null)
                throw new ArgumentNullException("Object");
            StringBuilder TempValue = new StringBuilder();
            TempValue.Append("<table><thead><tr><th>Property Name</th><th>Property Value</th></tr></thead><tbody>");
            Type ObjectType = Object.GetType();
            PropertyInfo[] Properties = ObjectType.GetProperties();
            foreach (PropertyInfo Property in Properties)
            {
                TempValue.Append("<tr><td>").Append(Property.Name).Append("</td><td>");
                ParameterInfo[] Parameters = Property.GetIndexParameters();
                if (Property.CanRead && Parameters.Length == 0)
                {
                    try
                    {
                        object Value = Property.GetValue(Object, null);
                        TempValue.Append(Value == null ? "null" : Value.ToString());
                    }
                    catch { }
                }
                TempValue.Append("</td></tr>");
            }
            TempValue.Append("</tbody></table>");
            return TempValue.ToString();
        }

        /// <summary>
        /// Dumps the properties names and current values
        /// from an object type (used for static classes)
        /// </summary>
        /// <param name="ObjectType">Object type to dunp</param>
        /// <returns>An HTML formatted table containing the information about the object type</returns>
        public static string DumpProperties(Type ObjectType)
        {
            if (ObjectType == null)
                throw new ArgumentNullException("ObjectType");
            StringBuilder TempValue = new StringBuilder();
            TempValue.Append("<table><thead><tr><th>Property Name</th><th>Property Value</th></tr></thead><tbody>");
            PropertyInfo[] Properties = ObjectType.GetProperties();
            foreach (PropertyInfo Property in Properties)
            {
                TempValue.Append("<tr><td>").Append(Property.Name).Append("</td><td>");
                if (Property.GetIndexParameters().Length == 0)
                {
                    try
                    {
                        TempValue.Append(Property.GetValue(null, null) == null ? "null" : Property.GetValue(null, null).ToString());
                    }
                    catch { }
                }
                TempValue.Append("</td></tr>");
            }
            TempValue.Append("</tbody></table>");
            return TempValue.ToString();
        }

        #endregion

        #region GetAssembliesFromDirectory

        /// <summary>
        /// Gets a list of assemblies from a directory
        /// </summary>
        /// <param name="Directory">The directory to search in</param>
        /// <param name="Recursive">Determines whether to search recursively or not</param>
        /// <returns>List of assemblies in the directory</returns>
        public static System.Collections.Generic.List<Assembly> GetAssembliesFromDirectory(string Directory, bool Recursive = false)
        {
            System.Collections.Generic.List<Assembly> ReturnList = new System.Collections.Generic.List<Assembly>();
            System.Collections.Generic.List<FileInfo> Files = new DirectoryInfo(Directory).GetFiles("*", Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            foreach (FileInfo File in Files)
            {
                if (File.Extension.Equals(".dll", StringComparison.CurrentCultureIgnoreCase))
                {
                    ReturnList.Add(Assembly.LoadFile(File.FullName));
                }
            }
            return ReturnList;
        }

        #endregion

        #region GetLoadedAssembly

        /// <summary>
        /// Gets an assembly by its name if it is currently loaded
        /// </summary>
        /// <param name="Name">Name of the assembly to return</param>
        /// <returns>The assembly specified if it exists, otherwise it returns null</returns>
        public static System.Reflection.Assembly GetLoadedAssembly(string Name)
        {
            if (string.IsNullOrEmpty(Name))
                return null;
            foreach (Assembly TempAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (TempAssembly.GetName().Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                    return TempAssembly;
            }
            return null;
        }

        #endregion

        #region GetObjectsFromAssembly

        /// <summary>
        /// Returns an instance of all classes that it finds within an assembly
        /// that are of the specified base type/interface.
        /// </summary>
        /// <typeparam name="ClassType">Base type/interface searching for</typeparam>
        /// <param name="Assembly">Assembly to search within</param>
        /// <returns>A list of objects that are of the type specified</returns>
        public static System.Collections.Generic.List<ClassType> GetObjectsFromAssembly<ClassType>(Assembly Assembly)
        {
            System.Collections.Generic.List<Type> Types = GetTypes(Assembly, typeof(ClassType));
            System.Collections.Generic.List<ClassType> ReturnValues = new System.Collections.Generic.List<ClassType>();
            foreach (Type Type in Types)
            {
                ReturnValues.Add((ClassType)Activator.CreateInstance(Type));
            }
            return ReturnValues;
        }

        /// <summary>
        /// Returns an instance of all classes that it finds within an assembly
        /// that are of the specified base type/interface.
        /// </summary>
        /// <typeparam name="ClassType">Base type/interface searching for</typeparam>
        /// <param name="Directory">Directory to search within</param>
        /// <returns>A list of objects that are of the type specified</returns>
        public static System.Collections.Generic.List<ClassType> GetObjectsFromAssembly<ClassType>(string Directory)
        {
            System.Collections.Generic.List<ClassType> ReturnValues = new System.Collections.Generic.List<ClassType>();
            System.Collections.Generic.List<Assembly> Assemblies = LoadAssembliesFromDirectory(Directory, true);
            foreach (Assembly Assembly in Assemblies)
            {
                System.Collections.Generic.List<Type> Types = GetTypes(Assembly, typeof(ClassType));
                foreach (Type Type in Types)
                {
                    ReturnValues.Add((ClassType)Activator.CreateInstance(Type));
                }
            }
            return ReturnValues;
        }

        #endregion

        #region GetPropertyGetter

        /// <summary>
        /// Gets a lambda expression that calls a specific property's getter function
        /// </summary>
        /// <typeparam name="ClassType">Class type</typeparam>
        /// <typeparam name="DataType">Data type expecting</typeparam>
        /// <param name="PropertyName">Property name</param>
        /// <returns>A lambda expression that calls a specific property's getter function</returns>
        public static Expression<Func<ClassType, DataType>> GetPropertyGetter<ClassType, DataType>(string PropertyName)
        {
            if (string.IsNullOrEmpty(PropertyName))
                throw new ArgumentNullException("PropertyName");
            string[] SplitName = PropertyName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            PropertyInfo Property = Utilities.Reflection.Reflection.GetProperty<ClassType>(SplitName[0]);
            ParameterExpression ObjectInstance = Expression.Parameter(Property.DeclaringType, "x");
            MemberExpression PropertyGet = Expression.Property(ObjectInstance, Property);
            for (int x = 1; x < SplitName.Length; ++x)
            {
                Property = Utilities.Reflection.Reflection.GetProperty(Property.PropertyType, SplitName[x]);
                PropertyGet = Expression.Property(PropertyGet, Property);
            }
            if (Property.PropertyType != typeof(DataType))
            {
                UnaryExpression Convert = Expression.Convert(PropertyGet, typeof(DataType));
                return Expression.Lambda<Func<ClassType, DataType>>(Convert, ObjectInstance);
            }
            return Expression.Lambda<Func<ClassType, DataType>>(PropertyGet, ObjectInstance);
        }

        /// <summary>
        /// Gets a lambda expression that calls a specific property's getter function
        /// </summary>
        /// <typeparam name="ClassType">Class type</typeparam>
        /// <typeparam name="DataType">Data type expecting</typeparam>
        /// <param name="Property">Property</param>
        /// <returns>A lambda expression that calls a specific property's getter function</returns>
        public static Expression<Func<ClassType, DataType>> GetPropertyGetter<ClassType, DataType>(PropertyInfo Property)
        {
            if (!IsOfType(Property.PropertyType, typeof(DataType)))
                throw new ArgumentException("Property is not of the type specified");
            if (!IsOfType(Property.DeclaringType, typeof(ClassType)))
                throw new ArgumentException("Property is not from the declaring class type specified");
            ParameterExpression ObjectInstance = Expression.Parameter(Property.DeclaringType, "x");
            MemberExpression PropertyGet = Expression.Property(ObjectInstance, Property);
            if (Property.PropertyType != typeof(DataType))
            {
                UnaryExpression Convert = Expression.Convert(PropertyGet, typeof(DataType));
                return Expression.Lambda<Func<ClassType, DataType>>(Convert, ObjectInstance);
            }
            return Expression.Lambda<Func<ClassType, DataType>>(PropertyGet, ObjectInstance);
        }

        /// <summary>
        /// Gets a lambda expression that calls a specific property's getter function
        /// </summary>
        /// <typeparam name="ClassType">Class type</typeparam>
        /// <param name="PropertyName">Property name</param>
        /// <returns>A lambda expression that calls a specific property's getter function</returns>
        public static Expression<Func<ClassType, object>> GetPropertyGetter<ClassType>(string PropertyName)
        {
            return GetPropertyGetter<ClassType, object>(PropertyName);
        }

        /// <summary>
        /// Gets a lambda expression that calls a specific property's getter function
        /// </summary>
        /// <typeparam name="ClassType">Class type</typeparam>
        /// <param name="Property">Property</param>
        /// <returns>A lambda expression that calls a specific property's getter function</returns>
        public static Expression<Func<ClassType, object>> GetPropertyGetter<ClassType>(PropertyInfo Property)
        {
            return GetPropertyGetter<ClassType, object>(Property);
        }

        #endregion

        #region GetPropertySetter

        /// <summary>
        /// Gets a lambda expression that calls a specific property's setter function
        /// </summary>
        /// <typeparam name="ClassType">Class type</typeparam>
        /// <typeparam name="DataType">Data type expecting</typeparam>
        /// <param name="PropertyName">Property name</param>
        /// <returns>A lambda expression that calls a specific property's setter function</returns>
        public static Expression<Action<ClassType, DataType>> GetPropertySetter<ClassType, DataType>(string PropertyName)
        {
            if (string.IsNullOrEmpty(PropertyName))
                throw new ArgumentNullException("PropertyName");
            string[] SplitName = PropertyName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            PropertyInfo Property = Utilities.Reflection.Reflection.GetProperty<ClassType>(SplitName[0]);
            ParameterExpression ObjectInstance = Expression.Parameter(Property.DeclaringType, "x");
            ParameterExpression PropertySet = Expression.Parameter(typeof(DataType), "y");
            MethodCallExpression SetterCall = null;
            MemberExpression PropertyGet = null;
            if (SplitName.Length > 1)
            {
                PropertyGet = Expression.Property(ObjectInstance, Property);
                for (int x = 1; x < SplitName.Length - 1; ++x)
                {
                    Property = Utilities.Reflection.Reflection.GetProperty(Property.PropertyType, SplitName[x]);
                    PropertyGet = Expression.Property(PropertyGet, Property);
                }
                Property = Utilities.Reflection.Reflection.GetProperty(Property.PropertyType, SplitName[SplitName.Length - 1]);
            }
            if (Property.PropertyType != typeof(DataType))
            {
                UnaryExpression Convert = Expression.Convert(PropertySet, Property.PropertyType);
                if (PropertyGet == null)
                    SetterCall = Expression.Call(ObjectInstance, Property.GetSetMethod(), Convert);
                else
                    SetterCall = Expression.Call(PropertyGet, Property.GetSetMethod(), Convert);
                return Expression.Lambda<Action<ClassType, DataType>>(SetterCall, ObjectInstance, PropertySet);
            }
            if (PropertyGet == null)
                SetterCall = Expression.Call(ObjectInstance, Property.GetSetMethod(), PropertySet);
            else
                SetterCall = Expression.Call(PropertyGet, Property.GetSetMethod(), PropertySet);
            return Expression.Lambda<Action<ClassType, DataType>>(SetterCall, ObjectInstance, PropertySet);
        }

        /// <summary>
        /// Gets a lambda expression that calls a specific property's setter function
        /// </summary>
        /// <typeparam name="ClassType">Class type</typeparam>
        /// <typeparam name="DataType">Data type expecting</typeparam>
        /// <param name="PropertyName">Property name</param>
        /// <returns>A lambda expression that calls a specific property's setter function</returns>
        public static Expression<Action<ClassType, DataType>> GetPropertySetter<ClassType, DataType>(PropertyInfo Property)
        {
            if (!IsOfType(Property.PropertyType, typeof(DataType)))
                throw new ArgumentException("Property is not of the type specified");
            if (!IsOfType(Property.DeclaringType, typeof(ClassType)))
                throw new ArgumentException("Property is not from the declaring class type specified");

            ParameterExpression ObjectInstance = Expression.Parameter(Property.DeclaringType, "x");
            ParameterExpression PropertySet = Expression.Parameter(typeof(DataType), "y");
            MethodCallExpression SetterCall = null;
            if (Property.PropertyType != typeof(DataType))
            {

                UnaryExpression Convert = Expression.Convert(PropertySet, Property.PropertyType);
                SetterCall = Expression.Call(ObjectInstance, Property.GetSetMethod(), Convert);
                return Expression.Lambda<Action<ClassType, DataType>>(SetterCall, ObjectInstance, PropertySet);
            }
            SetterCall = Expression.Call(ObjectInstance, Property.GetSetMethod(), PropertySet);
            return Expression.Lambda<Action<ClassType, DataType>>(SetterCall, ObjectInstance, PropertySet);
        }

        /// <summary>
        /// Gets a lambda expression that calls a specific property's setter function
        /// </summary>
        /// <typeparam name="ClassType">Class type</typeparam>
        /// <param name="PropertyName">Property name</param>
        /// <returns>A lambda expression that calls a specific property's setter function</returns>
        public static Expression<Action<ClassType, object>> GetPropertySetter<ClassType>(string PropertyName)
        {
            return GetPropertySetter<ClassType, object>(PropertyName);
        }

        /// <summary>
        /// Gets a lambda expression that calls a specific property's setter function
        /// </summary>
        /// <typeparam name="ClassType">Class type</typeparam>
        /// <param name="PropertyName">Property name</param>
        /// <returns>A lambda expression that calls a specific property's setter function</returns>
        public static Expression<Action<ClassType, object>> GetPropertySetter<ClassType>(PropertyInfo Property)
        {
            return GetPropertySetter<ClassType, object>(Property);
        }

        #endregion

        #region GetProperty

        /// <summary>
        /// Gets a property based on a path
        /// </summary>
        /// <typeparam name="Source">Source type</typeparam>
        /// <param name="PropertyPath">Path to the property</param>
        /// <returns>The property info</returns>
        public static PropertyInfo GetProperty<Source>(string PropertyPath)
        {
            return GetProperty(typeof(Source), PropertyPath);
        }

        /// <summary>
        /// Gets a property based on a path
        /// </summary>
        /// <param name="Source">Source type</param>
        /// <param name="PropertyPath">Path to the property</param>
        /// <returns>The property info</returns>
        public static PropertyInfo GetProperty(Type SourceType, string PropertyPath)
        {
            if (string.IsNullOrEmpty(PropertyPath))
                return null;
            string[] Splitter = { "." };
            string[] SourceProperties = PropertyPath.Split(Splitter, StringSplitOptions.None);
            Type PropertyType = SourceType;
            PropertyInfo PropertyInfo = PropertyType.GetProperty(SourceProperties[0]);
            PropertyType = PropertyInfo.PropertyType;
            for (int x = 1; x < SourceProperties.Length; ++x)
            {
                PropertyInfo = PropertyType.GetProperty(SourceProperties[x]);
                PropertyType = PropertyInfo.PropertyType;
            }
            return PropertyInfo;
        }

        #endregion

        #region GetPropertyName

        /// <summary>
        /// Gets the name of the property held within the expression
        /// </summary>
        /// <typeparam name="T">The type of object used in the expression</typeparam>
        /// <param name="Expression">The expression</param>
        /// <returns>A string containing the name of the property</returns>
        public static string GetPropertyName<T>(Expression<Func<T, object>> Expression)
        {
            if (Expression == null)
                return "";
            string Name = "";
            if (Expression.Body.NodeType == ExpressionType.Convert)
            {
                Name = Expression.Body.ToString().Replace("Convert(", "").Replace(")", "");
                Name = Name.Remove(0, Name.IndexOf(".") + 1);
            }
            else
            {
                Name = Expression.Body.ToString();
                Name = Name.Remove(0, Name.IndexOf(".") + 1);
            }
            return Name;
        }

        /// <summary>
        /// Gets a property name
        /// </summary>
        /// <typeparam name="ClassType">Class type</typeparam>
        /// <typeparam name="DataType">Data type of the property</typeparam>
        /// <param name="Expression">LINQ expression</param>
        /// <returns>The name of the property</returns>
        public static string GetPropertyName<ClassType, DataType>(Expression<Func<ClassType, DataType>> Expression)
        {
            if (Expression.Body is UnaryExpression && Expression.Body.NodeType == ExpressionType.Convert)
            {
                MemberExpression Temp = (MemberExpression)((UnaryExpression)Expression.Body).Operand;
                return GetPropertyName(Temp.Expression) + Temp.Member.Name;
            }
            if (!(Expression.Body is MemberExpression))
                throw new ArgumentException("Expression.Body is not a MemberExpression");
            return GetPropertyName(((MemberExpression)Expression.Body).Expression) + ((MemberExpression)Expression.Body).Member.Name;
        }

        private static string GetPropertyName(Expression expression)
        {
            if (!(expression is MemberExpression))
                return "";
            return GetPropertyName(((MemberExpression)expression).Expression) + ((MemberExpression)expression).Member.Name + ".";
        }

        #endregion

        #region GetPropertyParent

        /// <summary>
        /// Gets a property's parent object
        /// </summary>
        /// <param name="SourceObject">Source object</param>
        /// <param name="PropertyPath">Path of the property (ex: Prop1.Prop2.Prop3 would be
        /// the Prop1 of the source object, which then has a Prop2 on it, which in turn
        /// has a Prop3 on it.)</param>
        /// <param name="PropertyInfo">Property info that is sent back</param>
        /// <returns>The property's parent object</returns>
        public static object GetPropertyParent(object SourceObject, string PropertyPath, out PropertyInfo PropertyInfo)
        {
            if (SourceObject == null)
            {
                PropertyInfo = null;
                return null;
            }
            string[] Splitter = { "." };
            string[] SourceProperties = PropertyPath.Split(Splitter, StringSplitOptions.None);
            object TempSourceProperty = SourceObject;
            Type PropertyType = SourceObject.GetType();
            PropertyInfo = PropertyType.GetProperty(SourceProperties[0]);
            PropertyType = PropertyInfo.PropertyType;
            for (int x = 1; x < SourceProperties.Length; ++x)
            {
                if (TempSourceProperty != null)
                {
                    TempSourceProperty = PropertyInfo.GetValue(TempSourceProperty, null);
                }
                PropertyInfo = PropertyType.GetProperty(SourceProperties[x]);
                PropertyType = PropertyInfo.PropertyType;
            }
            return TempSourceProperty;
        }

        #endregion

        #region GetPropertyValue

        /// <summary>
        /// Gets a property's value
        /// </summary>
        /// <param name="SourceObject">object who contains the property</param>
        /// <param name="PropertyPath">Path of the property (ex: Prop1.Prop2.Prop3 would be
        /// the Prop1 of the source object, which then has a Prop2 on it, which in turn
        /// has a Prop3 on it.)</param>
        /// <returns>The value contained in the property or null if the property can not
        /// be reached</returns>
        public static object GetPropertyValue(object SourceObject, string PropertyPath)
        {
            if (SourceObject == null || string.IsNullOrEmpty(PropertyPath))
                return null;
            string[] Splitter = { "." };
            string[] SourceProperties = PropertyPath.Split(Splitter, StringSplitOptions.None);
            object TempSourceProperty = SourceObject;
            Type PropertyType = SourceObject.GetType();
            for (int x = 0; x < SourceProperties.Length; ++x)
            {
                PropertyInfo SourcePropertyInfo = PropertyType.GetProperty(SourceProperties[x]);
                if (SourcePropertyInfo == null)
                    return null;
                TempSourceProperty = SourcePropertyInfo.GetValue(TempSourceProperty, null);
                if (TempSourceProperty == null)
                    return null;
                PropertyType = SourcePropertyInfo.PropertyType;
            }
            return TempSourceProperty;
        }

        #endregion

        #region GetPropertyType

        /// <summary>
        /// Gets a property's type
        /// </summary>
        /// <param name="SourceObject">object who contains the property</param>
        /// <param name="PropertyPath">Path of the property (ex: Prop1.Prop2.Prop3 would be
        /// the Prop1 of the source object, which then has a Prop2 on it, which in turn
        /// has a Prop3 on it.)</param>
        /// <returns>The type of the property specified or null if it can not be reached.</returns>
        public static Type GetPropertyType(object SourceObject, string PropertyPath)
        {
            if (SourceObject == null || string.IsNullOrEmpty(PropertyPath))
                return null;
            string[] Splitter = { "." };
            string[] SourceProperties = PropertyPath.Split(Splitter, StringSplitOptions.None);
            object TempSourceProperty = SourceObject;
            Type PropertyType = SourceObject.GetType();
            PropertyInfo PropertyInfo = null;
            for (int x = 0; x < SourceProperties.Length; ++x)
            {
                PropertyInfo = PropertyType.GetProperty(SourceProperties[x]);
                PropertyType = PropertyInfo.PropertyType;
            }
            return PropertyType;
        }

        /// <summary>
        /// Gets a property's type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="PropertyPath">Path of the property (ex: Prop1.Prop2.Prop3 would be
        /// the Prop1 of the source object, which then has a Prop2 on it, which in turn
        /// has a Prop3 on it.)</param>
        /// <returns>The type of the property specified or null if it can not be reached.</returns>
        public static Type GetPropertyType<T>(string PropertyPath)
        {
            if (string.IsNullOrEmpty(PropertyPath))
                return null;
            Type ObjectType = typeof(T);
            object Object = ObjectType.Assembly.CreateInstance(ObjectType.FullName);
            return GetPropertyType(Object, PropertyPath);
        }

        #endregion

        #region GetTypeName

        /// <summary>
        /// Returns the type's name
        /// </summary>
        /// <param name="ObjectType">object type</param>
        /// <returns>string name of the type</returns>
        public static string GetTypeName(Type ObjectType)
        {
            if (ObjectType == null)
                return "";
            StringBuilder Output = new StringBuilder();
            if (ObjectType.Name == "Void")
            {
                Output.Append("void");
            }
            else
            {
                if (ObjectType.Name.Contains("`"))
                {
                    Type[] GenericTypes = ObjectType.GetGenericArguments();
                    Output.Append(ObjectType.Name.Remove(ObjectType.Name.IndexOf("`")))
                        .Append("<");
                    string Seperator = "";
                    foreach (Type GenericType in GenericTypes)
                    {
                        Output.Append(Seperator).Append(GetTypeName(GenericType));
                        Seperator = ",";
                    }
                    Output.Append(">");
                }
                else
                {
                    Output.Append(ObjectType.Name);
                }
            }
            return Output.ToString();
        }

        #endregion

        #region GetTypes

        /// <summary>
        /// Gets a list of types based on an interface
        /// </summary>
        /// <param name="Assembly">Assembly to check</param>
        /// <param name="Interface">Interface to look for (also checks base class)</param>
        /// <returns>List of types that use the interface</returns>
        public static System.Collections.Generic.List<Type> GetTypes(Assembly Assembly, string Interface)
        {
            System.Collections.Generic.List<Type> ReturnList = new System.Collections.Generic.List<Type>();
            if (Assembly == null)
                return ReturnList;
            Type[] Types = Assembly.GetTypes();
            foreach (Type Type in Types)
            {
                if (CheckIsOfInterface(Type, Interface))
                    ReturnList.Add(Type);
            }
            return ReturnList;
        }

        /// <summary>
        /// Gets a list of types based on an interface
        /// </summary>
        /// <param name="Assembly">Assembly to check</param>
        /// <param name="Interface">Interface to look for (also checks base class)</param>
        /// <returns>List of types that use the interface</returns>
        public static System.Collections.Generic.List<Type> GetTypes(Assembly Assembly, Type Interface)
        {
            System.Collections.Generic.List<Type> ReturnList = new System.Collections.Generic.List<Type>();
            if (Assembly == null)
                return ReturnList;
            Type[] Types = Assembly.GetTypes();
            foreach (Type Type in Types)
            {
                if (CheckIsOfInterface(Type, Interface))
                    ReturnList.Add(Type);
            }
            return ReturnList;
        }

        /// <summary>
        /// Gets a list of types based on an interface
        /// </summary>
        /// <param name="AssemblyLocation">Location of the DLL</param>
        /// <param name="Interface">Interface to look for</param>
        /// <param name="Assembly">The assembly holding the types</param>
        /// <returns>A list of types that use the interface</returns>
        public static System.Collections.Generic.List<Type> GetTypes(string AssemblyLocation, string Interface, out Assembly Assembly)
        {
            Assembly = Assembly.LoadFile(AssemblyLocation);
            return GetTypes(Assembly, Interface);
        }

        #endregion

        #region GetTypesFromDirectory

        /// <summary>
        /// Gets a list of types based on an interface from all assemblies found in a directory
        /// </summary>
        /// <param name="AssemblyDirectory">Directory to search in</param>
        /// <param name="Interface">The interface to look for</param>
        /// <param name="Recursive">Determines whether to search recursively or not</param>
        /// <returns>A list mapping using the assembly as the key and a list of types</returns>
        public static ListMapping<Assembly, Type> GetTypesFromDirectory(string AssemblyDirectory, string Interface, bool Recursive = false)
        {
            ListMapping<Assembly, Type> ReturnList = new ListMapping<Assembly, Type>();
            System.Collections.Generic.List<Assembly> Assemblies = GetAssembliesFromDirectory(AssemblyDirectory, Recursive);
            foreach (Assembly Assembly in Assemblies)
            {
                Type[] Types = Assembly.GetTypes();
                foreach (Type Type in Types)
                {
                    if (Type.GetInterface(Interface, true) != null)
                    {
                        ReturnList.Add(Assembly, Type);
                    }
                }
            }
            return ReturnList;
        }

        #endregion

        #region IsIEnumerable

        /// <summary>
        /// Simple function to determine if an item is an IEnumerable
        /// </summary>
        /// <param name="ObjectType">Object type</param>
        /// <returns>True if it is, false otherwise</returns>
        public static bool IsIEnumerable(Type ObjectType)
        {
            return IsOfType(ObjectType, typeof(IEnumerable));
        }

        #endregion

        #region IsOfType

        /// <summary>
        /// Determines if an object is of a specific type
        /// </summary>
        /// <param name="Object">Object</param>
        /// <param name="Type">Type</param>
        /// <returns>True if it is, false otherwise</returns>
        public static bool IsOfType(object Object, Type Type)
        {
            if (Object == null)
                throw new ArgumentNullException("Object");
            return IsOfType(Object.GetType(), Type);
        }

        /// <summary>
        /// Determines if an object type is of a specific type
        /// </summary>
        /// <param name="ObjectType">Object type</param>
        /// <param name="Type">Base type</param>
        /// <returns>True if it is, false otherwise</returns>
        public static bool IsOfType(Type ObjectType, Type Type)
        {
            if (ObjectType == null)
                throw new ArgumentException("ObjectType");
            if (Type == null)
                throw new ArgumentException("Type");
            return CheckIsOfInterface(ObjectType, Type);
        }

        #endregion

        #region LoadAssembly

        /// <summary>
        /// Loads an assembly from a specific location
        /// </summary>
        /// <param name="Location">Location of the assembly</param>
        /// <returns>The loaded assembly</returns>
        public static Assembly LoadAssembly(string Location)
        {
            AssemblyName Name = AssemblyName.GetAssemblyName(Location);
            return AppDomain.CurrentDomain.Load(Name);
        }

        #endregion

        #region LoadAssembliesFromDirectory

        /// <summary>
        /// Loads an assembly from a specific location
        /// </summary>
        /// <param name="Directory">Directory to search for assemblies</param>
        /// <param name="Recursive">Search recursively</param>
        /// <returns>The loaded assemblies</returns>
        public static System.Collections.Generic.List<Assembly> LoadAssembliesFromDirectory(string Directory, bool Recursive = false)
        {
            System.Collections.Generic.List<Assembly> ReturnValues = new System.Collections.Generic.List<Assembly>();
            System.Collections.Generic.List<FileInfo> Files = new DirectoryInfo(Directory).GetFiles("*.dll", Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            foreach (FileInfo File in Files)
            {
                ReturnValues.Add(LoadAssembly(File.FullName));
            }
            return ReturnValues;
        }

        #endregion

        #region MakeShallowCopy

        /// <summary>
        /// Makes a shallow copy of the object
        /// </summary>
        /// <param name="Object">Object to copy</param>
        /// <param name="SimpleTypesOnly">If true, it only copies simple types (no classes, only items like int, string, etc.), false copies everything.</param>
        /// <returns>A copy of the object</returns>
        public static object MakeShallowCopy(object Object, bool SimpleTypesOnly)
        {
            if (Object == null)
                return null;
            Type ObjectType = Object.GetType();
            PropertyInfo[] Properties = ObjectType.GetProperties();
            FieldInfo[] Fields = ObjectType.GetFields();
            object ClassInstance = Activator.CreateInstance(ObjectType);

            foreach (PropertyInfo Property in Properties)
            {
                try
                {
                    if (Property.GetGetMethod() != null && Property.GetSetMethod() != null)
                    {
                        if (SimpleTypesOnly)
                        {
                            SetPropertyifSimpleType(Property, ClassInstance, Object);
                        }
                        else
                        {
                            SetProperty(Property, ClassInstance, Object);
                        }
                    }
                }
                catch { }
            }

            foreach (FieldInfo Field in Fields)
            {
                try
                {
                    if (SimpleTypesOnly)
                    {
                        SetFieldifSimpleType(Field, ClassInstance, Object);
                    }
                    else
                    {
                        SetField(Field, ClassInstance, Object);
                    }
                }
                catch { }
            }

            return ClassInstance;
        }

        #endregion

        #region MakeShallowCopyInheritedClass

        /// <summary>
        /// Makes a shallow copy of the object to a different class type (inherits from the original)
        /// </summary>
        /// <param name="DerivedType">Derived type</param>
        /// <param name="Object">Object to copy</param>
        /// <param name="SimpleTypesOnly">If true, it only copies simple types (no classes, only items like int, string, etc.), false copies everything.</param>
        /// <returns>A copy of the object</returns>
        public static object MakeShallowCopyInheritedClass(Type DerivedType, object Object, bool SimpleTypesOnly)
        {
            if (DerivedType == null)
                return null;
            if (Object == null)
                return null;
            Type ObjectType = Object.GetType();
            Type ReturnedObjectType = DerivedType;
            PropertyInfo[] Properties = ObjectType.GetProperties();
            FieldInfo[] Fields = ObjectType.GetFields();
            object ClassInstance = Activator.CreateInstance(ReturnedObjectType);

            foreach (PropertyInfo Property in Properties)
            {
                try
                {
                    PropertyInfo ChildProperty = ReturnedObjectType.GetProperty(Property.Name);
                    if (ChildProperty != null)
                    {
                        if (SimpleTypesOnly)
                        {
                            SetPropertyifSimpleType(ChildProperty, Property, ClassInstance, Object);
                        }
                        else
                        {
                            SetProperty(ChildProperty, Property, ClassInstance, Object);
                        }
                    }
                }
                catch { }
            }

            foreach (FieldInfo Field in Fields)
            {
                try
                {
                    FieldInfo ChildField = ReturnedObjectType.GetField(Field.Name);
                    if (ChildField != null)
                    {
                        if (SimpleTypesOnly)
                        {
                            SetFieldifSimpleType(ChildField, Field, ClassInstance, Object);
                        }
                        else
                        {
                            SetField(ChildField, Field, ClassInstance, Object);
                        }
                    }
                }
                catch { }
            }

            return ClassInstance;
        }

        #endregion

        #region SetValue

        /// <summary>
        /// Sets the value of destination property
        /// </summary>
        /// <param name="SourceValue">The source value</param>
        /// <param name="DestinationObject">The destination object</param>
        /// <param name="DestinationPropertyInfo">The destination property info</param>
        /// <param name="Format">Allows for formatting if the destination is a string</param>
        public static void SetValue(object SourceValue, object DestinationObject,
            PropertyInfo DestinationPropertyInfo, string Format)
        {
            if (DestinationObject == null || DestinationPropertyInfo == null)
                return;
            Type DestinationPropertyType = DestinationPropertyInfo.PropertyType;
            DestinationPropertyInfo.SetValue(DestinationObject,
                Parse(SourceValue, DestinationPropertyType, Format),
                null);
        }

        /// <summary>
        /// Sets the value of destination property
        /// </summary>
        /// <param name="SourceValue">The source value</param>
        /// <param name="DestinationObject">The destination object</param>
        /// <param name="PropertyPath">The path to the property (ex: MyProp.SubProp.FinalProp
        /// would look at the MyProp on the destination object, then find it's SubProp,
        /// and finally copy the SourceValue to the FinalProp property on the destination
        /// object)</param>
        /// <param name="Format">Allows for formatting if the destination is a string</param>
        public static void SetValue(object SourceValue, object DestinationObject,
            string PropertyPath, string Format)
        {
            string[] Splitter = { "." };
            string[] DestinationProperties = PropertyPath.Split(Splitter, StringSplitOptions.None);
            object TempDestinationProperty = DestinationObject;
            Type DestinationPropertyType = DestinationObject.GetType();
            PropertyInfo DestinationProperty = null;
            for (int x = 0; x < DestinationProperties.Length - 1; ++x)
            {
                DestinationProperty = DestinationPropertyType.GetProperty(DestinationProperties[x]);
                DestinationPropertyType = DestinationProperty.PropertyType;
                TempDestinationProperty = DestinationProperty.GetValue(TempDestinationProperty, null);
                if (TempDestinationProperty == null)
                    return;
            }
            DestinationProperty = DestinationPropertyType.GetProperty(DestinationProperties[DestinationProperties.Length - 1]);
            SetValue(SourceValue, TempDestinationProperty, DestinationProperty, Format);
        }

        #endregion

        #endregion

        #region Private Static Functions

        #region CheckIsOfInterface

        /// <summary>
        /// Checks if the type is of a specific interface type
        /// </summary>
        /// <param name="Type">Type to check</param>
        /// <param name="Interface">Interface to check against</param>
        /// <returns>True if it is, false otherwise</returns>
        private static bool CheckIsOfInterface(Type Type, Type Interface)
        {
            if (Type == null)
                return false;
            if (Type == Interface || Type.GetInterface(Interface.Name, true) != null)
                return true;
            return CheckIsOfInterface(Type.BaseType, Interface);
        }

        /// <summary>
        /// Checks if the type is of a specific interface type
        /// </summary>
        /// <param name="Type">Type to check</param>
        /// <param name="Interface">Name of the interface to check against</param>
        /// <returns>True if it is, false otherwise</returns>
        private static bool CheckIsOfInterface(Type Type, string Interface)
        {
            if (Type == null)
                return false;
            if (Type.GetInterface(Interface, true) != null)
                return true;
            if (!string.IsNullOrEmpty(Type.FullName) && Type.FullName.Contains(Interface))
                return true;
            return CheckIsOfInterface(Type.BaseType, Interface);
        }

        #endregion

        #region DiscoverFormatString

        /// <summary>
        /// Used to find the format string to use
        /// </summary>
        /// <param name="InputType">Input type</param>
        /// <param name="OutputType">Output type</param>
        /// <param name="FormatString">the string format</param>
        /// <returns>The format string to use</returns>
        private static string DiscoverFormatString(Type InputType,
            Type OutputType, string FormatString)
        {
            if (InputType == OutputType
                || InputType == typeof(string)
                || OutputType == typeof(string))
                return FormatString;
            if (InputType == typeof(float)
                || InputType == typeof(double)
                || InputType == typeof(decimal)
                || OutputType == typeof(float)
                || OutputType == typeof(double)
                || OutputType == typeof(decimal))
                return "f0";
            return FormatString;
        }

        #endregion

        #region Parse

        /// <summary>
        /// Parses the object and turns it into the requested output type
        /// </summary>
        /// <param name="Input">Input object</param>
        /// <param name="OutputType">Output type</param>
        /// <returns>An object with the requested output type</returns>
        internal static object Parse(object Input, Type OutputType)
        {
            return Parse(Input, OutputType, "");
        }

        /// <summary>
        /// Parses the string into the requested output type
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <param name="OutputType">Output type</param>
        /// <returns>An object with the requested output type</returns>
        private static object Parse(string Input, Type OutputType)
        {
            if (string.IsNullOrEmpty(Input))
                return null;
            return Parse(Input, OutputType, "");
        }

        /// <summary>
        /// Parses the object into the requested output type
        /// </summary>
        /// <param name="Input">Input object</param>
        /// <param name="OutputType">Output object</param>
        /// <param name="Format">format string (may be overridded if the conversion 
        /// involves a floating point value to "f0")</param>
        /// <returns>The object converted to the specified output type</returns>
        private static object Parse(object Input, Type OutputType, string Format)
        {
            if (Input == null || OutputType == null)
                return null;
            Type InputType = Input.GetType();
            Type BaseType = InputType;
            while (BaseType != OutputType)
            {
                BaseType = BaseType.BaseType;
                if (BaseType == null)
                    break;
            }
            if (BaseType == OutputType)
            {
                return Input;
            }
            else if (InputType == OutputType)
            {
                return Input;
            }
            else if (OutputType == typeof(string))
            {
                return Input.FormatToString(Format);
            }
            else
            {
                return CallMethod("Parse", OutputType.Assembly.CreateInstance(OutputType.FullName), Input.FormatToString(DiscoverFormatString(InputType, OutputType, Format)));
            }
        }

        #endregion

        #region SetField

        /// <summary>
        /// Copies a field value
        /// </summary>
        /// <param name="Field">Field object</param>
        /// <param name="ClassInstance">Class to copy to</param>
        /// <param name="Object">Class to copy from</param>
        private static void SetField(FieldInfo Field, object ClassInstance, object Object)
        {
            try
            {
                SetField(Field, Field, ClassInstance, Object);
            }
            catch { }
        }

        /// <summary>
        /// Copies a field value
        /// </summary>
        /// <param name="ChildField">Child field object</param>
        /// <param name="Field">Field object</param>
        /// <param name="ClassInstance">Class to copy to</param>
        /// <param name="Object">Class to copy from</param>
        private static void SetField(FieldInfo ChildField, FieldInfo Field, object ClassInstance, object Object)
        {
            try
            {
                if (Field.IsPublic && ChildField.IsPublic)
                {
                    ChildField.SetValue(ClassInstance, Field.GetValue(Object));
                }
            }
            catch { }
        }

        #endregion

        #region SetFieldIfSimpleType

        /// <summary>
        /// Copies a field value
        /// </summary>
        /// <param name="Field">Field object</param>
        /// <param name="ClassInstance">Class to copy to</param>
        /// <param name="Object">Class to copy from</param>
        private static void SetFieldifSimpleType(FieldInfo Field, object ClassInstance, object Object)
        {
            try
            {
                SetFieldifSimpleType(Field, Field, ClassInstance, Object);
            }
            catch { }
        }

        /// <summary>
        /// Copies a field value
        /// </summary>
        /// <param name="ChildField">Child field object</param>
        /// <param name="Field">Field object</param>
        /// <param name="ClassInstance">Class to copy to</param>
        /// <param name="Object">Class to copy from</param>
        private static void SetFieldifSimpleType(FieldInfo ChildField, FieldInfo Field, object ClassInstance, object Object)
        {
            Type FieldType = Field.FieldType;
            if (Field.FieldType.FullName.StartsWith("System.Collections.Generic.List", StringComparison.CurrentCultureIgnoreCase))
            {
                FieldType = Field.FieldType.GetGenericArguments()[0];
            }

            if (FieldType.FullName.StartsWith("System"))
            {
                SetField(ChildField, Field, ClassInstance, Object);
            }
        }

        #endregion

        #region SetProperty

        /// <summary>
        /// Copies a property value
        /// </summary>
        /// <param name="Property">Property object</param>
        /// <param name="ClassInstance">Class to copy to</param>
        /// <param name="Object">Class to copy from</param>
        private static void SetProperty(PropertyInfo Property, object ClassInstance, object Object)
        {
            try
            {
                if (Property.GetGetMethod() != null && Property.GetSetMethod() != null)
                {
                    SetProperty(Property, Property, ClassInstance, Object);
                }
            }
            catch { }
        }

        /// <summary>
        /// Copies a property value
        /// </summary>
        /// <param name="ChildProperty">Child property object</param>
        /// <param name="Property">Property object</param>
        /// <param name="ClassInstance">Class to copy to</param>
        /// <param name="Object">Class to copy from</param>
        private static void SetProperty(PropertyInfo ChildProperty, PropertyInfo Property, object ClassInstance, object Object)
        {
            try
            {
                if (ChildProperty.GetSetMethod() != null && Property.GetGetMethod() != null)
                {
                    ChildProperty.SetValue(ClassInstance, Property.GetValue(Object, null), null);
                }
            }
            catch { }
        }

        #endregion

        #region SetPropertyIfSimpleType

        /// <summary>
        /// Copies a property value
        /// </summary>
        /// <param name="Property">Property object</param>
        /// <param name="ClassInstance">Class to copy to</param>
        /// <param name="Object">Class to copy from</param>
        private static void SetPropertyifSimpleType(PropertyInfo Property, object ClassInstance, object Object)
        {
            try
            {
                if (Property.GetGetMethod() != null && Property.GetSetMethod() != null)
                {
                    SetPropertyifSimpleType(Property, Property, ClassInstance, Object);
                }
            }
            catch { }
        }

        /// <summary>
        /// Copies a property value
        /// </summary>
        /// <param name="ChildProperty">Child property object</param>
        /// <param name="Property">Property object</param>
        /// <param name="ClassInstance">Class to copy to</param>
        /// <param name="Object">Class to copy from</param>
        private static void SetPropertyifSimpleType(PropertyInfo ChildProperty, PropertyInfo Property, object ClassInstance, object Object)
        {
            Type PropertyType = Property.PropertyType;
            if (Property.PropertyType.FullName.StartsWith("System.Collections.Generic.List", StringComparison.CurrentCultureIgnoreCase))
            {
                PropertyType = Property.PropertyType.GetGenericArguments()[0];
            }

            if (PropertyType.FullName.StartsWith("System"))
            {
                SetProperty(ChildProperty, Property, ClassInstance, Object);
            }
        }

        #endregion

        #endregion
    }
}