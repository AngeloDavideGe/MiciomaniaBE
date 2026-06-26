namespace Library.Extensions
{
    public static class EnumExtensions
    {
        public static string GetNameEnum<T>(this T enumValue) where T : Enum
        {
            return enumValue.ToString();
        }
    }
}