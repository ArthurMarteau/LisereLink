using System.ComponentModel.DataAnnotations;

namespace Lisere.Application.DTOs;

public class RespondToAlternativesDto
{
    [Required, MinLength(1)]
    public List<RespondToAlternativeLineDto> Responses { get; set; } = new();
}

public class RespondToAlternativeLineDto
{
    [Required]
    public Guid AlternativeLineId { get; set; }

    [Required]
    public bool Accepted { get; set; }
}
