using System;
using System.Collections.Generic;

namespace MiracleShoes1.Model;

public partial class Size
{
    public int IdSize { get; set; }

    public double ShoeSize { get; set; }

    public virtual ICollection<AvailableSize> AvailableSizes { get; set; } = new List<AvailableSize>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
