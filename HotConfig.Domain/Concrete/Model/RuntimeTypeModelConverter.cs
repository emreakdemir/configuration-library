using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using HotConfig.Domain.Abstract.Base;

namespace HotConfig.Domain.Concrete.Model
{
    /// <summary>
    /// Serializes an <see cref="IModel"/> using its runtime type (as the previous serializer did),
    /// instead of the declared interface type, which would drop all derived properties.
    /// </summary>
    internal sealed class RuntimeTypeModelConverter : JsonConverter<IModel>
    {
        public override IModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"Cannot deserialize the abstraction {typeToConvert}. Use a concrete cache model type.");
        }

        public override void Write(Utf8JsonWriter writer, IModel value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
