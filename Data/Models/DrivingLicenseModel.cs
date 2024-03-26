﻿
using Microsoft.AspNetCore.Http;

namespace Data.Models
{
    public class DrivingLicenseCreateModel
    {
        public string Type { get; set; }
        public DateOnly IssueDate { get; set; }
        public DateOnly ExpriedDate { get; set; }
    }

    public class DrivingLicenseUpdateModel
    {
        public string? Type { get; set; }
        public DateOnly? IssueDate { get; set; }
        public DateOnly? ExpriedDate { get; set; }
    }

    public class DrivingLicenseModel
    {
        public Guid Id { get; set; }
        public Guid DriverId { get; set; }
        public string Type { get; set; }
        public DateOnly IssueDate { get; set; }
        public DateOnly ExpriedDate { get; set; }
    }

    public class DrivingLicenseImageCreateModel
    {
        public Guid DrivingLicenseId { get; set; }
        public bool IsFront { get; set; }
        public IFormFile File { get; set; }
    }

    public class DrivingLicenseImageUpdateModel
    {
        public IFormFile File { get; set; }
    }

    public class DrivingLicenseImageModel
    {
        public Guid Id { get; set; }
        public Guid DrivingLicenseId { get; set; }
        public bool IsFront { get; set; }
        public string ImageData { get; set; }
    }
}