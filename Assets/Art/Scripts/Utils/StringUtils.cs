using System.Text.RegularExpressions;
using System.Globalization;

public static class StringUtils
{
    public static string PrettifyCamelCase(string input)
    {
        // Insert space before each capital (except the first)
        string spaced = Regex.Replace(input, "(?<!^)([A-Z])", " $1");

        // Capitalize each word (optional)
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(spaced);
    }
}
