using System.Xml.Linq;

namespace Karls.GithubUtility.Core.Extensions;

public static class XDocumentExtensions {
    public static string? FirstValueOrDefault(this IEnumerable<XElement> enumerable, Func<XElement, bool> predicate) {
        var first = enumerable.FirstOrDefault(predicate);
        return first?.Value;
    }

    public static string? FirstValueOrDefault(this IEnumerable<XElement> enumerable) {
        var first = enumerable.FirstOrDefault();
        return first?.Value;
    }

    public static string? FirstValueOrDefault(this IEnumerable<XAttribute> enumerable, Func<XAttribute, bool> predicate) {
        var first = enumerable.FirstOrDefault(predicate);
        return first?.Value;
    }

    public static string? FirstValueOrDefault(this IEnumerable<XAttribute> enumerable) {
        var first = enumerable.FirstOrDefault();
        return first?.Value;
    }

    public static T? ValueOrDefault<T>(this XAttribute? attribute) {
        var value = attribute?.Value;

        if(value == null) {
            return default;
        }

        var convertTo = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        try {
            return (T)Convert.ChangeType(value, convertTo);
        } catch(InvalidCastException) {
            return default;
        } catch(FormatException) {
            return default;
        }
    }

    public static T? ValueOrDefault<T>(this XElement? attribute) {
        var value = attribute?.Value;

        if(value == null) {
            return default;
        }

        var convertTo = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        try {
            return (T)Convert.ChangeType(value, convertTo);
        } catch(InvalidCastException) {
            return default;
        } catch(FormatException) {
            return default;
        }
    }
}
