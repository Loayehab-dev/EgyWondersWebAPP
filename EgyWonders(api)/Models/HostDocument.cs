using System;
using System.Collections.Generic;

namespace EgyWonders.Models;

public partial class HostDocument
{
    public int DocumentId { get; set; }

    public string DocumentPath { get; set; } = null!;

    public string? TextRecord { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? NationalId { get; set; }

    public bool Verified { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
