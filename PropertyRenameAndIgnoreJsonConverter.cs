using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PropertyRenameAndIgnoreJsonConverter
{
    /// <summary>
    /// Property Rename And Ignore JsonConverter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Text.Json.Serialization.JsonConverter&lt;T&gt;" />
    public class PropertyRenameAndIgnoreJsonConverter<T> : JsonConverter<T>
        where T : class
    {
        /// <summary>
        /// The properties to ignore
        /// </summary>
        private readonly HashSet<string> propertiesToIgnore;

        /// <summary>
        /// The properties to rename
        /// </summary>
        private readonly Dictionary<string, string> propertiesToRename;

        /// <summary>
        /// The properties to remap
        /// </summary>
        private readonly Dictionary<string, Func<T, object>> propertiesToRemap;

        /// <summary>
        /// The properties to add
        /// </summary>
        private readonly List<Property> propertiesToAdd;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRenameAndIgnoreJsonConverter{T}"/> class.
        /// </summary>
        public PropertyRenameAndIgnoreJsonConverter()
        {
            propertiesToIgnore = new HashSet<string>();
            propertiesToRename = new Dictionary<string, string>();
            propertiesToRemap = new Dictionary<string, Func<T, object>>();
            propertiesToAdd = new List<Property>();
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        public void AddProperty(string propertyName, object propertyValue)
        {
            var property = new Property()
            {
                Name = propertyName,
                Value = propertyValue
            };
            propertiesToAdd.Add(property);
        }

        /// <summary>
        /// Ignore the given property/properties of the given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="jsonPropertyNames">The json property names.</param>
        public void IgnoreProperties(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                propertiesToIgnore.Add(propertyName);
            }
        }

        /// <summary>
        /// Ignore the given property/properties of the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions">The expressions.</param>
        public void IgnoreProperties(params Expression<Func<T, object>>[] expressions)
        {
            foreach (var expression in expressions)
            {
                var propertyInfo = GetPropertyInfo<T>(expression);
                propertiesToIgnore.Add(propertyInfo.Name);
            }
        }

        /// <summary>
        /// Rename a property of the given type.
        /// </summary>
        /// <param name="propertyName">The property name to rename.</param>
        /// <param name="newPropertyName">New name of the property.</param>
        public void RenameProperty(string propertyName, string newPropertyName)
        {
            propertiesToRename[propertyName] = newPropertyName;
        }

        /// <summary>
        /// Remaps the properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="expression">The expression.</param>
        public void RemapProperties(string propertyName, Func<T, object> projection)
        {
            propertiesToRemap[propertyName] = projection;
        }

        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        /// <param name="typeToConvert">The type to compare against.</param>
        /// <returns>
        ///   <see langword="true" /> if the type can be converted; otherwise, <see langword="false" />.
        /// </returns>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(T).IsAssignableFrom(typeToConvert);
        }

        /// <summary>
        /// Reads and converts the JSON to type <typeparamref name="T" />.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            Type declaringType = value.GetType();
            JsonIgnoreCondition jsonIgnoreCondition = options.DefaultIgnoreCondition;
            if (value is null)
            {
                if (options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
                {
                    return;
                }
                writer.WriteNullValue();
            }
            else if (jsonIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault && IsDefault(value))
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
            }
            else if (PropertyRenameAndIgnoreJsonConverter<T>.IsCollection(declaringType))
            {
                JsonSerializer.Serialize(writer, (object)value, options);
            }
            else
            {
                writer.WriteStartObject();
                var typeProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).Where(p => p.CanRead).Select(propertyInfo => new
                {
                    PropertyInfo = propertyInfo,
                    Type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType
                }).ToList();
                foreach (var property in typeProperties)
                {
                    var jsonIgnoreAttribute = property.PropertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();
                    jsonIgnoreCondition = jsonIgnoreAttribute?.Condition ?? options.DefaultIgnoreCondition;

                    if (jsonIgnoreCondition == JsonIgnoreCondition.Always || IsIgnored(property.PropertyInfo.Name))
                    {
                        continue;
                    }

                    string propertyName = property.PropertyInfo.Name;
                    object? propertyValue = property.PropertyInfo.GetValue(value, null);

                    if (IsRemapped(propertyName, out Func<T, object>? projection))
                    {
                        propertyValue = projection!(value);
                    }

                    if (IsRenamed(propertyName, out string? newPropertyName))
                    {
                        propertyName = newPropertyName!;
                    }

                    if (propertyValue is null)
                    {
                        if (jsonIgnoreCondition == JsonIgnoreCondition.WhenWritingNull || jsonIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault)
                        {
                            continue;
                        }
                        writer.WritePropertyName(propertyName, options);
                        writer.WriteNullValue();
                    }
                    else if (jsonIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault && IsDefault(propertyValue))
                    {
                        continue;
                    }
                    else
                    {
                        writer.WritePropertyName(propertyName, options);
                        JsonSerializer.Serialize(writer, propertyValue, options);
                    }
                }
                foreach (var property in propertiesToAdd)
                {
                    writer.WritePropertyName(property.Name, options);
                    JsonSerializer.Serialize(writer, property.Value, options);
                }
                writer.WriteEndObject();
            }
        }

        /// <summary>
        /// Determines whether the specified property is ignored.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        ///   <c>true</c> if the specified property from type is ignored; otherwise, <c>false</c>.
        /// </returns>
        private bool IsIgnored(string propertyName)
        {
            return propertiesToIgnore.Contains(propertyName);
        }

        /// <summary>
        /// Determines whether the specified property is renamed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="newPropertyName">New name of the property.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is renamed; otherwise, <c>false</c>.
        /// </returns>
        private bool IsRenamed(string propertyName, out string? newPropertyName)
        {
            if (!propertiesToRename.TryGetValue(propertyName, out newPropertyName))
            {
                newPropertyName = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified property is remapped.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="projection">The projection.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is renamed; otherwise, <c>false</c>.
        /// </returns>
        private bool IsRemapped(string propertyName, out Func<T, object>? projection)
        {
            if (!propertiesToRemap.TryGetValue(propertyName, out projection))
            {
                projection = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified type is collection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is collection; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCollection(Type type)
        {
            return new Type[] { typeof(IEnumerable<>), typeof(ICollection<>), typeof(IList<>), typeof(List<>) }.Any(t => t.Name == type.Name) || type.IsArray;
        }

        /// <summary>
        /// Determines whether the specified value is default.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is default; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDefault<TSource>(TSource value)
        {
            if (value is null)
            {
                return true;
            }
            Type type = value.GetType();
            if (type.IsValueType)
            {
                return (Equals(value, Activator.CreateInstance(type)));
            }
            else
            {
                return JsonSerializer.Serialize(value) == JsonSerializer.Serialize(New<TSource>.Instance());
            }
        }

        /// <summary>
        /// Gets the property information.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="propertyLambda">The property lambda.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static PropertyInfo GetPropertyInfo<TSource>(Expression<Func<TSource, object>> expression)
        {
            Type type = typeof(TSource);

            var member = expression.Body is not UnaryExpression unary ? expression.Body as MemberExpression : unary.Operand as MemberExpression;
            if (member is null)
            {
                throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", expression.ToString()));
            }
            if (member.Member is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", expression.ToString()));
            }
            if (type != propertyInfo.ReflectedType && propertyInfo.ReflectedType != null && !type.IsSubclassOf(propertyInfo.ReflectedType))
            {
                throw new ArgumentException(string.Format("Expression '{0}' refers to a property that is not from type {1}.", expression.ToString(), type));
            }

            return propertyInfo;
        }

        /// <summary>
        /// Track Properties
        /// </summary>
        private class Property
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; } = null!;

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public object Value { get; set; } = null!;
        }

        /// <summary>
        /// Fast creation of objects instead of Activator.CreateInstance(type).
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <seealso cref="System.Text.Json.Serialization.JsonConverter&lt;T&gt;" />
        /// <remarks>
        /// https://stackoverflow.com/questions/6582259/fast-creation-of-objects-instead-of-activator-createinstancetype
        /// </remarks>
        private static class New<TSource>
        {
            public static readonly Func<TSource> Instance = Creator();

            static Func<TSource> Creator()
            {
                Type type = typeof(TSource);
                if (type == typeof(string))
                {
                    return Expression.Lambda<Func<TSource>>(Expression.Constant(string.Empty)).Compile();
                }
                if (type.HasDefaultConstructor())
                {
                    return Expression.Lambda<Func<TSource>>(Expression.New(type)).Compile();
                }
                return () => (TSource)FormatterServices.GetUninitializedObject(type);
            }
        }
    }
}
