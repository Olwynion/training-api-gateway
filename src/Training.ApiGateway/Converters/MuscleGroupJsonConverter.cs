using System.Text.Json;
using System.Text.Json.Serialization;
using Training.Training.Proto;

namespace Training.ApiGateway.Converters;

public class MuscleGroupJsonConverter : JsonConverter<MuscleGroup>
{
    public override MuscleGroup Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return (MuscleGroup)reader.GetInt32();
        }
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (int.TryParse(s, out var intVal))
                return (MuscleGroup)intVal;
            if (Enum.TryParse<MuscleGroup>(s, ignoreCase: true, out var result))
                return result;
            throw new JsonException($"Cannot parse MuscleGroup from '{s}'");
        }
        throw new JsonException($"Unexpected token {reader.TokenType} for MuscleGroup");
    }

    public override void Write(Utf8JsonWriter writer, MuscleGroup value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}
