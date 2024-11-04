using System.ComponentModel;
using System.Reflection;

namespace JsonToSql.CLI.Extensions;

internal static class EnumExtensions
{
    public static string GetDescription<T>(this T enumValue) where T : struct, System.Enum
    {
        Type objType = typeof(T);
        if (!objType.IsEnum)
            throw new ArgumentException("Argument must be of enum type", nameof(enumValue));
        var memberInfo = objType.GetMember(enumValue.ToString());
        return memberInfo?.FirstOrDefault()?
            .GetCustomAttribute<DescriptionAttribute>()?.Description ?? enumValue.ToString();
    }
}