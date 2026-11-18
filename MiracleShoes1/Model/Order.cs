using System;
using System.Collections.Generic;

namespace MiracleShoes1.Model;

public partial class Order
{
    public int IdOrder { get; set; }

    public int? IdUser { get; set; }

    public DateOnly DateOrder { get; set; }

    public decimal? TotalCost { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? IdUserNavigation { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
