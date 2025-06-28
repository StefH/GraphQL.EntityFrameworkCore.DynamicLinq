using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;

[ExcludeFromCodeCoverage]
public class Room
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int Number { get; set; }

    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    public RoomStatus Status { get; set; }

    public bool AllowedSmoking { get; set; }

    [ForeignKey("RoomDetailId")]
    public RoomDetail RoomDetail { get; set; }

    public int? RoomDetailId { get; set; }

    [ForeignKey("RoomDetailId")]
    public Building Building { get; set; }

    public int? BuildingId { get; set; }
}