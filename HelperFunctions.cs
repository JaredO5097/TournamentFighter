using System.Reflection;

namespace TournamentFighter
{
    internal static class HelperFunctions
    {
        public static T GetAttributeFrom<T>(this object instance, string propertyName)
        {
            Type type = typeof(T);
            var property = instance.GetType().GetProperty(propertyName);
            return (T)property?.GetCustomAttributes(type, false).First();
        }

        public static double Sum(List<double[]> set, int index)
        {
            return set?[index]?.Sum() ?? double.NaN;
        }
    }
}
