namespace RestaurantePOS.Models;

public class OrdenDto
{
    public int MesaId { get; set; }
    public List<DetalleOrdenDto> Detalles { get; set; } = new List<DetalleOrdenDto>();
}

public class DetalleOrdenDto
{
    public int PlatoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}