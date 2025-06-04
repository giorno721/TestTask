namespace TestTask.Entities;

public class TvProducts
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Product { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
