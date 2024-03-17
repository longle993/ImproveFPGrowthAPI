using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CNDM.Models;

[Table("HoaDon")]
public partial class HoaDon
{
    [Key]
    [StringLength(255)]
    public string IdHoaDon { get; set; } = null!;

    public string? SanPham { get; set; }
}
