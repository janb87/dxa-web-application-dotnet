﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Sdl.Web.Mvc.Models;

namespace Sdl.Web.Mvc.Mapping
{
    public abstract class BaseViewModelBuilder : IViewModelBuilder
    {
        private static Dictionary<Type, Dictionary<string, List<SemanticProperty>>> _entityPropertySemantics;
        public static Dictionary<Type, Dictionary<string, List<SemanticProperty>>> EntityPropertySemantics 
        {
            get
            {
                if (_entityPropertySemantics == null)
                {
                    _entityPropertySemantics = new Dictionary<Type, Dictionary<string, List<SemanticProperty>>>();
                }
                return _entityPropertySemantics;
            }
            set
            {
                _entityPropertySemantics = value;
            }
        }
        
        public abstract object Create(object sourceEntity, Type type);

        protected virtual Dictionary<string, string> GetVocabulariesFromType(Type type)
        {
            bool addedDefaults = false;
            Dictionary<string, string> res = new Dictionary<string, string>();
            foreach (var attr in type.GetCustomAttributes())
            {
                if (attr is SemanticEntityAttribute)
                {
                    var semantics = (SemanticEntityAttribute)attr;
                    res.Add(semantics.Prefix, semantics.Vocab);
                }
                if (attr is SemanticDefaultsAttribute)
                {
                    var semantics = (SemanticDefaultsAttribute)attr;
                    res.Add(semantics.Prefix, semantics.Vocab);
                    addedDefaults = true;
                }
            }
            //Add default vocab if none was specified on entity
            if (!addedDefaults)
            {
                var semantics = new SemanticDefaultsAttribute();
                res.Add(semantics.Prefix, semantics.Vocab);
            }
            return res;
        }

        protected virtual Dictionary<string, string> GetEntitiesFromType(Type type)
        {
            Dictionary<string,string> res = new Dictionary<string,string>();
            foreach (var attr in type.GetCustomAttributes())
            {
                if (attr is SemanticEntityAttribute)
                {
                    var semantics = (SemanticEntityAttribute)attr;
                    res.Add(semantics.Prefix, semantics.EntityName);
                }
            }
            return res;
        }

        protected virtual Dictionary<string, List<SemanticProperty>> LoadPropertySemantics(Type type)
        {
            //The default prefix is empty - this implies that the prefix should be inherited from the parent object default prefix
            var defaultPrefix = "";
            var mapAllProperties = true;
            foreach (var attr in type.GetCustomAttributes())
            {
                if (attr is SemanticDefaultsAttribute)
                {
                    defaultPrefix = ((SemanticDefaultsAttribute)attr).Prefix;
                    mapAllProperties = ((SemanticDefaultsAttribute)attr).MapAllProperties;
                    break;
                }
            }
            if (!EntityPropertySemantics.ContainsKey(type))
            {
                var result = new Dictionary<string, List<SemanticProperty>>();
                foreach (var pi in type.GetProperties())
                {
                    var name = pi.Name;
                    //flag to indicate we have processed a default mapping, or we explicitly should ignore this property when mapping
                    bool ignore = false;
                    foreach (var attr in pi.GetCustomAttributes(true))
                    {
                        if (attr is SemanticPropertyAttribute)
                        {
                            var propertySemantics = (SemanticPropertyAttribute)attr;
                            if (!propertySemantics.IgnoreMapping)
                            {
                                if (!result.ContainsKey(name))
                                {
                                    result.Add(name, new List<SemanticProperty>());
                                }
                                var bits = ((SemanticPropertyAttribute)attr).PropertyName.Split(':');
                                if (bits.Length > 1)
                                {
                                    result[name].Add(new SemanticProperty(bits[0], bits[1]));
                                }
                                else
                                {
                                    //Add the default prefix and set the ignore flag - so no need to apply default mapping using property name
                                    result[name].Add(new SemanticProperty(defaultPrefix, bits[0]));
                                    ignore = true;
                                }
                            }
                            else
                            {
                                ignore = true;
                            }
                        }
                    }
                    if (!ignore && mapAllProperties)
                    {
                        if (!result.ContainsKey(name))
                        {
                            result.Add(name, new List<SemanticProperty>());
                        }
                        //Add default semantics 
                        result[name].Add(GetDefaultPropertySemantics(pi, defaultPrefix));
                    }
                
                }
                EntityPropertySemantics.Add(type, result);
            }
            return EntityPropertySemantics[type];
        }

        protected virtual SemanticProperty GetDefaultPropertySemantics(PropertyInfo pi, string defaultPrefix)
        {
            //This is where we map model class property names to Tridion schema xml field names
            //Which is: the property name with the first character lower case and the last character removed if its an 's' and a List<> type (so Paragraphs becomes paragraph etc.)
            var name = pi.Name.Substring(0, 1).ToLower() + pi.Name.Substring(1);
            if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(List<>) && name.EndsWith("s"))
            {
                name = name.Substring(0,name.Length-1);
            }
            return new SemanticProperty(defaultPrefix, name);
        }
    }
}