using System.ComponentModel.DataAnnotations;

namespace MagicVilla_VillaAPI.Models.Dto;

public class VillaDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Details { get; set; }
    public double Rate { get; set; }
    public int Sqm { get; set; }
    public int Occupancy { get; set; }
    public string? ImageUrl { get; set; }
    public string? Amenity { get; set; }
}