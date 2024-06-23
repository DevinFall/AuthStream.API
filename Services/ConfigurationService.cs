namespace AuthStream.API.Services;

public class ConfigurationService
{
    private readonly IConfiguration _configuration;
    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetSection(string section)
    {
        var sectionValue = _configuration.GetSection(section).Value;

        if (sectionValue is null)
        {
            throw new KeyNotFoundException($"Configuration for appsetting '{section}' not found.");
        }

        return sectionValue;
    }
}