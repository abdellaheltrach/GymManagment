namespace GymManagement.Domain.Bases
{
    public abstract class AuditableEntity : SoftDeletableEntity
    {
        public byte[]? RowVersion { get; set; }
    }
}
