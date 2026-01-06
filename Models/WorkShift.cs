using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WorkFlowManager.Models;

    /// <summary>
    /// Represents a work shift assigned to an employee.
    /// </summary>
    public class WorkShift : IValidatableObject
    {
        public int Id { get; set; }
    
        [Required]
        public DateTime StartTime { get; set; }
    
        [Required]
        public DateTime EndTime { get; set; }
    
        /// <summary>
        /// Foreign key to the assigned employee.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;
    
        /// <summary>
        /// Navigation property to the assigned employee.
        /// </summary>
        public ApplicationUser? User { get; set; }
    
        /// <summary>
        /// Calculated duration of the shift in hours.
        /// </summary>
        public double DurationHours => (EndTime - StartTime).TotalHours;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "EndTime must be later than StartTime.",
                    new[] { nameof(EndTime), nameof(StartTime) });
            }
        }
    }
