using System;

namespace Parkeasy.Models
{
    /// <summary>
    /// ErrorViewModel Class used for handling errors.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// RequestId Getter and Setter.
        /// </summary>
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}