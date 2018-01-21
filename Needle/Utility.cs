using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using IMS.Common.Needle.Attributes;
using IMS.Common.Needle.Extensions;

namespace IMS.Common.Needle
{
    internal static class Utility
    {
        public enum PropertyType
        {
            Flat,
            Collection
        }

        public static string GetCompositePropertyValue(object src, string name)
        {
            var route = name.Split('.');
            var t = ResolveRoute(route, 0, 11, src, true);

            if (t == null || !t.Any())
            {
                return null;
            }

            {
                var sb = new StringBuilder();
                foreach (var e in t)
                {
                    var p = e.Item1;
                    var o = e.Item2;
                    var v = p.GetValue(o, null);

                    var isString = v is string;
                    var isEnumerable = v is IEnumerable;

                    if (!isString && isEnumerable)
                    {
                        var subSb = new StringBuilder();
                        var enumerable = v as IEnumerable;

                        foreach (var f in enumerable)
                        {
                            subSb.Append(f);
                            subSb.Append(NeedleSearchExtensions.ElementDelimiter);
                        }

                        sb.Append(subSb.ToString().TrimEnd(NeedleSearchExtensions.ElementDelimiter));
                    }
                    else
                    {
                        sb.Append(v);
                    }

                    sb.Append(NeedleSearchExtensions.SubDelimiter);
                }

                return sb.ToString().TrimEnd(NeedleSearchExtensions.SubDelimiter);
            }
        }

        public static Type GetPropertyType(object src, string name)
        {
            var route = name.Split('.');

            var l = ResolveRoute(route, 0, 11, src);
            var e = l.FirstOrDefault();

            var p = e?.Item1;
            return p?.PropertyType;
        }

        public static IEnumerable<PropertyInfo> GetPropertiesOnObject(object o)
        {
            var attrs = o.GetType().GetProperties();
            return attrs.ToList();
        }

        private static object GetPropertyValue(PropertyInfo property, object o)
        {
            return property.GetValue(o, null);
        }

        private static object ResolveNullDestination(Tuple<PropertyInfo, object> sourceProp)
        {
            var t = sourceProp.Item1;
            var parent = sourceProp.Item2;

            CreateInstance(t, parent);
            return GetPropertyValue(sourceProp.Item1, sourceProp.Item2);
        }

        private static void CreateInstance(PropertyInfo t, object parent)
        {
            var o = Activator.CreateInstance(t.PropertyType); //I hope it has a parameterless constructor
            var p = parent.GetType().GetProperty(t.Name);
            p?.SetValue(parent, o);
        }

        private static List<Tuple<PropertyInfo, object>> ResolveRoute(IReadOnlyList<string> route, int routeIndex,
            int limit,
            object source, bool readOnly = false)
        {
            var o = new List<Tuple<PropertyInfo, object>>();
            while (true)
            {
                if (routeIndex > limit)
                {
                    throw new Exception(
                        "Aborted recursive routing, will not attempt to use an index greater than " +
                        limit);
                }

                //get the current route position
                var current = route[routeIndex];

                //resolve all sub paths if this is an IEnumerable
                var isString = source is string;
                var isEnumerable = source is IEnumerable;

                if (!isString && isEnumerable)
                {
                    var preEnumerable = source as IEnumerable;
                    var type = preEnumerable.GetType().GetGenericArguments()[0];
                    var subRoute = route.Skip(routeIndex).ToArray();

                    if (!preEnumerable.AsQueryable().Any())
                    {
                        if (readOnly)
                        {
                            return null;
                        }

                        var listType = typeof(List<>);
                        var constructedListType = listType.MakeGenericType(type);
                        var list = (IList) Activator.CreateInstance(constructedListType);

                        var u = Activator.CreateInstance(type);
                        list.Add(u);

                        source = list;
                    }

                    var enumerable = (IEnumerable) source;
                    foreach (var e in enumerable)
                    {
                        o.AddRange(ResolveRoute(subRoute, 0, 11, e));
                    }

                    return o;
                }

                //get list of properties on source
                var props = GetPropertiesOnObject(source).ToList();
                //select the one that matches this step of the route
                var prop = props.Single(x => x.Name == current);

                //is this the end of the chain?
                if (route.Count - 1 == routeIndex)
                {
                    o.Add(new Tuple<PropertyInfo, object>(prop, source));
                    return o;
                }

                var sp = GetPropertyValue(prop, source);
                if (sp == null)
                {
                    if (!readOnly)
                    {
                        //instantiate the property so we can continue resolving
                        sp = ResolveNullDestination(new Tuple<PropertyInfo, object>(prop, source));
                    }
                    else
                    {
                        //if readonly, do not modify the instance
                        return null;
                    }
                }

                //go further down the chain
                routeIndex = routeIndex + 1;
                source = sp;
            }
        }

        public static List<Tuple<string, PropertyType>> TokenizeRoute(IReadOnlyList<string> route, int routeIndex,
            int limit,
            object source, bool readOnly = false)
        {
            var o = new List<Tuple<string, PropertyType>>();

