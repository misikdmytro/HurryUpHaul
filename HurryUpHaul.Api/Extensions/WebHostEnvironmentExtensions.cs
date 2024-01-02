namespace HurryUpHaul.Api.Extensions
{
    public static class WebHostEnvironmentExtensions
    {
        public static bool IsDocker(this IWebHostEnvironment env)
        {
            return env.IsEnvironment("Docker");
        }
    }
}