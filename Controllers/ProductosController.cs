using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DistribuidoraLosAndes.Data;
using DistribuidoraLosAndes.Models;
using Microsoft.AspNetCore.Hosting; // NUEVO: Para guardar archivos
using System.Threading.Tasks;
using System.Linq;
using System.IO; // NUEVO: Para manejar rutas y archivos
using System; // NUEVO: Para generar nombres únicos

namespace DistribuidoraLosAndes.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // NUEVO: Para acceder a la carpeta wwwroot

        // Modificamos el constructor para inyectar IWebHostEnvironment
        public ProductosController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Productos (Muestra la lista)
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos.Include(p => p.Categoria).ToListAsync();
            return View(productos);
        }

        // GET: Productos/Create (Muestra el formulario)
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre");
            return View();
        }

        // POST: Productos/Create (Guarda en SQL y guarda la imagen)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,Stock,Precio,CategoriaId,ImagenUrl,ImagenArchivo")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                // LOGICA PARA GUARDAR LA IMAGEN FÍSICA
                if (producto.ImagenArchivo != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(producto.ImagenArchivo.FileName);
                    string folderPath = Path.Combine(wwwRootPath, "images", "productos");

                    // Si la carpeta no existe, la crea automáticamente
                    Directory.CreateDirectory(folderPath);

                    string filePath = Path.Combine(folderPath, fileName);

                    // Copiar la imagen a la carpeta
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await producto.ImagenArchivo.CopyToAsync(fileStream);
                    }

                    // Guardar la ruta en el producto
                    producto.ImagenUrl = "/images/productos/" + fileName;
                }

                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            ViewBag.CategoriaId = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Stock,Precio,CategoriaId,ImagenUrl,ImagenArchivo")] Producto producto)
        {
            if (id != producto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // LOGICA PARA ACTUALIZAR LA IMAGEN FÍSICA SI SUBIERON UNA NUEVA
                if (producto.ImagenArchivo != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(producto.ImagenArchivo.FileName);
                    string folderPath = Path.Combine(wwwRootPath, "images", "productos");

                    Directory.CreateDirectory(folderPath);
                    string filePath = Path.Combine(folderPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await producto.ImagenArchivo.CopyToAsync(fileStream);
                    }

                    producto.ImagenUrl = "/images/productos/" + fileName;
                }

                _context.Update(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoriaId = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null) return NotFound();

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}