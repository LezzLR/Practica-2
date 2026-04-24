# Portal Académico Universitario — Práctica 2

Aplicación web interna para gestionar cursos, estudiantes y matrículas.  
Desarrollada con ASP.NET Core MVC (.NET 8), Identity, EF Core (SQLite), Redis y desplegada en Render.com.

---

## 🔧 Ejecución Local

### Requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Cuenta de Redis Labs (https://app.redislabs.com/) o Redis local

### Pasos

```bash
# 1. Clonar el repositorio
git clone https://github.com/TU_USUARIO/Practica-2.git
cd Practica-2

# 2. Restaurar dependencias
dotnet restore

# 3. Aplicar migraciones (se ejecutan automáticamente al iniciar,
#    pero puedes forzar con):
dotnet ef database update

# 4. Ejecutar la aplicación
dotnet run
```

La aplicación estará en: `https://localhost:5001` o `http://localhost:5000`

### Variables de Entorno (Local)

Las configuraciones se leen desde `appsettings.json`:

| Clave | Ejemplo |
|---|---|
| `ConnectionStrings:DefaultConnection` | `DataSource=app.db;Cache=Shared` |
| `ConnectionStrings:RedisConnection` | `redis-XXXX.cloud.redislabs.com:XXXX,password=XXXX` |

---

## 🗃️ Migraciones

La migración inicial `DominioInicial` se aplica automáticamente al iniciar la app gracias al `DbInitializer`.

Para agregar nuevas migraciones manualmente:

```bash
dotnet ef migrations add NombreMigracion
dotnet ef database update
```

---

## 🌱 Datos Semilla (Seeding)

Al iniciar la aplicación por primera vez se crean automáticamente:

| Tipo | Detalle |
|---|---|
| Roles | `Coordinador`, `Estudiante` |
| Usuario Coordinador | Email: `coordinador@uni.edu` / Password: `Admin123!` |
| Cursos | CS101 (Intro. Programación), CS102 (Estructuras de Datos), MATH201 (Cálculo I) |

---

## 🌐 Variables de Entorno en Render.com

| Variable | Valor |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://0.0.0.0:${PORT}` |
| `ConnectionStrings__DefaultConnection` | `DataSource=app.db;Cache=Shared` |
| `Redis__ConnectionString` | `redis-XXXX.cloud.redislabs.com:XXXX,password=XXXX` |

---

## 🚀 URL de Render

> **URL de producción:** `https://TU-APP.onrender.com`  
> *(Actualizar una vez desplegado)*

---

## 📋 Ramas y PRs

| Rama | Descripción |
|---|---|
| `feature/bootstrap-dominio` | Proyecto base, modelos, migraciones y seeding |
| `feature/catalogo-cursos` | Vista catálogo con filtros y detalle de cursos |
| `feature/matriculas` | Inscripción con validaciones de cupo y horarios |
| `feature/sesion-redis` | Sesiones y caché con Redis |
| `feature/panel-coordinador` | CRUD de cursos y gestión de matrículas |
| `deploy/render` | Despliegue en Render.com |
