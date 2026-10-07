namespace Shortener.Application.Options;

public class AppOptions
{
    public const string SectionName = "App";

    public string BaseUrl { get; set; } = string.Empty;
}