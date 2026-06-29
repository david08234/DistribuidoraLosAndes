using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // <-- LIBRERÍA NUEVA PARA MANEJAR ARCHIVOS

namespace DistribuidoraLosAndes.Models
{
    [Table("productos")]
    public class Producto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [Column("stock")]
        public int Stock { get; set; }

        [Required]
        [Column("precio", TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Column("imagenurl")]
        public string? ImagenUrl { get; set; }

        // 👇👇👇 AQUÍ ESTÁ LA MAGIA PARA SUBIR ARCHIVOS 👇👇👇
        [NotMapped] // Esto le dice a PostgreSQL: "Ignora esto, es solo para el formulario web"
        [Display(Name = "Subir Imagen desde PC")]
        public IFormFile? ImagenArchivo { get; set; }

        [Required]
        [Column("categoriaid")]
        public int CategoriaId { get; set; }

        // Relación: Un producto pertenece a una categoría
        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }
    }
}