using Data.Enums;

namespace Data.Entities;
public class IdentityCard : BaseEntity
{
    public string FullName { get; set; }
    public DateOnly Dob { get; set; }
    public Gender Gender { get; set; }
    public string Nationality { get; set; }
    public string PlaceOrigin { get; set; }
    public string PlaceResidence { get; set; }
    public string PersonalIdentification { get; set; }
    public DateOnly ExpiredDate { get; set; }
}
