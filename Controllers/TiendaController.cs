using DistribuidoraLosAndes.Data;
using DistribuidoraLosAndes.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        // Recibe opcionalmente un ID de categoría para filtrar las bebidas
        public async Task<IActionResult> Index(int? categoriaId)
        {
            // 1. Mandamos todas las categorías a la vista para dibujar el menú lateral
            ViewBag.Categorias = await _context.Categorias.ToListAsync();

            // 2. Guardamos qué categoría seleccionó el usuario para pintarla de oscuro en el menú
            ViewBag.CategoriaSeleccionada = categoriaId;

            // 3. Preparamos la consulta de productos (incluyendo el nombre de su categoría)
            var productos = _context.Productos.Include(p => p.Categoria).AsQueryable();

            // Si el cliente hizo clic en una categoría (ej. Rones), filtramos la lista
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

            // 1. Leer la memoria del carrito actual
            List<CarritoItem> carrito = new List<CarritoItem>();
            string? carritoString = HttpContext.Session.GetString("Carrito");

            if (!string.IsNullOrEmpty(carritoString))
            {
                carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoString) ?? new List<CarritoItem>();
            }

            // 2. Revisar si el Flor de Caña (o cualquier producto) ya estaba en el carrito
            var itemExistente = carrito.FirstOrDefault(c => c.ProductoId == id);
            if (itemExistente != null)
            {
                itemExistente.Cantidad++; // Si ya estaba, solo sumamos 1
            }
            else
            {
                // Si es nuevo, lo metemos a la lista
                carrito.Add(new CarritoItem
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = 1,
                    ImagenUrl = producto.ImagenUrl
                });
            }

            // 3. Guardar la lista actualizada de vuelta en la memoria de sesión
            HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(carrito));

            // Recargar la tienda
            return RedirectToAction(nameof(Index));
        }
        // GET: Tienda/Carrito
        public IActionResult Carrito()
        {
            // Leemos qué hay en la memoria
            List<CarritoItem> carrito = new List<CarritoItem>();
            string? carritoString = HttpContext.Session.GetString("Carrito");

            if (!string.IsNullOrEmpty(carritoString))
            {
                carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoString) ?? new List<CarritoItem>();
            }

            // Calculamos el total de dinero sumando todos los subtotales
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

                // Buscamos el producto y lo borramos de la lista
                var item = carrito.FirstOrDefault(c => c.ProductoId == id);
                if (item != null)
                {
                    carrito.Remove(item);
                    // Guardamos la lista actualizada
                    HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(carrito));
                }
            }
            return RedirectToAction(nameof(Carrito));
        }

        // GET: Tienda/Checkout
        public IActionResult Checkout()
        {
            // 1. Validar si inició sesión
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Account");

            // 2. Leer el carrito de la memoria
            string? carritoString = HttpContext.Session.GetString("Carrito");
            if (string.IsNullOrEmpty(carritoString))
                return RedirectToAction(nameof(Carrito)); // Si está vacío, lo devuelve al carrito

            var carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoString);
            if (carrito == null || !carrito.Any())
                return RedirectToAction(nameof(Carrito));

            // 3. Mandar el total a pagar a la vista
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

            // 1. CREAR LA FACTURA (Tabla Pedidos)
            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                Total = carrito!.Sum(c => c.SubTotal),
                DireccionEnvio = direccion,
                MetodoPago = metodoPago,
                NumeroReferencia = "REF-" + DateTime.Now.Ticks.ToString().Substring(0, 8),
                Estado = EstadoPedido.Confirmado
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync(); // Guardamos para que SQL le asigne un ID al pedido

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

                // Descontamos el stock de la botella en la tabla Productos
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
            // 1. Verificamos quién es el usuario
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioIdStr))
                return RedirectToAction("Login", "Account");

            int usuarioId = int.Parse(usuarioIdStr);

            // 2. Buscamos sus facturas, incluyendo los detalles y los nombres de los productos
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