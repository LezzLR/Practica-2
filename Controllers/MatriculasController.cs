using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practica_2.Data;
using Practica_2.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Practica_2.Controllers
{
    [Authorize]
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            // Validar existencia y estado del curso
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null || !curso.Activo)
            {
                TempData["ErrorMessage"] = "El curso no existe o no se encuentra activo.";
                return RedirectToAction("Index", "Cursos");
            }

            // Validar Duplicidad
            var yaInscrito = await _context.Matriculas
                .AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada);
            
            if (yaInscrito)
            {
                TempData["ErrorMessage"] = "Ya te encuentras matriculado en este curso.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            // Validar Cupo Máximo
            var cantidadMatriculados = await _context.Matriculas
                .CountAsync(m => m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);

            if (cantidadMatriculados >= curso.CupoMaximo)
            {
                TempData["ErrorMessage"] = "Lo sentimos, el curso ha alcanzado su cupo máximo.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            // Validar Solapamiento de Horarios
            // Fórmula: InicioA < FinB && InicioB < FinA
            var matriculasActuales = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada)
                .ToListAsync();

            var solapamiento = matriculasActuales.FirstOrDefault(m => 
                curso.HorarioInicio < m.Curso!.HorarioFin && m.Curso.HorarioInicio < curso.HorarioFin);

            if (solapamiento != null && solapamiento.Curso != null)
            {
                TempData["ErrorMessage"] = $"El horario se solapa con tu curso actual: {solapamiento.Curso.Codigo} - {solapamiento.Curso.Nombre} ({solapamiento.Curso.HorarioInicio:hh\\:mm} a {solapamiento.Curso.HorarioFin:hh\\:mm}).";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            // Crear Matrícula
            var nuevaMatricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = userId,
                Estado = EstadoMatricula.Pendiente
            };

            _context.Matriculas.Add(nuevaMatricula);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"¡Inscripción exitosa! Tu matrícula en {curso.Codigo} ha sido registrada y está Pendiente de confirmación.";
            return RedirectToAction("Details", "Cursos", new { id = cursoId });
        }
    }
}
