namespace BibliotecaAPI.Models
{
    public class Favorito
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int BookId { get; set; }

        public string Titulo { get; set; }

        public string Autor { get; set; }

        public string Imagen { get; set; }
    }
}