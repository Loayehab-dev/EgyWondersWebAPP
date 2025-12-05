using System;
using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models    
{

    public class GuideCertificationDto : GuideCertificationBaseDto
    {
        public int CertificationId { get; set; } // Primary Key
        public string DocumentPath { get; set; } = null!;
    }
}