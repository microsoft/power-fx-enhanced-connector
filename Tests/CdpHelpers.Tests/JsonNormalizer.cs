﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Microsoft.PowerFx.Tests
{
    // From: https://github.com/microsoft/PowerApps-Language-Tooling/blob/master/src/PAModel/Utility/JsonNormalizer.cs
    // Write out Json in a normalized sorted order. 
    // Orders properties, whitespace/indenting, etc. 

    /// <summary>
    /// Provides utilities for serializing and normalizing JSON in a consistent, sorted, and indented format.
    /// </summary>
    public class JsonNormalizer
    {
        /// <summary>
        /// Serializes an object to JSON and normalizes the output for consistent formatting and property ordering.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A normalized JSON string.</returns>
        public static string Serialize<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return Normalize(json);
        }

        /// <summary>
        /// Normalizes a JSON string for consistent formatting and property ordering.
        /// </summary>
        /// <param name="jsonStr">The JSON string to normalize.</param>
        /// <returns>A normalized JSON string.</returns>
        public static string Normalize(string jsonStr)
        {
            var je = JsonDocument.Parse(jsonStr).RootElement;
            return Normalize(je);
        }

        /// <summary>
        /// Normalizes a <see cref="JsonElement"/> for consistent formatting and property ordering.
        /// </summary>
        /// <param name="je">The <see cref="JsonElement"/> to normalize.</param>
        /// <returns>A normalized JSON string.</returns>
        public static string Normalize(JsonElement je)
        {
            var ms = new MemoryStream();
            var opts = new JsonWriterOptions
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            using (var writer = new Utf8JsonWriter(ms, opts))
            {
                Write(je, writer);
            }

            var bytes = ms.ToArray();
            var str = Encoding.UTF8.GetString(bytes);
            return str;
        }

        /// <summary>
        /// Writes a <see cref="JsonElement"/> to a <see cref="Utf8JsonWriter"/> in a normalized, sorted order.
        /// </summary>
        /// <param name="je">The <see cref="JsonElement"/> to write.</param>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
        private static void Write(JsonElement je, Utf8JsonWriter writer)
        {
            switch (je.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();

                    foreach (var x in je.EnumerateObject().OrderBy(prop => prop.Name))
                    {
                        // Skip nulls. 
                        if (x.Value.ValueKind == JsonValueKind.Null)
                        {
                            continue;
                        }

                        writer.WritePropertyName(x.Name);
                        Write(x.Value, writer);
                    }

                    writer.WriteEndObject();
                    break;

                // When normalizing... original msapp arrays can be in any order...
                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var x in je.EnumerateArray())
                    {
                        Write(x, writer);
                    }

                    writer.WriteEndArray();
                    break;

                case JsonValueKind.Number:
                    writer.WriteNumberValue(je.GetDouble());
                    break;

                case JsonValueKind.String:
                    // Escape the string 
                    writer.WriteStringValue(je.GetString());
                    break;

                case JsonValueKind.Null:
                    writer.WriteNullValue();
                    break;

                case JsonValueKind.True:
                    writer.WriteBooleanValue(true);
                    break;

                case JsonValueKind.False:
                    writer.WriteBooleanValue(false);
                    break;

                default:
                    throw new NotImplementedException($"Kind: {je.ValueKind}");
            }
        }
    }
}
