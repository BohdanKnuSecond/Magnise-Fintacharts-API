namespace MagniseTest.Domain.Entities;

public class AssetPrice
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; } 

    public decimal Bid { get; set; } // sale price
    public decimal Ask { get; set; } // purchase price
    public decimal Last { get; set; } // last price

    public DateTime UpdatedAt { get; set; } 

    public Asset Asset { get; set; } = null!; 
}