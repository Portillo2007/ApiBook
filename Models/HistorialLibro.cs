using System;

namespace BibliotecaAPI.Models
{
    public class HistorialLibro
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int BookId { get; set; }

        public string Titulo { get; set; }

        public string Autor { get; set; }

        public string Imagen { get; set; }

        public DateTime FechaLectura { get; set; }

        public string LinkLectura { get; set; }
    }
}
