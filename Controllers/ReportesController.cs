using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DistribuidoraLosAndes.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DistribuidoraLosAndes.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reportes
        public async Task<IActionResult> Index()
        {
            // Protegemos el área (solo si inició sesión)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }

            // 1. Calculamos las estadísticas generales
            var pedidos = await _context.Pedidos.ToListAsync();
            var detalles = await _context.PedidoDetalles.ToListAsync(); // Ojo: usando DetallesPedido como está en tu SQL

            ViewBag.TotalIngresos = pedidos.Sum(p => p.Total);
            ViewBag.TotalVentas = pedidos.Count;
            ViewBag.BotellasVendidas = detalles.Sum(d => d.Cantidad);

            // 2. Traemos las últimas 10 ventas para mostrar en la tabla, ordenadas por la más reciente
            var ultimasVentas = await _context.Pedidos
                .Include(p => p.Usuario) // Para saber quién compró (si tienes esta relación activa)
                .OrderByDescending(p => p.FechaPedido)
                .Take(10)
                .ToListAsync();

            return View(ultimasVentas);
        }
    }
}