namespace DataCollectors.ClientLibrary.Services.External.Configuration.Responses;

public record SettingDto
{
    public string Key { get; set; } = default!;

    public string Value { get; set; } = default!;
}
