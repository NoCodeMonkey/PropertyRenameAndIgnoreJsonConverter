// See https://aka.ms/new-console-template for more information
using PropertyRenameAndIgnoreJsonConverter;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

JsonSerializerOptions jsonSerializerOptions1 = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
#if DEBUG
    WriteIndented = true,
#else
			WriteIndented = false,
#endif
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    ReferenceHandler = ReferenceHandler.IgnoreCycles,
    //By default, comments and trailing commas are not allowed in JSON
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString
}.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

Test1 nullTest = null;
Test1 test1 = new Test1();
Test1 test2 = new Test1()
{
    Id = 0
};
Test1 test3 = new Test1()
{
    Id = 1
};
Test1 test4 = new Test1()
{
    Id = 1,
    NullId = 32,
    IAmAString = "Sweet"
};
Test2 test5 = null;
Test2 test6 = new Test2();
Test2 test7 = new Test2()
{
    new Test1()
    {
        Id = 1,
        IAmAString = "Col"
    }
};

Console.WriteLine(JsonSerializer.Serialize(nullTest, jsonSerializerOptions1));
Console.WriteLine(JsonSerializer.Serialize(test1, jsonSerializerOptions1));
Console.WriteLine(JsonSerializer.Serialize(test2, jsonSerializerOptions1));
Console.WriteLine(JsonSerializer.Serialize(test3, jsonSerializerOptions1));
Console.WriteLine(JsonSerializer.Serialize(test4, jsonSerializerOptions1));
Console.WriteLine(JsonSerializer.Serialize(test5, jsonSerializerOptions1));
Console.WriteLine(JsonSerializer.Serialize(test6, jsonSerializerOptions1));
Console.WriteLine(JsonSerializer.Serialize(test7, jsonSerializerOptions1));

Console.WriteLine();

JsonSerializerOptions jsonSerializerOptions2 = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
#if DEBUG
    WriteIndented = true,
#else
			WriteIndented = false,
#endif
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    ReferenceHandler = ReferenceHandler.IgnoreCycles,
    //By default, comments and trailing commas are not allowed in JSON
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString
}.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

jsonSerializerOptions2.Converters.Add(new PropertyRenameAndIgnoreJsonConverter<Test1>());
Console.WriteLine(JsonSerializer.Serialize(nullTest, jsonSerializerOptions2));
Console.WriteLine(JsonSerializer.Serialize(test1, jsonSerializerOptions2));
Console.WriteLine(JsonSerializer.Serialize(test2, jsonSerializerOptions2));
Console.WriteLine(JsonSerializer.Serialize(test3, jsonSerializerOptions2));
Console.WriteLine(JsonSerializer.Serialize(test4, jsonSerializerOptions2));
Console.WriteLine(JsonSerializer.Serialize(test5, jsonSerializerOptions2));
Console.WriteLine(JsonSerializer.Serialize(test6, jsonSerializerOptions2));
Console.WriteLine(JsonSerializer.Serialize(test7, jsonSerializerOptions2));

Console.WriteLine();

JsonSerializerOptions jsonSerializerOptions3 = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
#if DEBUG
    WriteIndented = true,
#else
			WriteIndented = false,
#endif
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    ReferenceHandler = ReferenceHandler.IgnoreCycles,
    //By default, comments and trailing commas are not allowed in JSON
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString
}.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

PropertyRenameAndIgnoreJsonConverter<Test1> converter = new PropertyRenameAndIgnoreJsonConverter<Test1>();
converter.IgnoreProperties(nameof(Test1.Id));
jsonSerializerOptions3.Converters.Add(converter);
Console.WriteLine(JsonSerializer.Serialize(nullTest, jsonSerializerOptions3));
Console.WriteLine(JsonSerializer.Serialize(test1, jsonSerializerOptions3));
Console.WriteLine(JsonSerializer.Serialize(test2, jsonSerializerOptions3));
Console.WriteLine(JsonSerializer.Serialize(test3, jsonSerializerOptions3));
Console.WriteLine(JsonSerializer.Serialize(test4, jsonSerializerOptions3));
Console.WriteLine(JsonSerializer.Serialize(test5, jsonSerializerOptions3));
Console.WriteLine(JsonSerializer.Serialize(test6, jsonSerializerOptions3));
Console.WriteLine(JsonSerializer.Serialize(test7, jsonSerializerOptions3));

Console.WriteLine("Hello, World!");

public class Test1
{
    public int Id { get; set; }

    public int? NullId { get; set; }

    public string? IAmAString { get; set; }

    [JsonIgnore]
    public string PleaseIgnoreMe { get; set; } = "Crap";

    public DateTime Now { get; set; } = DateTime.Now;

    public Instant InstantNow { get; set; } = Instant.FromDateTimeUtc(DateTime.UtcNow);

}

public class Test2 : List<Test1>
{
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
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
    public void IgnoreProperties<T>(params Expression<Func<T, object>>[] expressions)
    {
        foreach (var expression in expressions)
        {
            var propertyInfo = GetPropertyInfo<T>(expression);
            propertiesToIgnore.Add(propertyInfo.Name);
        }
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
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                if (IsIgnored(property.PropertyInfo.Name))
                {
                    continue;
                }
                var jsonIgnoreAttribute = property.PropertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();
                if (jsonIgnoreAttribute is not null)
                {
                    jsonIgnoreCondition = jsonIgnoreAttribute.Condition;
                }
                else
                {
                    jsonIgnoreCondition = options.DefaultIgnoreCondition;
                }
                if (jsonIgnoreCondition == JsonIgnoreCondition.Always)
                {
                    continue;
                }
                string propertyName = property.PropertyInfo.Name;
                if (IsRenamed(propertyName, out string? newPropertyName))
                {
                    propertyName = newPropertyName ?? propertyName;
                }
                object? propertyValue = property.PropertyInfo.GetValue(value, null);
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
                    var test = Convert.GetTypeCode(propertyValue);
                    var test2 = IsDefault(propertyValue);
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
    /// Determines whether the specified property from type is ignored.
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
    /// Determines whether the specified type is renamed.
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
}

public static class New<T>
{
    public static readonly Func<T> Instance = Creator();

    static Func<T> Creator()
    {
        Type type = typeof(T);
        if (type == typeof(string))
        {
            return Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile();
        }
        if (type.HasDefaultConstructor())
        {
            return Expression.Lambda<Func<T>>(Expression.New(type)).Compile();
        }
        return () => (T)FormatterServices.GetUninitializedObject(type);
    }
}