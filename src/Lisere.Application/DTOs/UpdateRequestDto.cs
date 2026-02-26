using System.ComponentModel.DataAnnotations;
using Lisere.Domain.Enums;

namespace Lisere.Application.DTOs;

public class UpdateRequestDto
{
    [Required]
    public RequestStatus Status { get; set; }

    public Guid? StockistId { get; set; }
}
