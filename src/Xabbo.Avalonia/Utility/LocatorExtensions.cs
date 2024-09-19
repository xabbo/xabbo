using Splat;

internal static class LocatorExtensions
{
    public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver)
    {
        return resolver.GetService<T>() ??
            throw new System.Exception($"Failed to resolve service {typeof(T).FullName}.");
    }
}
