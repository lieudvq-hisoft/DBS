using System;
using Data.Entities;

namespace Data.Models
{
    public class KafkaModel
    {
        public List<Guid> UserReceiveNotice { get; set; }
        public object Payload { get; set; }
    }
}

