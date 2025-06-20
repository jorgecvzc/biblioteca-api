using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Validaciones
{
    // Clase de validación por atributo para forzar que la primera letra del valor de cadena de un campo se mayúscula
    public class PrimeraLetraMayusculaAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var primeraLetra = value.ToString()![0].ToString();
            if (primeraLetra != primeraLetra.ToUpper())
            {
                return new ValidationResult("La primera letra tiene que ser mayúscula");
            }

            return ValidationResult.Success;
        }
    }
}
