namespace BibliotecaAPI.Models
{
    public class BookDTO
    {
        public int Id { get; set; }

        public string Titulo { get; set; }

        public string Autor { get; set; }

        public string Imagen { get; set; }

        public string LinkLectura { get; set; }

        public bool Premium { get; set; } = false;
    }
}