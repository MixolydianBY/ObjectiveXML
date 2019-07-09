namespace ObjectiveXML.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using ImpromptuInterface;

    public static class CustomActivator
    {
        public static TObject CreateInstanceAndPopulate<TObject>(IDictionary<string, object> propertiesValues)
            where TObject : class
        {
            Type desiredType = typeof(TObject);

            if (desiredType.IsInterface)
            {
                // The "Activator.CreateInstance<>" from the .NET Framework can't instantiate interfaces
                // To do this we need to create an object (in runtime) that implements this interface
                return CreateInterfaceInstance<TObject>(propertiesValues);
            }
            else
            {
                var objectCreated = Activator.CreateInstance(typeof(TObject));
                PopulateObjectProperties(objectCreated, propertiesValues);
                return (TObject)objectCreated;
            }
        }     

        public static object CreateInstanceAndPopulate(Type type, IDictionary<string, object> propertiesValues)
        {
            return ((Func<IDictionary<string, object>, object>)CreateInstanceAndPopulate<object>).Method
                                                                .GetGenericMethodDefinition()
                                                                .MakeGenericMethod(type)
                                                                .Invoke(null, new object[] { propertiesValues });
        }

        public static TObject CreateInstance<TObject>()
            where TObject : class
        {
            return CreateInstanceAndPopulate<TObject>(null);
        }

        public static object CreateGenericInstance(Type genType, params Type[] constraints)
        {
            Type[] typeArgs = constraints;
            Type result = genType.MakeGenericType(typeArgs);
            return CreateInstance(result);
        }

        public static TObject CreateDynamicAndMerge<TObject>(IDictionary<string, object> propertiesValues)
            where TObject : class
        {
            if (typeof(DynamicObject).IsAssignableFrom(typeof(TObject)) 
                && !typeof(TObject).IsAbstract)
            {
                dynamic obj = Activator.CreateInstance<TObject>();
                IDictionary<string, object> asDictionary;

                try
                {
                    asDictionary = obj;
                    propertiesValues.ToList().ForEach(x => asDictionary[x.Key] = x.Value);
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidOperationException("Cant cast dynamic object to IDictionary," +
                                                        "consider implementing TryConvert explicitly", e);
                }

                return obj;
            }

            return CreateInstanceAndPopulate<TObject>(propertiesValues);
        }

        public static object CreateInstance(Type type)
        {
            return ((Func<object>)CreateInstance<object>).Method
                                                   .GetGenericMethodDefinition()
                                                   .MakeGenericMethod(type)
                                                   .Invoke(null, null);
        }

        public static void PopulateObjectProperties(object obj, IDictionary<string, object> propertiesValues)
        {
            PopulatePropertiesOfObjectWithValuesFromDictionary(obj, propertiesValues);
        }

        private static TInterface CreateInterfaceInstance<TInterface>(
            IDictionary<string, object> propertiesValues = null)
            where TInterface : class
        {
            dynamic expandoObject = new ExpandoObject();
            Type expandoType = typeof(TInterface);

            var expandoProperties = expandoObject as IDictionary<string, object>;

            foreach (var member in expandoType.GetMembers())
            {
                // Get value from dictionary if it exists
                if (propertiesValues != null && propertiesValues.ContainsKey(member.Name))
                {
                    var parameterValue = propertiesValues[member.Name];
                    object convertedValue;

                    if (typeof(DynamicObject).IsAssignableFrom(parameterValue.GetType()))
                    {
                        convertedValue = CastDynamicValueToPropertyType(member.GetUnderlyingType(), parameterValue);
                    }
                    else
                    {
                        convertedValue = CastValueToPropertyType(member, parameterValue);
                    }
                    expandoProperties[member.Name] = convertedValue;
                }

                // If not, get the default value
                else
                {
                    var propertyType = member.GetUnderlyingType();

                    if(propertyType != typeof(void))
                    {
                        object defaultValue = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
                        expandoProperties[member.Name] = defaultValue;
                    }                
                }
            }

            return Impromptu.ActLike(expandoObject);
        }

        private static void PopulatePropertiesOfObjectWithValuesFromDictionary(object obj, IDictionary<string, object> propertiesValues = null)
        {
            if (propertiesValues is null)
            {
                return;              
            }

            var objectType = obj.GetType();


            foreach (var property in objectType.GetPublicProperties())
            {
                if (propertiesValues.ContainsKey(property.Name))
                {
                    object parameterValue = propertiesValues[property.Name];
                    object convertedValue;

                    if (typeof(DynamicObject).IsAssignableFrom(parameterValue.GetType()))
                    {
                        convertedValue = CastDynamicValueToPropertyType(property.GetUnderlyingType(), parameterValue);
                    }
                    else
                    {
                        convertedValue = CastValueToPropertyType(property, parameterValue);
                    }
   
                    property.SetValue(obj, convertedValue);
                }
            }
        }

        public static object CastValueToPropertyType(MemberInfo propertyToBeSetted, object value)
        {
            Type propertyType = Nullable.GetUnderlyingType(propertyToBeSetted.GetUnderlyingType()) ?? propertyToBeSetted.GetUnderlyingType();
            object safeValue = (value == null) ? null : propertyType.IsAssignableFrom(value.GetType())
                                                      ? value
                                                      : Convert.ChangeType(value, propertyType);

            return safeValue;
        }

        private static object CastDynamicValueToPropertyType<T>(dynamic value)
        {
            T preConvertionValue = value;

            Type underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            object safeValue = (value == null) ? null : typeof(T).IsAssignableFrom(preConvertionValue.GetType())
                                                      ? preConvertionValue
                                                      : Convert.ChangeType(preConvertionValue, underlyingType);

            return safeValue;
        }


        private static object CastDynamicValueToPropertyType(Type propertyType, object value)
        {
            return ((Func<dynamic, object>)CastDynamicValueToPropertyType<object>).Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(propertyType)
                .Invoke(null, new object[] { value });
        }
    }
}
