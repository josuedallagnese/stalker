using Dawn;

namespace Microsoft.Extensions.Configuration
{
    public static class IConfigurationExtensions
    {
        public static T CreateAndConfigureWith<T>(this IConfiguration configuration, string key)
        {
            Guard.Argument(configuration, nameof(configuration)).NotNull();
            Guard.Argument(key, nameof(key)).NotNull().NotEmpty().NotWhiteSpace();

            return configuration.ConfigureWith(key, Activator.CreateInstance<T>());
        }

        public static T ConfigureWith<T>(this IConfiguration configuration, string key, T item)
        {
            Guard.Argument(configuration, nameof(configuration)).NotNull();
            Guard.Argument(key, nameof(key)).NotNull().NotEmpty().NotWhiteSpace();

            configuration.Bind(key, item);

            return item;
        }
    }
}
