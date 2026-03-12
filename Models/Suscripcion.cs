namespace BibliotecaAPI.Models
{
    public class Suscripcion
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public string? TipoPlan { get; set; }

        public decimal Precio { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public bool Activa { get; set; }
    }
}