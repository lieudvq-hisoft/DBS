using System;
using Data.Entities;

namespace Data.Models
{
    public class ExternalAuthModel
    {
        public string? Provider { get; set; }
        public string? IdToken { get; set; }
    }
}

