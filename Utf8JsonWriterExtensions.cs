using GuardAgainstLib;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PropertyRenameAndIgnoreJsonConverter
{
    public static class Utf8JsonWriterExtensions
    {
        /// <summary>
        /// Writes an object while respecting <see cref="JsonSerializerOptions.IgnoreNullValues"/>.
        /// </summary>
        public static void WriteObject(this Utf8JsonWriter writer, string propertyName, object? value, JsonSerializerOptions options)
        {
            GuardAgainst.ArgumentBeingNull(writer, nameof(writer));
            GuardAgainst.ArgumentBeingNullOrWhitespace(propertyName, nameof(propertyName));
            GuardAgainst.ArgumentBeingNull(options, nameof(options));

            if (value is null && options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
            {
                return;
            }

            writer.WritePropertyName(propertyName, options);
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }

        /// <summary>
        /// Writes an object while respecting <see cref="JsonSerializerOptions.IgnoreNullValues"/>.
        /// </summary>
        public static void WriteObjectValue(this Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
        {
            GuardAgainst.ArgumentBeingNull(writer, nameof(writer));
            GuardAgainst.ArgumentBeingNull(options, nameof(options));

            if (value is null && options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
            {
                return;
            }

            if (value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }

        /// <summary>
        /// Writes the property name (as a JSON string) as the first part of a name/value pair
        /// of a JSON object. The output name will respect <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>.
        /// </summary>
        public static void WritePropertyName(this Utf8JsonWriter writer, string propertyName, JsonSerializerOptions options)
        {
            GuardAgainst.ArgumentBeingNull(writer, nameof(writer));
            GuardAgainst.ArgumentBeingNullOrWhitespace(propertyName, nameof(propertyName));
            GuardAgainst.ArgumentBeingNull(options, nameof(options));

            string serializedName = options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
            writer.WritePropertyName(serializedName);
        }
    }
}
