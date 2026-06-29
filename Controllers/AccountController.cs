using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using DistribuidoraLosAndes.Data;
using DistribuidoraLosAndes.Models;
using System.Threading.Tasks;

namespace DistribuidoraLosAndes.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                // CORRECCIÓN 1: Redirigir a Tienda en lugar de Home
                return RedirectToAction("Index", "Tienda");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Por favor, llena todos los campos.";
                return View();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (usuario != null)
            {
                HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? "Usuario");
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol?.Nombre ?? "Cliente");

                // CORRECCIÓN 2: Redirigir a Tienda en lugar de Home al iniciar sesión con éxito
                return RedirectToAction("Index", "Tienda");
            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Nombre,Email,Password")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
                if (existe)
                {
                    ViewBag.Error = "El correo electrónico ya está registrado.";
                    return View(usuario);
                }

                usuario.RolId = 2; // Cliente por defecto

                _context.Add(usuario);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Login));
            }

            ViewBag.Error = "Por favor, revisa que todos los datos estén correctos.";
            return View(usuario);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            // Borra al usuario y el carrito de la memoria
            HttpContext.Session.Clear();
            // Lo devuelve a la tienda pública
            return RedirectToAction("Index", "Tienda");
        }
    }
}