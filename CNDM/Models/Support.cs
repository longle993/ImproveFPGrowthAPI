using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CNDM.Models;

[Table("Support")]
public partial class Support
{
    [Key]
    [Column("SMin")]
    [StringLength(255)]
    public string Smin { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PhanTram { get; set; }
}
