using DistribuidoraLosAndes.Data;
using DistribuidoraLosAndes.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System; // <-- Agregado para usar DateTime
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DistribuidoraLosAndes.Controllers
{
    public class TiendaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TiendaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tienda
        public async Task<IActionResult> Index(int? categoriaId)
        {
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            ViewBag.CategoriaSeleccionada = categoriaId;

            var productos = _context.Productos.Include(p => p.Categoria).AsQueryable();

            if (categoriaId.HasValue)
            {
                productos = productos.Where(p => p.CategoriaId == categoriaId);
            }

            return View(await productos.ToListAsync());
        }

        // POST: Tienda/AgregarAlCarrito
        [HttpPost]
        public async Task<IActionResult> AgregarAlCarrito(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            List<CarritoItem> carrito = new List<CarritoItem>();
            string? carritoString = HttpContext.Session.GetString("Carrito");

            if (!string.IsNullOrEmpty(carritoString))
            {
                carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoString) ?? new List<CarritoItem>();
            }

            var itemExistente = carrito.FirstOrDefault(c => c.ProductoId == id);
            if (itemExistente != null)
            {
                itemExistente.Cantidad++;
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = 1,
                    ImagenUrl = producto.ImagenUrl
                });
            }

            HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(carrito));
            return RedirectToAction(nameof(Index));
        }

        // GET: Tienda/Carrito
        public IActionResult Carrito()
        {
            List<CarritoItem> carrito = new List<CarritoItem>();
            string? carritoString = HttpContext.Session.GetString("Carrito");

            if (!string.IsNullOrEmpty(carritoString))
            {
                carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoString) ?? new List<CarritoItem>();
            }

            ViewBag.Total = carrito.Sum(item => item.SubTotal);
            return View(carrito);
        }

        // POST: Tienda/EliminarDelCarrito
        [HttpPost]
        public IActionResult EliminarDelCarrito(int id)
        {
            string? carritoString = HttpContext.Session.GetString("Carrito");
            if (!string.IsNullOrEmpty(carritoString))
            {
                var carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoString) ?? new List<CarritoItem>();
                var item = carrito.FirstOrDefault(c => c.ProductoId == id);
                if (item != null)
                {
                    carrito.Remove(item);
                    HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(carrito));
                }
            }
            return RedirectToAction(nameof(Carrito));
        }

        // GET: Tienda/Checkout
        public IActionResult Checkout()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Account");

            string? carritoString = HttpContext.Session.GetString("Carrito");
            if (string.IsNullOrEmpty(carritoString))
                return RedirectToAction(nameof(Carrito));

            var carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoString);
            if (carrito == null || !carrito.Any())
                return RedirectToAction(nameof(Carrito));

            ViewBag.Total = carrito.Sum(c => c.SubTotal);
            return View();
        }

        // POST: Tienda/ConfirmarPedido
        [HttpPost]
        public async Task<IActionResult> ConfirmarPedido(string direccion, string metodoPago)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioIdStr))
                return RedirectToAction("Login", "Account");

            int usuarioId = int.Parse(usuarioIdStr);
            string? carritoString = HttpContext.Session.GetString("Carrito");
            var carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoString!);

            // 1. CREAR LA FACTURA (Con fecha UTC forzada)
            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                Total = carrito!.Sum(c => c.SubTotal),
                DireccionEnvio = direccion,
                MetodoPago = metodoPago,
                NumeroReferencia = "REF-" + DateTime.UtcNow.Ticks.ToString().Substring(0, 8), // <-- CAMBIADO A UTC
                Estado = EstadoPedido.Confirmado,
                FechaPedido = DateTime.UtcNow // <-- ESTA ES LA LÍNEA MÁGICA QUE SOLUCIONA EL ERROR
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            // 2. CREAR LOS DETALLES Y DESCONTAR INVENTARIO
            foreach (var item in carrito!)
            {
                var detalle = new PedidoDetalle
                {
                    PedidoId = pedido.Id,
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Precio
                };
                _context.PedidoDetalles.Add(detalle);

                var productoDb = await _context.Productos.FindAsync(item.ProductoId);
                if (productoDb != null)
                {
                    productoDb.Stock -= item.Cantidad;
                    _context.Update(productoDb);
                }
            }
            await _context.SaveChangesAsync();

            // 3. VACIAR EL CARRITO DE LA MEMORIA
            HttpContext.Session.Remove("Carrito");

            // 4. Mandarlo a su historial de compras
            return RedirectToAction("MisCompras");
        }

        // GET: Tienda/MisCompras
        public async Task<IActionResult> MisCompras()
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioIdStr))
                return RedirectToAction("Login", "Account");

            int usuarioId = int.Parse(usuarioIdStr);

            var pedidos = await _context.Pedidos
                .Include(p => p.Detalles!)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }
    }
}