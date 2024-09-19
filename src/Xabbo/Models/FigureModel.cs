namespace Xabbo.Models;

public class FigureModel
{
    public int Id { get; set; }
    public bool IsOrigins { get; set; }
    public string FigureString { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public int Order { get; set; }
}
