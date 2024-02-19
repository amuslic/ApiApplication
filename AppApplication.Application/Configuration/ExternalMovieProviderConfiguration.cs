namespace AppApplication.Application.Configuration
{
    public class ExternalMovieProviderConfiguration
    {
        public static readonly string ConfigurationKey = "ExternalMovieProvider";
        public string Url { get; set; }
        public string ApiKey { get; set; }
    }
}
