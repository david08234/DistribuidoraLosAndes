using DistribuidoraLosAndes.Data;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
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

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }

            var pedidos = await _context.Pedidos.ToListAsync();
            var detalles = await _context.PedidoDetalles.ToListAsync();

            ViewBag.TotalIngresos = pedidos.Sum(p => p.Total);
            ViewBag.TotalVentas = pedidos.Count;
            ViewBag.BotellasVendidas = detalles.Sum(d => d.Cantidad);

            var ultimasVentas = await _context.Pedidos
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.FechaPedido)
                .Take(10)
                .ToListAsync();

            return View(ultimasVentas);
        }

        // CORRECCIÓN: Usamos la tabla Pedidos en lugar de Ventas
        public async Task<IActionResult> ExportarExcel()
        {
            // Traemos los pedidos de la base de datos
            var datos = await _context.Pedidos.ToListAsync();

            var builder = new StringBuilder();
            // Cabeceras exactas para tu archivo
            builder.AppendLine("ID,Fecha,Total,Estado,MetodoPago,Referencia");

            foreach (var item in datos)
            {
                // Usamos las propiedades exactas de tu clase Pedido
                builder.AppendLine($"{item.Id},{item.FechaPedido:yyyy-MM-dd HH:mm},{item.Total},{item.Estado},{item.MetodoPago},{item.NumeroReferencia}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "ReporteVentas.csv");
        }
    }
}