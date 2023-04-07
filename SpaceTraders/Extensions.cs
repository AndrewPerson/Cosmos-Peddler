using System.Linq;

namespace CosmosPeddler;

public static class StringExtensions
{
    public static string ToHuman(this string str)
    {
        return str.Split('_')
        .Where(s => s.Length > 0)
        .Select
        (
            s => s.Length == 1 ?
                    s.ToUpper() :
                    $"{char.ToUpper(s[0])}{s[1..].ToLower()}"
        )
        .Aggregate((a, b) => a + " " + b);
    }
}
