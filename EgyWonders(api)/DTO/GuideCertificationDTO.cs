using System;
using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{

    public class GuideCertificationDto : GuideCertificationBaseDto
    {
        public int CertificationId { get; set; } // Primary Key
        public string DocumentPath { get; set; } = null!;
    }
}