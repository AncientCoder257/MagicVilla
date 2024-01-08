using System.ComponentModel.DataAnnotations;

namespace MagicVilla_VillaAPI.Models.Dto;

public class VillaDTO
{
    public int Id { get; set; }
    [Required]
    public string? Name { get; set; }

    public int? Occupancy { get; set; }
    public int? Sqm { get; set; }
}