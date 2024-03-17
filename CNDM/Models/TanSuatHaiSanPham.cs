using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CNDM.Models;

[Table("TanSuatHaiSanPham")]
public partial class TanSuatHaiSanPham
{
    [StringLength(255)]
    public string IdHaiSanPham { get; set; } = null!;

    public string? TanSuat { get; set; }

    [Key]
    public int ThuTu { get; set; }
}
