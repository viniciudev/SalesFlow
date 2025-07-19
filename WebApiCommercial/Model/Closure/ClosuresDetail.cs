namespace Model.Closure
{
  public class ClosuresDetail : BaseEntity
  {
    public string Description { get; set; }
    public decimal Value { get; set; }
    public int IdClosures { get; set; }
    public Closures Closures { get; set; }
  }
}
