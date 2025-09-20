using System.Collections;
using System.Text.Json;
using Microsoft.Extensions.Primitives;

namespace RbacApi.Infrastructure.Services;

public class AuditSanitizer
{

    public static object? ToSerializable(object? value)
    {
        if (value == null) return null;

        // primitivas comunes
        if (value is string s) return s;
        if (value is bool b) return b;
        if (value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal) return value;
        if (value is DateTime dt) return dt.ToString("o");
        if (value is Guid g) return g.ToString();

        // PathString / QueryString -> string
        if (value is PathString ps) return ps.Value;
        if (value is QueryString qs) return qs.Value;

        // StringValues (headers/query/form values)
        if (value is StringValues sv)
        {
            if (sv.Count == 1) return sv.ToString();
            return sv.ToArray();
        }

        // IHeaderDictionary / IQueryCollection / IFormCollection => Dictionary<string, object?>
        if (value is IHeaderDictionary headers)
            return headers.ToDictionary(k => k.Key, v => (object?)ToSerializable(v.Value));

        if (value is IQueryCollection query)
            return query.ToDictionary(k => k.Key, v => (object?)ToSerializable(v.Value));

        if (value is IFormCollection form)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var kv in form) dict[kv.Key] = ToSerializable(kv.Value);
            return dict;
        }

        // IDictionary (generic)
        if (value is IDictionary<string, object> dictStringObj)
        {
            var outd = new Dictionary<string, object?>();
            foreach (var kv in dictStringObj) outd[kv.Key] = ToSerializable(kv.Value);
            return outd;
        }

        if (value is IDictionary dictionary)
        {
            var outd = new Dictionary<string, object?>();
            foreach (DictionaryEntry kv in dictionary)
            {
                var key = kv.Key?.ToString() ?? "null";
                outd[key] = ToSerializable(kv.Value);
            }
            return outd;
        }

        // IEnumerable<KeyValuePair<string, StringValues>> common in some contexts
        if (value is IEnumerable<KeyValuePair<string, StringValues>> kvps)
        {
            var outd = new Dictionary<string, object?>();
            foreach (var kv in kvps) outd[kv.Key] = ToSerializable(kv.Value);
            return outd;
        }

        // IEnumerable (arrays/lists) -> List<object?>
        if (value is IEnumerable enumerable && !(value is string))
        {
            var list = new List<object?>();
            foreach (var item in enumerable) list.Add(ToSerializable(item));
            return list;
        }

        // JsonDocument / JsonElement -> convertir a objeto simple
        if (value is JsonDocument jd)
            return JsonElementToSerializable(jd.RootElement);

        if (value is JsonElement je)
            return JsonElementToSerializable(je);

        // Fallback: intenta ToString() pero limitando tamaño
        var text = value.ToString();
        if (text == null) return null;
        if (text.Length > 2000) return text.Substring(0, 2000) + "...(truncated)";
        return text;
    }


    private static object? JsonElementToSerializable(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                var d = new Dictionary<string, object?>();
                foreach (var p in el.EnumerateObject())
                    d[p.Name] = JsonElementToSerializable(p.Value);
                return d;
            case JsonValueKind.Array:
                var list = new List<object?>();
                foreach (var item in el.EnumerateArray())
                    list.Add(JsonElementToSerializable(item));
                return list;
            case JsonValueKind.String:
                return el.GetString();
            case JsonValueKind.Number:
                if (el.TryGetInt64(out var l)) return l;
                if (el.TryGetDouble(out var dnum)) return dnum;
                return el.GetRawText();
            case JsonValueKind.True:
            case JsonValueKind.False:
                return el.GetBoolean();
            case JsonValueKind.Null:
            default:
                return null;
        }
    }
}
