using Data.Enums;
using Data.Model;
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
        public string IdentityCardNumber { get; set; }
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
        public string IdentityCardNumber { get; set; }
        public DateOnly? ExpiredDate { get; set; }
    }

    public class IdentityCardModel
    {
        public Guid Id { get; set; }
        public UserModel User { get; set; }
        public string FullName { get; set; }
        public DateOnly Dob { get; set; }
        public Gender Gender { get; set; }
        public string Nationality { get; set; }
        public string PlaceOrigin { get; set; }
        public string PlaceResidence { get; set; }
        public string PersonalIdentification { get; set; }
        public string IdentityCardNumber { get; set; }
        public DateOnly ExpiredDate { get; set; }
    }

    public class IdentityCardImageCreateModel
    {
        public Guid IdentityCardId { get; set; }
        public IFormFile File { get; set; }
        public bool IsFront { get; set; }
    }

    public class IdentityCardImageUpdateModel
    {
        public IFormFile File { get; set; }
    }

    public class IdentityCardImageModel
    {
        public Guid IdentityCardId { get; set; }
        public Guid Id { get; set; }
        public string ImageData { get; set; }
        public bool IsFront { get; set; }
    }
}
