namespace Backend
{
    public class PropertyValue
    {
        public static T GetPropertyValue<T>(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return (T)property.GetValue(obj);
        }

    }
}
