using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ProjectERA.Services.Network.Protocols
{
    internal partial class GeneralCache<K, T>
    {
        /// <summary>
        /// Gets approximate size of object
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static UInt64 ApproximateSize(Object item)
        {
            Double size = 0;

            FieldInfo[] fields = item.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo m in fields)
            {
                // All valuetypes except for usertypes and structs
                if (m.FieldType.IsPrimitive)
                {
                    // TODO: variable part should be marked for rechecking.
                    if (m.FieldType == typeof(String))
                        size += (m.GetValue(item) as String).Length;
                    else
                        size += SizeOfPrimitiveType(m.FieldType);
                }
                // Must be a struct
                else if (m.FieldType.IsValueType)
                {
                    size += ApproximateSize(m.GetValue(item));
                }
                // Array
                else if (m.FieldType.IsArray)
                {
                    // TODO: variable part should be marked for rechecking.
                    size += SizeOfArray(m.GetValue(item) as Array);
                }
                // Lists, Dictionaries and so on
                else if (m.FieldType.IsGenericType)
                {

                }
                // Interfaces
                else if (m.FieldType.IsInterface)
                {
                    
                }
            }

            return (UInt64)(Math.Ceiling(size));
        }

        /// <summary>
        /// Gets approximate size of array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private static Double SizeOfArray(Array array)
        {
            Double size = 0;
            foreach (Object element in array)
            {
                Type nestedType = element.GetType();
                if (nestedType.IsPrimitive)
                    if (nestedType == typeof(String))
                        size += (element as String).Length;
                    else
                        size += SizeOfPrimitiveType(nestedType);
                else if (nestedType.IsValueType)
                    size += ApproximateSize(element);
                else if (nestedType.IsArray)
                    size += SizeOfArray(((Array)element));
            }
            return size;
        }

        /// <summary>
        /// Gets exact size of primitive type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Double SizeOfPrimitiveType(Type type)
        {
            if (type == typeof(Boolean))
                return (1 / 8f);
            if (type == typeof(Byte) || type == typeof(SByte))
                return 1;
            else if (type == typeof(Int16) || type == typeof(UInt16) || type == typeof(Char))
                return 2;
            else if (type == typeof(Int32) || type == typeof(UInt32) || type == typeof(Single))
                return 4;
            else if (type == typeof(Int64) || type == typeof(UInt64) || type == typeof(Double))
                return 8;
            else if (type == typeof(Decimal))
                return 16;

            return 0;
        }
    }
}
