using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace StickFightExtendedPlayers
{
    public class TomlTypeConverter_Patches
    {
        private static bool IsListType(Type type)
        {
            return typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        }

        [HarmonyPatch(typeof(TomlTypeConverter), nameof(TomlTypeConverter.ConvertToString))]
        [HarmonyPrefix]
        static bool ConvertToStringPrefix(object value, Type valueType, ref string __result)
        {
            if (IsListType(valueType))
            {
                var elementType = valueType.IsArray
                    ? valueType.GetElementType()
                    : valueType.GetGenericArguments()[0];

                var items = ((IEnumerable)value)
                    .Cast<object>()
                    .Select(item => TomlTypeConverter.ConvertToString(item, elementType));

                __result = "[" + string.Join(", ", [..items]) + "]";
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(TomlTypeConverter), nameof(TomlTypeConverter.ConvertToValue), [typeof(string), typeof(Type)])]
        [HarmonyPrefix]
        static bool ConvertToValuePrefix(string value, Type valueType, ref object __result)
        {
            if (IsListType(valueType))
            {
                var elementType = valueType.IsArray
                    ? valueType.GetElementType()
                    : valueType.GetGenericArguments()[0];

                var trimmed = value.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    trimmed = trimmed.Substring(1, trimmed.Length - 2);

                if (trimmed.IsNullOrWhiteSpace())
                {
                    __result = valueType.IsArray
                        ? Array.CreateInstance(elementType, 0)
                        : Activator.CreateInstance(valueType);
                    return false;
                }

                var parts = SplitTomlList(trimmed);

                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                foreach (var part in parts)
                {
                    list.Add(TomlTypeConverter.ConvertToValue(part, elementType));
                }

                if (valueType.IsArray)
                {
                    var array = Array.CreateInstance(elementType, list.Count);
                    list.CopyTo(array, 0);
                    __result = array;
                    return false;
                }

                __result = list;
                return false;
            }
            return true;
        }
        public static List<string> SplitTomlList(string input)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(input))
                return result;

            var sb = new StringBuilder();
            int depth = 0;
            bool inQuotes = false;
            char quoteChar = '\0';

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if ((c == '"' || c == '\''))
                {
                    if (inQuotes && c == quoteChar)
                        inQuotes = false;
                    else if (!inQuotes)
                    {
                        inQuotes = true;
                        quoteChar = c;
                    }
                }

                if (!inQuotes)
                {
                    if (c == '[' || c == '{') depth++;
                    if (c == ']' || c == '}') depth--;

                    if (c == ',' && depth == 0)
                    {
                        result.Add(sb.ToString().Trim());
                        sb.Length = 0;
                        continue;
                    }
                }

                sb.Append(c);
            }

            if (sb.Length > 0)
                result.Add(sb.ToString().Trim());

            return result;
        }
        static PropertyInfo p_TypeConverters = AccessTools.Property(typeof(TomlTypeConverter), "TypeConverters");
        [HarmonyPatch(typeof(TomlTypeConverter), nameof(TomlTypeConverter.GetSupportedTypes))]
        [HarmonyPrefix]
        static bool GetSupportedTypesPrefix(ref IEnumerable<Type> __result)
        {
            Dictionary<Type, BepInEx.Configuration.TypeConverter> TypeConverters = (Dictionary<Type, BepInEx.Configuration.TypeConverter>)p_TypeConverters.GetValue(null, null);
            
            var baseTypes = TypeConverters.Keys.ToList();

            var listAndArrayTypes = baseTypes
                .SelectMany(t => new[]
                {
                    typeof(List<>).MakeGenericType(t),
                    t.MakeArrayType()
                });

            var enumTypes = baseTypes
                .Where(t => t == typeof(Enum))
                .SelectMany(_ =>
                    AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .Where(t => t.IsEnum)
                        .SelectMany(enumType => new[]
                        {
                            enumType,
                            typeof(List<>).MakeGenericType(enumType),
                            enumType.MakeArrayType()
                        }));

            __result = baseTypes
                .Concat(listAndArrayTypes)
                .Concat(enumTypes)
                .Distinct()
                .ToList();
            return false;
        }
        [HarmonyPatch(typeof(TomlTypeConverter), nameof(TomlTypeConverter.CanConvert))]
        [HarmonyPrefix]
        static bool CanConvertPrefix(Type type, ref bool __result)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsEnum)
            {
                __result = true;
                return false;
            }

            if (type.IsArray)
            {
                __result = TomlTypeConverter.CanConvert(type.GetElementType());
                return false;
            }

            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                if (genericDef == typeof(List<>) || genericDef == typeof(IList<>) || genericDef == typeof(IEnumerable<>))
                {
                    var elementType = type.GetGenericArguments()[0];
                    __result = TomlTypeConverter.CanConvert(elementType);
                    return false;
                }
            }

            return true;
        }
    }
}
