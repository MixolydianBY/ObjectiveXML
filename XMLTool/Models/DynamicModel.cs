namespace ObjectiveXML.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using MEFLight.Attributes;
    using Newtonsoft.Json.Linq;
    using Services;
    using Services.Json;
    using Utilities;

    internal class DynamicModel : DynamicObject
    {
        private readonly IDictionary<string, object> _innerData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);


        public DynamicModel()
        {
            MefLightFactory.Singleton.ResolveImports(this);
        }

        [Import]
        public JsonParser Parser { get; set; }

        [Import]
        public ModelConversionsEngine ConversionsEngine { get; set; }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _innerData.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {

            return _innerData.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            object oldValue;

            _innerData.TryGetValue(binder.Name, out oldValue);

            if(oldValue != value)
            {
                if (value is JToken)
                {
                    value = Parser.ToDynamicModel(value.ToString());
                }

                _innerData[binder.Name] = value;
            }
                 
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            bool flag = false;

            if (binder.Type == typeof(XmlDocument) 
                && !binder.Type.IsAbstract)
            {
                result = ConversionsEngine.CreateXmlDocument(_innerData);
                return true;
            }

            if (!binder.Type.IsAbstract
                && !typeof(ICollection).IsAssignableFrom(binder.Type)
                && !typeof(IEnumerable).IsAssignableFrom(binder.Type)
                && _innerData.Count != 0)
            {
                try
                {
                    dynamic dynamicData = this.InnerObjects.FirstOrDefault();

                    if (dynamicData != null)
                    {
                        IDictionary<string, object> objData = dynamicData;
                        result = CustomActivator.CreateInstanceAndPopulate(binder.Type, objData);
                    }
                    else
                    {
                        result = CustomActivator.CreateInstanceAndPopulate(binder.Type, _innerData);
                    }

                    flag = true;
                }
                catch (InvalidCastException e)
                {
                    result = null;
                }
                catch (Exception e)
                {
                    result = null;
                }

                return flag;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(binder.Type))
            {
                if (binder.Type == typeof(string))
                {
                    result = this.ToString();
                }
                else if (binder.Type == typeof(IDictionary<string, object>))
                {
                    result = _innerData;                  
                }
                else if (binder.Type.IsGenericType)
                {
                    Type genericType = binder.Type.GenericTypeArguments[0];

                    if (genericType == typeof(XmlDocument))
                    {
                        List<XmlDocument> collection = new List<XmlDocument>();

                        foreach (var entry in _innerData)
                        {
                            collection.Add(ConversionsEngine.CreateXmlDocument(entry));
                        }

                        result = collection;
                    }
                    else
                    {
                        result = ConversionsEngine.CreateCollection(genericType, InnerObjects);
                    }                             
                }
                else if(binder.Type.BaseType == typeof(Array))
                {
                    
                    result = ConversionsEngine.CreateArray(binder.Type.GetElementType(), InnerObjects);
                }
                else
                {
                    result = null;
                    return false;
                }

                return true;
            }

            throw new InvalidCastException("Could not cast to type which is not implementing IAppSettings");
        }


        protected IDictionary<string, object> InnerData
        {
            get
            {
                if (_innerData != null)
                    return _innerData;
                throw new NullReferenceException("Dynamic model data wasn't initialized");
            }
        }

        protected IEnumerable<object> InnerObjects
        {
            get
            {
                if (_innerData != null)
                {
                    foreach (var entry in _innerData)
                    {
                        if (entry.Value is DynamicObject)
                        {
                            yield return entry.Value;
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
           var builder = new StringBuilder();

            foreach (var entry in _innerData)
            {
                string value = String.Empty;

                if (entry.Value is DynamicModel)
                {
                    value = Parser.SerializeToJString(entry.Value);
                }

                string part = $"{entry.Key}_{value}";
                builder.AppendLine(part);
            }

            return builder.ToString();
        }

        #region IDictionary
       
        public int Count => _innerData.Count;

        public bool IsReadOnly => _innerData.IsReadOnly;

        public ICollection<string> Keys => _innerData.Keys;

        public ICollection<object> Values => _innerData.Values;

        #endregion   
    }
}
