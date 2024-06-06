using Data.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class AdminOverviewModel
    {
        public int TotalAccounts { get; set; }
        public int TotalTrips { get; set; }
        public int TotalSupportRequests { get; set; }
        public int TotalEmergencyRequests { get; set; }
        public List<int> AccountDetails { get; set; } = new List<int>();
        public List<double> TripStatistics { get; set; } = new List<double>();
        public List<int> SupportStatusDetails { get; set; } = new List<int>();
        public List<int> EmergencyStatusDetails { get; set; } = new List<int>();
    }

    public class AdminLineChartModel
    {
        public List<long> MonthlyIncome { get; set; } = new List<long>();
    }

    public class RegisterDriverByAdminModel
    {
        //Driver Profile
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public Gender Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public IFormFile? File { get; set; }

        //Driver Driving License
        public string DrivingLicenseNumber { get; set; }
        public string Type { get; set; }
        public DateOnly IssueDate { get; set; }
        public DateOnly DrivingLicenseExpiredDate { get; set; }

        //Driver IdentityCard
        public string Nationality { get; set; }
        public string PlaceOrigin { get; set; }
        public string PlaceResidence { get; set; }
        public string PersonalIdentification { get; set; }
        public string IdentityCardNumber { get; set; }
        public DateOnly IdentityCardExpiredDate { get; set; }

        //Linked Account
        public string AccountNumber { get; set; }
        public LinkedAccountType LinkedAccountTypeType { get; set; }
        public string Brand { get; set; }
        public string LinkedImgUrl { get; set; }
    }

    public class RegisterStaffByAdminModel
    {
        public string Name { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public Gender? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public IFormFile? File { get; set; }
    }
}
