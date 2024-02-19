namespace ApiApplication.Application.Configuration
{
    public record ExternalMovieProviderConfiguration
    {
        public static readonly string ConfigurationKey = "ExternalMovieProvider";
        public string Url { get; init; }
        public string ApiKey { get; init; }
    }
}
