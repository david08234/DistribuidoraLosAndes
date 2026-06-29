using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLosAndes.Models
{
    public enum EstadoPedido
    {
        Pendiente,
        Confirmado,
        Enviado,
        Entregado,
        Cancelado
    }

    [Table("Pedidos")]
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        [StringLength(200)]
        public string? DireccionEnvio { get; set; }

        [StringLength(50)]
        public string? MetodoPago { get; set; }

        [StringLength(20)]
        public string? NumeroReferencia { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        public virtual ICollection<PedidoDetalle>? Detalles { get; set; }
    }
}