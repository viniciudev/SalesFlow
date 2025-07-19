namespace Model.Registrations
{
  public class File : BaseEntity
  {
    public byte[] Files { get; set; }
    public byte[] FileThumb { get; set; }
    public int IdDescriptionFiles { get; set; }
    public DescriptionFiles DescriptionFiles { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
  }
}
