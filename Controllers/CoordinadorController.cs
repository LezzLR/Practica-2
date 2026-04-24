using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Practica_2.Data;
using Practica_2.Models;
using System.Threading.Tasks;

namespace Practica_2.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private const string CacheKey = "CursosActivos";

        public CoordinadorController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /Coordinador
        public async Task<IActionResult> Index()
        {
            var cursos = await _context.Cursos.OrderBy(c => c.Codigo).ToListAsync();
            return View(cursos);
        }

        // GET: /Coordinador/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Coordinador/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Curso curso)
        {
            if (!ModelState.IsValid)
                return View(curso);

            var existe = await _context.Cursos.AnyAsync(c => c.Codigo == curso.Codigo);
            if (existe)
            {
                ModelState.AddModelError("Codigo", "Ya existe un curso con ese código.");
                return View(curso);
            }

            curso.Activo = true;
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            // Invalidar caché
            await _cache.RemoveAsync(CacheKey);

            TempData["Success"] = $"Curso {curso.Codigo} creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Coordinador/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            return View(curso);
        }

        // POST: /Coordinador/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Curso curso)
        {
            if (id != curso.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(curso);

            // Verificar unicidad de código ignorando el propio curso
            var duplicado = await _context.Cursos
                .AnyAsync(c => c.Codigo == curso.Codigo && c.Id != id);
            if (duplicado)
            {
                ModelState.AddModelError("Codigo", "Ya existe otro curso con ese código.");
                return View(curso);
            }

            _context.Update(curso);
            await _context.SaveChangesAsync();

            // Invalidar caché
            await _cache.RemoveAsync(CacheKey);

            TempData["Success"] = $"Curso {curso.Codigo} actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Coordinador/Desactivar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            curso.Activo = !curso.Activo;
            await _context.SaveChangesAsync();

            // Invalidar caché
            await _cache.RemoveAsync(CacheKey);

            var estado = curso.Activo ? "activado" : "desactivado";
            TempData["Success"] = $"Curso {curso.Codigo} {estado} exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Coordinador/Matriculas/5
        public async Task<IActionResult> Matriculas(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            var matriculas = await _context.Matriculas
                .Include(m => m.Usuario)
                .Where(m => m.CursoId == id)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync();

            ViewBag.Curso = curso;
            return View(matriculas);
        }

        // POST: /Coordinador/Confirmar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int id)
        {
            var matricula = await _context.Matriculas.Include(m => m.Curso).FirstOrDefaultAsync(m => m.Id == id);
            if (matricula == null) return NotFound();

            matricula.Estado = EstadoMatricula.Confirmada;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Matrícula confirmada exitosamente.";
            return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
        }

        // POST: /Coordinador/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var matricula = await _context.Matriculas.Include(m => m.Curso).FirstOrDefaultAsync(m => m.Id == id);
            if (matricula == null) return NotFound();

            matricula.Estado = EstadoMatricula.Cancelada;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Matrícula cancelada exitosamente.";
            return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
        }
    }
}
