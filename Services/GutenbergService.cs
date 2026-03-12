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
                books.Add(new BookDTO
                {
                    Id = item.GetProperty("id").GetInt32(),
                    Titulo = item.GetProperty("title").GetString(),
                    Autor = item.GetProperty("authors")[0].GetProperty("name").GetString(),
                    Imagen = item.GetProperty("formats").GetProperty("image/jpeg").GetString(),
                    LinkLectura = item.GetProperty("formats").GetProperty("text/html").GetString()
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

    var titulo = data.GetProperty("title").GetString();

    var autor = data.GetProperty("authors")[0].GetProperty("name").GetString();

    var imagen = data.GetProperty("formats")
        .GetProperty("image/jpeg").GetString();

    var lectura = data.GetProperty("formats")
        .GetProperty("text/html").GetString();

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