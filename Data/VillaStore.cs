using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI.Data;

public class VillaStore
{
    public static List<VillaDTO> villaList = new List<VillaDTO>
    {
        new VillaDTO
        {
            Id = 1,
            Name = "Pool View",
            Sqm = 100,
            Occupancy = 4
        },
        new VillaDTO
        {
            Id = 2,
            Name = "Another View",
            Sqm = 200,
            Occupancy = 6
        }
    };
}