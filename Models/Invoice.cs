using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeasy.Models
{
    /// <summary>
    /// Invoice Class with properties.
    /// </summary>
    [Table("Invoice")]
    public class Invoice
    {
        /// <summary>
        /// Invoice Class - Blank Constructor.
        /// </summary>
        public Invoice(){}

        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// CardPartial Getter and Setter.
        /// </summary>
        public int CardPartial{ get; set; }
        /// <summary>
        /// Price Getter and Setter.
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// InvoiceType Getter and Setter.
        /// </summary>
        public string InvoiceType { get; set; }
        /// <summary>
        /// InvoiceBody Getter and Setter.
        /// </summary>
        public string InvoiceBody { get; set; }
    }
}