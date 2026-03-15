namespace BibliotecaAPI.Models
{
    public class Suscripcion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string TipoPlan { get; set; } = "Basico"; // Basico, Premium
        public decimal Precio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activa { get; set; }
        
        // Propiedades calculadas
        public int LimiteLibrosDiario => TipoPlan == "Basico" ? 3 : int.MaxValue;
        public bool EsPremium => TipoPlan == "Premium";
    }

    public static class TiposPlan
    {
        public const string BASICO = "Basico";
        public const string PREMIUM = "Premium";
    }
}