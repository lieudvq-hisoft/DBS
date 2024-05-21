using Data.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models;
public class RatingCreateModel
{
    public Guid BookingId { get; set; }
    public int Star { get; set; }
    public string? Comment { get; set; }
    public IFormFile? File { get; set; }
}

public class RatingUpdateModel
{
    public int Star { get; set; }
    public string? Comment { get; set; }
    public IFormFile? File { get; set; }
}

public class RatingModel
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public BookingModel Booking { get; set; }
    public int Star { get; set; }
    public string Comment { get; set; }
    public string ImageUrl { get; set; }
    public DateTime DateCreated { get; set; }
}
