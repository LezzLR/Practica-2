using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Practica_2.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Practica_2.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Migrar la BD (crea si no existe y aplica migraciones pendientes)
            context.Database.Migrate();

            // Seed Roles
            var roles = new[] { "Coordinador", "Estudiante" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Coordinador
            var adminEmail = "coordinador@uni.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Coordinador");
                }
            }

            // Seed Cursos
            if (!context.Cursos.Any())
            {
                var cursos = new[]
                {
                    new Curso { Codigo = "CS101", Nombre = "Introducción a la Programación", Creditos = 4, CupoMaximo = 30, HorarioInicio = new TimeSpan(8, 0, 0), HorarioFin = new TimeSpan(10, 0, 0), Activo = true },
                    new Curso { Codigo = "CS102", Nombre = "Estructuras de Datos", Creditos = 4, CupoMaximo = 25, HorarioInicio = new TimeSpan(10, 0, 0), HorarioFin = new TimeSpan(12, 0, 0), Activo = true },
                    new Curso { Codigo = "MATH201", Nombre = "Cálculo I", Creditos = 5, CupoMaximo = 40, HorarioInicio = new TimeSpan(14, 0, 0), HorarioFin = new TimeSpan(16, 0, 0), Activo = true }
                };

                context.Cursos.AddRange(cursos);
                await context.SaveChangesAsync();
            }
        }
    }
}
