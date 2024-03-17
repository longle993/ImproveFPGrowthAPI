using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CNDM.Models;

[Table("TanSuatMotSanPham")]
public partial class TanSuatMotSanPham
{
    public int? TanSuat { get; set; }

    [Key]
    public int ThuTu { get; set; }

    public string? IdSanPham { get; set; }
}
