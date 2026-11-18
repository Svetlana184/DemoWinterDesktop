using System;
using System.Collections.Generic;

namespace MiracleShoes1.Model;

public partial class OrderItem
{
    public int IdOrderItem { get; set; }

    public int IdOrder { get; set; }

    public int IdProduct { get; set; }

    public int? IdSize { get; set; }

    public int Amount { get; set; }

    public decimal CostForItem { get; set; }

    public decimal? Cost { get; set; }

    public virtual Order IdOrderNavigation { get; set; } = null!;

    public virtual Product IdProductNavigation { get; set; } = null!;

    public virtual Size? IdSizeNavigation { get; set; }
}
