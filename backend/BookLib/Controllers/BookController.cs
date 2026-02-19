using BookLib.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookLib.Controllers
{
    [ApiController]
    [Route("[controller]/book")]
    public class BookController : Controller

    {
        private static List<Book> books = new List<Book>();
        private static int nextId = 1;
        [HttpGet]
        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            int totalItems = books.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedBooks = books
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                totalItems,
                totalPages,
                currentPage = pageNumber,
                pageSize,
                data = pagedBooks
            };

            return Ok(response);
        }

        [HttpPost]
        public IActionResult Create(Book book)
        {
            // ISBN validation: exactly 13 digits
            if (string.IsNullOrWhiteSpace(book.Isbn) ||
                book.Isbn.Length != 13 ||
                !book.Isbn.All(char.IsDigit))
            {
                return BadRequest("ISBN must be exactly 13 numeric characters.");
            }

            // Duplicate check: Title OR ISBN
            bool exists = books.Any(b =>
                b.Title.Equals(book.Title, StringComparison.OrdinalIgnoreCase) ||
                b.Isbn == book.Isbn);

            if (exists)
            {
                return Conflict("A book with the same title or ISBN already exists.");
            }
            book.Id = nextId++;
            books.Add(book);
            return Ok(book);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Book updatedBook)
        {
            var book = books.FirstOrDefault(b => b.Id == id);
            if (book == null) return NotFound();

            // Duplicate check excluding current book
            bool exists = books.Any(b =>
                b.Id != id &&
                (b.Title.Equals(updatedBook.Title, StringComparison.OrdinalIgnoreCase) ||
                 b.Isbn == updatedBook.Isbn));

            if (exists)
            {
                return Conflict("Another book with the same title or ISBN already exists.");
            }

            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.Isbn = updatedBook.Isbn;
            book.PublicationDate = updatedBook.PublicationDate;

            return Ok(book);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var book = books.FirstOrDefault(b => b.Id == id);
            if (book == null) return NotFound();

            books.Remove(book);
            return NoContent();
        }
    }
}
