using System.Text.Json;
using BibliotecaAPI.Models;

namespace BibliotecaAPI.Services
{
    public class GutenbergService
    {
        private readonly HttpClient _http;

        public GutenbergService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BookDTO>> SearchBooks(string query)
        {
            var response = await _http.GetAsync($"https://gutendex.com/books?search={query}");

            var json = await response.Content.ReadAsStringAsync();

            var document = JsonDocument.Parse(json);

            var books = new List<BookDTO>();

            foreach (var item in document.RootElement.GetProperty("results").EnumerateArray())
            {
                // Manejar autores (puede no tener)
                string autor = "Autor desconocido";
                if (item.TryGetProperty("authors", out var authorsElement) && 
                    authorsElement.GetArrayLength() > 0)
                {
                    autor = authorsElement[0].GetProperty("name").GetString() ?? "Autor desconocido";
                }

                // Manejar imagen (puede no tener)
                string imagen = null;
                if (item.TryGetProperty("formats", out var formatsElement) &&
                    formatsElement.TryGetProperty("image/jpeg", out var imageElement))
                {
                    imagen = imageElement.GetString();
                }

                // Manejar link de lectura (puede no tener)
                string linkLectura = null;
                if (item.TryGetProperty("formats", out var formatsElement2) &&
                    formatsElement2.TryGetProperty("text/html", out var htmlElement))
                {
                    linkLectura = htmlElement.GetString();
                }

                books.Add(new BookDTO
                {
                    Id = item.GetProperty("id").GetInt32(),
                    Titulo = item.GetProperty("title").GetString() ?? "Sin título",
                    Autor = autor,
                    Imagen = imagen,
                    LinkLectura = linkLectura
                });
            }

            return books;
        }

        public async Task<string?> GetBookReadLink(int bookId)
        {
            var response = await _http.GetAsync($"https://gutendex.com/books/{bookId}");

            var json = await response.Content.ReadAsStringAsync();

            var document = JsonDocument.Parse(json);

            var formats = document.RootElement.GetProperty("formats");

            if (formats.TryGetProperty("text/html", out var html))
                return html.GetString();

            return null;
        }


        public async Task<BookDTO?> ObtenerLibro(int bookId)
{
    var url = $"https://gutendex.com/books/{bookId}";

    var response = await _http.GetAsync(url);

    if (!response.IsSuccessStatusCode)
        return null;

    var json = await response.Content.ReadAsStringAsync();

    var data = JsonDocument.Parse(json).RootElement;

    var titulo = data.GetProperty("title").GetString() ?? "Sin título";

    // Manejar autores
    string autor = "Autor desconocido";
    if (data.TryGetProperty("authors", out var authorsElement) && 
        authorsElement.GetArrayLength() > 0)
    {
        autor = authorsElement[0].GetProperty("name").GetString() ?? "Autor desconocido";
    }

    // Manejar imagen
    string imagen = null;
    if (data.TryGetProperty("formats", out var formatsElement) &&
        formatsElement.TryGetProperty("image/jpeg", out var imageElement))
    {
        imagen = imageElement.GetString();
    }

    // Manejar link de lectura
    string lectura = null;
    if (data.TryGetProperty("formats", out var formatsElement2) &&
        formatsElement2.TryGetProperty("text/html", out var htmlElement))
    {
        lectura = htmlElement.GetString();
    }

    return new BookDTO
    {
        Titulo = titulo,
        Autor = autor,
        Imagen = imagen,
        LinkLectura = lectura
    };
}
    }
}