using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeasy.Models
{
    /// <summary>
    /// Payment Class with Properties.
    /// </summary>
    [Table("Payment")]
    public class Payment
    {
        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// CardNumber Getter and Setter.
        /// </summary>
        /// <value>**** **** **** ****</value>
        public int CardNumber { get; set; }
        /// <summary>
        /// CardType Getter and Setter.
        /// </summary>
        /// <value>Visa/Mastercard</value>
        public string CardType { get; set; }
        /// <summary>
        /// Amounter Getter and Setter.
        /// </summary>
        /// <value></value>
        public double Amount { get; set; }
        /// <summary>
        /// ExpiryDate Getter and Setter.
        /// </summary>
        /// <value>**/08/21</value>
        public DateTime ExpiryDate { get; set; }
        /// <summary>
        /// SecurityNumber Getter and Setter.
        /// </summary>
        /// <value>***</value>
        public int SecurityNumber { get; set; }
        /// <summary>
        /// DatePaid Getter and Setter.
        /// </summary>
        /// <value>20/05/2019 12:54:09</value>
        public DateTime DatePaid { get; set; }
    }
}