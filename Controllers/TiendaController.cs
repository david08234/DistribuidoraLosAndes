using DistribuidoraLosAndes.Data;
using DistribuidoraLosAndes.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

        // GET: Tienda (Ahora con soporte para búsqueda de voz/texto)
        public async Task<IActionResult> Index(int? categoriaId, string busqueda)
        {
            // Consulta base
            var query = _context.Productos.Include(p => p.Categoria).AsQueryable();

            // Filtro por categoría
            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId);
                ViewBag.CategoriaSeleccionada = categoriaId;
            }

            // Filtro por búsqueda de voz/texto
            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(p => p.Nombre.Contains(busqueda) || (p.Descripcion != null && p.Descripcion.Contains(busqueda)));
            }

            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(await query.ToListAsync());
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

            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                Total = carrito!.Sum(c => c.SubTotal),
                DireccionEnvio = direccion,
                MetodoPago = metodoPago,
                NumeroReferencia = "REF-" + DateTime.UtcNow.Ticks.ToString().Substring(0, 8),
                Estado = EstadoPedido.Confirmado,
                FechaPedido = DateTime.UtcNow
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

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

            HttpContext.Session.Remove("Carrito");

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