            while (true)
            {
                if (routeIndex > limit)
                {
                    throw new Exception(
                        "Aborted recursive routing, will not attempt to use an index greater than " +
                        limit);
                }

                //get the current route position
                var current = route[routeIndex];

                //resolve a single sub path if this is an IEnumerable
                var isString = source is string;
                var isEnumerable = source is IEnumerable;

                if (!isString && isEnumerable)
                {
                    var preEnumerable = source as IEnumerable;
                    var type = preEnumerable.GetType().GetGenericArguments()[0];
                    var subRoute = route.Skip(routeIndex).ToArray();

                    if (!preEnumerable.AsQueryable().Any())
                    {
                        if (readOnly)
                        {
                            return null;
                        }

                        var listType = typeof(List<>);
                        var constructedListType = listType.MakeGenericType(type);
                        var list = (IList) Activator.CreateInstance(constructedListType);

                        var u = Activator.CreateInstance(type);
                        list.Add(u);

                        source = list;
                    }

                    var enumerable = ((IEnumerable) source).AsQueryable().Cast<object>().ToArray();
                    o.AddRange(TokenizeRoute(subRoute, 0, 11, enumerable[0]));

                    return o;
                }

                //get list of properties on source
                var props = GetPropertiesOnObject(source).ToList();
                //select the one that matches this step of the route
                var prop = props.Single(x => x.Name == current);

                //is this the end of the chain?
                if (route.Count - 1 == routeIndex)
                {
                    o.Add(new Tuple<string, PropertyType>(current, PropertyType.Flat));
                    return o;
                }

                var sp = GetPropertyValue(prop, source);
                if (sp == null)
                {
                    if (!readOnly)
                    {
                        //instantiate the property so we can continue resolving
                        sp = ResolveNullDestination(new Tuple<PropertyInfo, object>(prop, source));
                    }
                    else
                    {
                        //if readonly, do not modify the instance
                        return null;
                    }
                }

                //go further down the chain
                routeIndex = routeIndex + 1;
                source = sp;

                if (source is IEnumerable)
                {
                    o.Add(new Tuple<string, PropertyType>(current, PropertyType.Collection));
                }
                else
                {
                    o.Add(new Tuple<string, PropertyType>(current, PropertyType.Flat));
                }
            }
        }

        public static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetMetadata<T>()
        {
            var type = typeof(T);
            var metadataType = type.GetCustomAttributes(typeof(MetadataTypeAttribute), true)
                .OfType<MetadataTypeAttribute>().FirstOrDefault();

            if (metadataType != null)
            {
                return metadataType.MetadataClassType.GetProperties()
                    .Where(x => x.GetCustomAttributes().Any(y => y is HaystackAttribute)).ToList();
            }

            return new List<PropertyInfo>();
        }

        public static List<IHaystackAttribute> GetHaystackAttributes<T>()
        {
            var props = new List<IHaystackAttribute>();

            var hasAttr = typeof(T).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(HaystackAttribute))).ToList();
            hasAttr.AddRange(GetMetadata<T>());

            foreach (var prop in hasAttr)
            {
                var hs = prop.GetCustomAttributes().Where(x => x is HaystackAttribute).Cast<HaystackAttribute>()
                    .ToList();
                var displayName = prop.GetCustomAttributes().Where(x => x is DisplayNameAttribute)
                    .Cast<DisplayNameAttribute>().SingleOrDefault()?.DisplayName;
                var basePath = prop.Name;
                var hp = hs.Select(
                    haystack => new HaystackProperty
                    {
                        DisplayName = string.IsNullOrEmpty(displayName)
                            ? $"{basePath}.{haystack.Path}".TrimEnd('.')
                            : displayName,
                        Path = $"{basePath}.{haystack.Path}".TrimEnd('.'),
                        IsVisible = haystack.IsVisible,
                        QueryRender = haystack.QueryRender
                    });
                props.AddRange(hp);
            }

            return props;
        }

        public static string BuildAccessor(IHaystackAttribute haystackProperty, Type type,
            List<Tuple<string, PropertyType>> route)
        {
            string accessor;
            if (string.IsNullOrWhiteSpace(haystackProperty.QueryRender))
            {
                var isCollection = false;
                if (type.Namespace == "System.Collections.Generic")
                {
                    type = type.GetGenericArguments()[0];
                    isCollection = true;
                }

                //these types are unsupported due to limitations in Linq-To-Entities
                if (type == typeof(bool) || type == typeof(bool?) || type == typeof(decimal) ||
                    type == typeof(decimal?))
                {
                    throw new NotImplementedException(
                        $"{type} is not supported by the haystack query generator. " +
                        "Consider a custom QueryRender and IsVisible expression, or performing additional filtering on the haystack query.");
                }

                var nb = "***";
                var lv = "x";

                //build the code for this route
                var selectMany = route.Count(x => x.Item2 == PropertyType.Collection) - 1;

                foreach (var step in route)
                {
                    switch (step.Item2)
                    {
                        case PropertyType.Collection:
                            var selector = selectMany > 0 ? "SelectMany" : "Select";
                            selectMany--;

                            nb = nb.Replace("***", $".{step.Item1}.{selector}({lv} => {lv}***)");
                            lv += "x";
                            break;
                        case PropertyType.Flat:
                            nb = nb.Replace("***", $".{step.Item1}***");
                            break;
                        default:
                            throw new NotImplementedException(
                                $"{step.Item2} is not a supported route property type.");
                    }
                }

                //finish building the accessor
                if (!isCollection)
                {
                    nb = nb.Replace("***", type == typeof(string) ? ".ToLower()" : ".ToString().ToLower()");

                    if (route.Any(x => x.Item2 == PropertyType.Collection))
                    {
                        nb += ".Where(v => v***).Any()";
                    }
                    else
                    {
                        nb += "***";
                    }
                }
                else
                {
                    nb = nb.Replace("***",
                        type == typeof(string)
                            ? ".Select(u => u.ToLower()).Where(v => v***).Any()"
                            : ".Select(u => u.ToString().ToLower()).Where(v => v***).Any()");
                }

                accessor = nb.Trim('.');
            }
            else
            {
                accessor = haystackProperty.QueryRender + "***";
            }

            return accessor;
        }

        internal class HaystackProperty : IHaystackAttribute
        {
            public string Path { get; set; }
            public string IsVisible { get; set; }
            public string QueryRender { get; set; }
            public string DisplayName { get; set; }
        }
    }
}