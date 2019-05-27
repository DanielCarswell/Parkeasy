using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeasy.Models
{
    /// <summary>
    /// ReportViewModel class.
    /// </summary>
    public class ReportViewModel
    {
        public ReportViewModel(){}
        /// <summary>
        /// Report Getter and Setter.
        /// </summary>
        public string Report { get; set; }
    }
}