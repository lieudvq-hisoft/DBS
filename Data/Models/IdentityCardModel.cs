using Data.Enums;
using Microsoft.AspNetCore.Http;

namespace Data.Models
{
    public class IdentityCardCreateModel
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

    public class IdentityCardUpdateModel
    {
        public string? FullName { get; set; }
        public DateOnly? Dob { get; set; }
        public Gender? Gender { get; set; }
        public string? Nationality { get; set; }
        public string? PlaceOrigin { get; set; }
        public string? PlaceResidence { get; set; }
        public string? PersonalIdentification { get; set; }
        public DateOnly? ExpiredDate { get; set; }
    }

    public class IdentityCardModel
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

    public class IdentityCardImageCreateModel
    {
        public Guid IdentityCardId { get; set; }
        //public string ImageData { get; set; }
        public IFormFile File { get; set; }
        public bool IsFront { get; set; }
    }

    public class IdentityCardImageUpdateModel
    {
        public string? ImageData { get; set; }
        public bool? IsFront { get; set; }
    }

    public class IdentityCardImageModel
    {
        public Guid IdentityCardId { get; set; }
        public string ImageData { get; set; }
        public bool IsFront { get; set; }
    }
}
