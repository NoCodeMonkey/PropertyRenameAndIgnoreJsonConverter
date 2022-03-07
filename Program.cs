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
