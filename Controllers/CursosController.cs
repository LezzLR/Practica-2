using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Practica_2.Data;
using Practica_2.Models;
using Practica_2.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace Practica_2.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private const string CacheKey = "CursosActivos";

        public CursosController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: Cursos
        public async Task<IActionResult> Index(CatalogoCursosViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            List<Curso> cursosActivos;

            // Intentar obtener de la caché
            var cachedCursos = await _cache.GetStringAsync(CacheKey);

            if (!string.IsNullOrEmpty(cachedCursos))
            {
                cursosActivos = JsonSerializer.Deserialize<List<Curso>>(cachedCursos) ?? new List<Curso>();
            }
            else
            {
                // Si no está en caché, obtener de la BD
                cursosActivos = await _context.Cursos.Where(c => c.Activo).ToListAsync();

                // Guardar en caché por 60 segundos
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };
                
                var serializedCursos = JsonSerializer.Serialize(cursosActivos);
                await _cache.SetStringAsync(CacheKey, serializedCursos, cacheOptions);
            }

            // Aplicar filtros en memoria
            var query = cursosActivos.AsQueryable();

            if (!string.IsNullOrEmpty(model.Nombre))
                query = query.Where(c => c.Nombre.Contains(model.Nombre, StringComparison.OrdinalIgnoreCase));
            
            if (model.CreditosMin.HasValue)
                query = query.Where(c => c.Creditos >= model.CreditosMin.Value);
            
            if (model.CreditosMax.HasValue)
                query = query.Where(c => c.Creditos <= model.CreditosMax.Value);
            
            if (model.HorarioInicio.HasValue)
                query = query.Where(c => c.HorarioInicio >= model.HorarioInicio.Value);
            
            if (model.HorarioFin.HasValue)
                query = query.Where(c => c.HorarioFin <= model.HorarioFin.Value);

            model.Cursos = query.ToList();

            return View(model);
        }

        // GET: Cursos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FirstOrDefaultAsync(m => m.Id == id);
            if (curso == null) return NotFound();

            // Sesión: Guardar último curso visitado
            HttpContext.Session.SetString("UltimoCursoId", curso.Id.ToString());
            HttpContext.Session.SetString("UltimoCursoNombre", curso.Nombre);

            return View(curso);
        }

        // POST: Cursos/CreateDummy (Solo para demostrar invalidación de caché)
        [HttpPost]
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> CreateDummy(Curso curso)
        {
            _context.Add(curso);
            await _context.SaveChangesAsync();
            
            // Invalidar Caché
            await _cache.RemoveAsync(CacheKey);

            return RedirectToAction(nameof(Index));
        }

        // POST: Cursos/EditDummy (Solo para demostrar invalidación de caché)
        [HttpPost]
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> EditDummy(int id, Curso curso)
        {
            _context.Update(curso);
            await _context.SaveChangesAsync();

            // Invalidar Caché
            await _cache.RemoveAsync(CacheKey);

            return RedirectToAction(nameof(Index));
        }
    }
}
