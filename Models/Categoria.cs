using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLosAndes.Models
{
    [Table("Categorias")]
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Descripcion { get; set; }

        // Relación: Una categoría tiene muchos productos
        public virtual ICollection<Producto>? Productos { get; set; }
    }
}

