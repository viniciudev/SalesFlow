namespace Model.Registrations
{
    public class DetailsService : BaseEntity
    {
        public string Description { get; set; }
        public int IdServiceProvided { get; set; }
        public ServiceProvided ServiceProvided { get; set; }
    }
}
