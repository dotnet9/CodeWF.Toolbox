namespace CodeWF.Utils;

public static class EnumExtensions
{
    public static string Description(this Enum value)
    {
        Type enumType = value.GetType();

        // 1、检查是否是位域枚举的组合值  
        bool isFlagsEnum = enumType.GetCustomAttribute<FlagsAttribute>() != null;

        // 2、非位域枚举直接返回描述
        if (!isFlagsEnum)
        {
            return GetDescription(value);
        }

        // 3、位域枚举获取每个标志的描述并用逗号分隔  
        List<string> descriptions = new();
        foreach (Enum enumValue in Enum.GetValues(enumType))
        {
            // 跳过值为0的枚举成员，因为任何数与0进行“或”运算都不会改变该数的值
            if (Convert.ToInt64(enumValue) == 0)
            {
                continue;
            }

            if (value.HasFlag(enumValue))
            {
                descriptions.Add(GetDescription(enumValue));
            }
        }

        return descriptions.Count <= 0 ? GetDescription(value) : string.Join(",", descriptions);
    }

    private static string GetDescription(Enum value)
    {
        FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());
        DescriptionAttribute? attribute =
            Attribute.GetCustomAttribute(fieldInfo!, typeof(DescriptionAttribute)) as DescriptionAttribute;
        return attribute?.Description ?? value.ToString();
    }
}