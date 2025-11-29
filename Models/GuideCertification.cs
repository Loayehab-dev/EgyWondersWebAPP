using System;
using System.Collections.Generic;

namespace EgyWonders.Models;

public partial class GuideCertification
{
    public int CertificationId { get; set; }

    public string DocumentPath { get; set; } = null!;

    public int GuideId { get; set; }

    public string CertificationName { get; set; } = null!;

    public DateOnly IssueDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public virtual User Guide { get; set; } = null!;
}
