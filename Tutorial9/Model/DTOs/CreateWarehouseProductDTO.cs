﻿using System.ComponentModel.DataAnnotations;

namespace Tutorial9.Model.DTOs;

public class CreateWarehouseProductDTO
{
    [Required] 
    public int IdProduct { get; set; }
    [Required]
    public int IdWarehouse { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public int Amount { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
}