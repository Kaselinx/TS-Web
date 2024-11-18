using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TSL.Base.Platform.JsonConverters
{
    /// <summary>
    /// dictionary converter
    /// <see href="https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to#support-dictionary-with-non-string-key">HERE</see>
    /// </summary>
    public class DictionaryTKeyNonStringConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            if (typeToConvert.GetGenericTypeDefinition() != typeof(Dictionary<,>))
            {
                return false;
            }

            return typeToConvert.GetInterface("IDictionary") != null;
        }

        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(DictionaryNonStringConverterInner<,>).MakeGenericType(
                    new Type[] { keyType, valueType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);

            return converter;
        }

        private class DictionaryNonStringConverterInner<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>>
        {
            private readonly JsonConverter<TValue> _valueConverter;

            public DictionaryNonStringConverterInner(JsonSerializerOptions options)
            {
                // For performance, use the existing converter if available.
                _valueConverter = (JsonConverter<TValue>)options
                    .GetConverter(typeof(TValue));
            }

            public override Dictionary<TKey, TValue> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                throw new JsonException("not supported.");
            }

            public override void Write(
                Utf8JsonWriter writer,
                Dictionary<TKey, TValue> dictionary,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
                {
                    writer.WritePropertyName(kvp.Key.ToString());

                    if (_valueConverter != null)
                    {
                        _valueConverter.Write(writer, kvp.Value, options);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, kvp.Value, options);
                    }
                }

                writer.WriteEndObject();
            }
        }
    }
}