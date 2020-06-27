using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceApi.Models
{
    public class ApplicationStepDetailTag
    {
        public int ApplicationStepDetailTagId { get; set; }
        public int ApplicationStepDetailId { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
        public ApplicationStepDetail ApplicationStepDetail { get; set; }
    }
 }
