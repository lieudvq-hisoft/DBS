using System.ComponentModel.DataAnnotations.Schema;


namespace Data.Entities;
public class IdentityCardImage : BaseEntity
{
    public Guid IdentityCardId { get; set; }
    [ForeignKey("IdentityCardId")]
    public virtual IdentityCard? IdentityCard { get; set; }
    public string ImageUrl { get; set; }
    public bool IsFront { get; set; }
}
