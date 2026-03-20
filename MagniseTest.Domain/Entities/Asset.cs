namespace MagniseTest.Domain.Entities;

public class Asset
{
    public Guid Id { get; set; } 
    public string Provider { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty; 
    public string Symbol { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty;

    
    public ICollection<AssetPrice> Prices { get; set; } = new List<AssetPrice>();
}
