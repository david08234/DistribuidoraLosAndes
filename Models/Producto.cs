using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLosAndes.Models
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Exactamente igual a tu script SQL
        public decimal Precio { get; set; }

        public string? ImagenUrl { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        // Relación: Un producto pertenece a una categoría
        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }
    }
}