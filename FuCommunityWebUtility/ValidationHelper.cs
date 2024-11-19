using System;
using System.ComponentModel.DataAnnotations;

namespace FuCommunityWebUtility
{
    public class ValidationHelper
    {
        public static ValidationResult ValidateDateOfBirth(DateTime? dateOfBirth, ValidationContext context)
        {
            if (dateOfBirth.HasValue)
            {
                if (dateOfBirth.Value > DateTime.Now)
                {
                    return new ValidationResult("Date of birth cannot be in the future.");
                }

                DateTime dateValid = new DateTime(1900, 1, 1);
                if (dateOfBirth.Value < dateValid)
                {
                    return new ValidationResult("Date of birth is invalid.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
