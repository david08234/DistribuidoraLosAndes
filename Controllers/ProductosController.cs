using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DistribuidoraLosAndes.Data;
using DistribuidoraLosAndes.Models;
using System.Threading.Tasks;
using System.Linq;

namespace DistribuidoraLosAndes.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Productos (Muestra la lista)
        public async Task<IActionResult> Index()
        {
            // Incluimos la categoría para poder ver el nombre en la tabla, no solo el número
            var productos = await _context.Productos.Include(p => p.Categoria).ToListAsync();
            return View(productos);
        }

        // GET: Productos/Create (Muestra el formulario)
        public IActionResult Create()
        {
            // Enviamos la lista de categorías al menú desplegable HTML
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre");
            return View();
        }

        // POST: Productos/Create (Guarda en SQL)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Stock,Precio,CategoriaId,ImagenUrl")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Si hay error, recargamos el menú de categorías
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }
        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            // Carga la lista de categorías para el menú desplegable
            ViewBag.CategoriaId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoriaId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
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