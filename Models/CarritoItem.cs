namespace DistribuidoraLosAndes.Models
{
    public class CarritoItem
    {
        public int ProductoId { get; set; }
        public string? Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string? ImagenUrl { get; set; }

        // Calcula el subtotal automáticamente (Precio x Cantidad)
        public decimal SubTotal => Precio * Cantidad;
    }
}