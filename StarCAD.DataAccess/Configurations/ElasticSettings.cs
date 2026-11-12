namespace StarCAD.DataAccess.Configurations;

public class ElasticSettings
{
    public string Endpoint { get; set; } = default!;
    public int MaxSearchResults { get; set; } = 1000;
}