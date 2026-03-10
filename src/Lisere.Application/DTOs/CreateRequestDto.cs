using System.ComponentModel.DataAnnotations;
using Lisere.Domain.Enums;

namespace Lisere.Application.DTOs;

public class CreateRequestDto
{
    [Required]
    public Guid SellerId { get; set; }

    [Required]
    public ZoneType Zone { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateRequestLineDto> Lines { get; set; } = new();
}
