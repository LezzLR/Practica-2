using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Practica_2.Models;

namespace Practica_2.Models.ViewModels
{
    public class CatalogoCursosViewModel : IValidatableObject
    {
        public string? Nombre { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Los créditos no pueden ser negativos.")]
        [Display(Name = "Créditos Mínimos")]
        public int? CreditosMin { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Los créditos no pueden ser negativos.")]
        [Display(Name = "Créditos Máximos")]
        public int? CreditosMax { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Horario Inicio")]
        public TimeSpan? HorarioInicio { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Horario Fin")]
        public TimeSpan? HorarioFin { get; set; }

        // Resultados
        public List<Curso> Cursos { get; set; } = new List<Curso>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HorarioInicio.HasValue && HorarioFin.HasValue && HorarioInicio > HorarioFin)
            {
                yield return new ValidationResult(
                    "El horario de fin no puede ser anterior al horario de inicio.",
                    new[] { nameof(HorarioInicio), nameof(HorarioFin) });
            }
            if (CreditosMin.HasValue && CreditosMax.HasValue && CreditosMin > CreditosMax)
            {
                yield return new ValidationResult(
                    "El crédito mínimo no puede ser mayor al crédito máximo.",
                    new[] { nameof(CreditosMin), nameof(CreditosMax) });
            }
        }
    }
}
