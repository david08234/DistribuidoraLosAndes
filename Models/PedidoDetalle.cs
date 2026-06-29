using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLosAndes.Models
{
    // ¡AQUÍ ESTÁ EL CAMBIO CLAVE!
    [Table("DetallesPedido")]
    public class PedidoDetalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;

        [ForeignKey("PedidoId")]
        public virtual Pedido? Pedido { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto? Producto { get; set; }
    }
}