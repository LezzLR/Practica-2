using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practica_2.Data;
using Practica_2.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Practica_2.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cursos
        public async Task<IActionResult> Index(CatalogoCursosViewModel model)
        {
            // Solo buscar si el modelo es válido según DataAnnotations e IValidatableObject
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var query = _context.Cursos.Where(c => c.Activo).AsQueryable();

            if (!string.IsNullOrEmpty(model.Nombre))
            {
                query = query.Where(c => c.Nombre.Contains(model.Nombre));
            }

            if (model.CreditosMin.HasValue)
            {
                query = query.Where(c => c.Creditos >= model.CreditosMin.Value);
            }

            if (model.CreditosMax.HasValue)
            {
                query = query.Where(c => c.Creditos <= model.CreditosMax.Value);
            }

            if (model.HorarioInicio.HasValue)
            {
                query = query.Where(c => c.HorarioInicio >= model.HorarioInicio.Value);
            }

            if (model.HorarioFin.HasValue)
            {
                query = query.Where(c => c.HorarioFin <= model.HorarioFin.Value);
            }

            model.Cursos = await query.ToListAsync();

            return View(model);
        }

        // GET: Cursos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }
    }
}
