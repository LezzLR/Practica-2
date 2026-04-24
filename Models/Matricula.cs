using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Practica_2.Models
{
    public class Matricula
    {
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;

        [ForeignKey("CursoId")]
        public Curso? Curso { get; set; }

        [ForeignKey("UsuarioId")]
        public IdentityUser? Usuario { get; set; }
    }
}
