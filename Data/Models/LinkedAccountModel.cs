using Data.Entities;
using Data.Enums;
using Data.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class LinkedAccountCreateModel
    {
        public string AccountNumber { get; set; }
        public LinkedAccountType Type { get; set; }
        public string Brand { get; set; }
        public string LinkedImgUrl { get; set; }
    }

    public class LinkedAccountModel
    {
        public Guid Id { get; set; }
        public UserModel User { get; set; }
        public string AccountNumber { get; set; }
        public LinkedAccountType Type { get; set; }
        public string Brand { get; set; }
        public string LinkedImgUrl { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
