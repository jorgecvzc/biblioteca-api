﻿using BibliotecaAPI.Validaciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.PruebasUnitarias.Validaciones
{
    [TestClass]
    public class PrimeraLetraMayusculaAttributePruebas
    {
        [TestMethod]
        [DataRow("")]
        [DataRow("  ")]
        [DataRow(null)]
        [DataRow("Felipe")]
        public void IsValid_RetornaExistoso_SiValueNoTieneLaPrimeraLetraMinuscula0(
            string value)
        {
            // Preparación

            var primeraLetraMayusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());
    
            // Prueba

            var resultado = primeraLetraMayusculaAttribute.GetValidationResult(value, validationContext);

            // Verificación

            Assert.AreEqual(expected: ValidationResult.Success, actual: resultado);
        }

        [TestMethod]
        [DataRow("felipe")]
        public void IsValid_RetornaExistoso_SiValueTieneLaPrimeraLetraMinuscula0(
          string value)
        {
            // Preparación

            var primeraLetraMayusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());

            // Prueba

            var resultado = primeraLetraMayusculaAttribute.GetValidationResult(value, validationContext);

            // Verificación

            Assert.AreEqual(expected: "La primera letra tiene que ser mayúscula", actual: resultado!.ErrorMessage);
        }
    }
}
