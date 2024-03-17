using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CNDM.Models;

[Table("SanPham")]
public partial class SanPham
{
    [Key]
    [StringLength(255)]
    public string IdSanPham { get; set; } = null!;

    public string? TenSanPham { get; set; }
}
