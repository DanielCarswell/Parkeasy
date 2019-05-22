using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeasy.Models
{
    /// <summary>
    /// Invoice Class with properties.
    /// </summary>
    [Table("Invoice")]
    public class SendInvoiceModel
    {
        /// <summary>
        /// Invoice Class - Blank Constructor.
        /// </summary>
        public SendInvoiceModel()
        {
            Emails = new List<string>();
        }

        /// <summary>
        /// InvoiceType Getter and Setter.
        /// </summary>
        [Display(Name = "Subject")]
        public string InvoiceType { get; set; }
        /// <summary>
        /// InvoiceBody Getter and Setter.
        /// </summary>
        [Display(Name = "Message")]
        public string InvoiceBody { get; set; }
        /// <summary>
        /// Email Getter and Setter.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Emails List Getter and Setter.
        /// </summary>
        public ICollection<string> Emails { get; set; }
    }
}