using System;
using System.Collections.Generic;

namespace MiracleShoes1.Model;

public partial class AvailableSize
{
    public int IdAvailableSize { get; set; }

    public int IdProduct { get; set; }

    public int IdSize { get; set; }

    public bool IsAvailable { get; set; }

    public int Amount { get; set; }

    public virtual Product IdProductNavigation { get; set; } = null!;

    public virtual Size IdSizeNavigation { get; set; } = null!;
}
