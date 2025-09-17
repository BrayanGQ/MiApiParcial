using MiApiParcial.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    // Lista simple de productos
    private static List<Producto> productos = new()
    {
        new Producto { Id = 1, Nombre = "Laptop", Precio = 1500 },
        new Producto { Id = 2, Nombre = "Mouse", Precio = 25 },
        new Producto { Id = 3, Nombre = "Teclado", Precio = 75 }
    };

    // GET: api/productos (ver todos)
    [HttpGet]
    public IActionResult GetTodos()
    {
        return Ok(productos);
    }

    // GET: api/productos/1 (ver uno)
    [HttpGet("{id}")]
    public IActionResult GetUno(int id)
    {
        var producto = productos.Find(p => p.Id == id);
        if (producto == null) return NotFound();
        return Ok(producto);
    }

    // POST: api/productos (crear nuevo)
    [HttpPost]
    public IActionResult Crear(Producto producto)
    {
        producto.Id = productos.Count + 1;
        productos.Add(producto);
        return Ok(producto);
    }
}