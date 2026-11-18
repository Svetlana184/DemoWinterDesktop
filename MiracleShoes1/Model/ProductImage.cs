using System;
using System.Collections.Generic;

namespace MiracleShoes1.Model;

public partial class ProductImage
{
    public int IdProductImage { get; set; }

    public int IdProduct { get; set; }

    public string? FileName { get; set; }

    public string ContentType { get; set; } = null!;

    public byte[]? Data { get; set; }

    public string? Url { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product IdProductNavigation { get; set; } = null!;
}
