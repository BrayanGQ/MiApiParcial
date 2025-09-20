using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApiParcial.Data;
using MiApiParcial.Models;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/productos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
    {
        var productos = await _context.Productos
            .Where(p => p.Activo)
            .OrderBy(p => p.Id)
            .ToListAsync();

        return Ok(productos);
    }

    // GET: api/productos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

        if (producto == null)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }

        return Ok(producto);
    }

    // POST: api/productos
    [HttpPost]
    public async Task<ActionResult<Producto>> CreateProducto(Producto producto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        producto.FechaCreacion = DateTime.Now;
        producto.Activo = true;

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
    }

    // PUT: api/productos/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProducto(int id, Producto producto)
    {
        if (id != producto.Id)
        {
            return BadRequest(new { message = "ID en URL no coincide con ID del producto" });
        }

        var productoExistente = await _context.Productos.FindAsync(id);
        if (productoExistente == null || !productoExistente.Activo)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }

        // Actualizar propiedades
        productoExistente.Nombre = producto.Nombre;
        productoExistente.Descripcion = producto.Descripcion;
        productoExistente.Precio = producto.Precio;
        productoExistente.Stock = producto.Stock;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Error de concurrencia al actualizar" });
        }

        return Ok(productoExistente);
    }

    // DELETE: api/productos/5 (borrado lógico)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null || !producto.Activo)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }

        // Borrado lógico
        producto.Activo = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Producto '{producto.Nombre}' eliminado correctamente" });
    }

    // GET: api/productos/buscar?nombre=laptop&minPrecio=100&maxPrecio=2000
    [HttpGet("buscar")]
    public async Task<ActionResult<IEnumerable<Producto>>> BuscarProductos(
        [FromQuery] string? nombre,
        [FromQuery] decimal? minPrecio,
        [FromQuery] decimal? maxPrecio,
        [FromQuery] bool incluirInactivos = false)
    {
        var query = _context.Productos.AsQueryable();

        if (!incluirInactivos)
        {
            query = query.Where(p => p.Activo);
        }

        if (!string.IsNullOrEmpty(nombre))
        {
            query = query.Where(p => p.Nombre.Contains(nombre) ||
                                   (p.Descripcion != null && p.Descripcion.Contains(nombre)));
        }

        if (minPrecio.HasValue)
        {
            query = query.Where(p => p.Precio >= minPrecio.Value);
        }

        if (maxPrecio.HasValue)
        {
            query = query.Where(p => p.Precio <= maxPrecio.Value);
        }

        var productos = await query.OrderBy(p => p.Nombre).ToListAsync();

        return Ok(productos);
    }

    // GET: api/productos/estadisticas
    [HttpGet("estadisticas")]
    public async Task<IActionResult> GetEstadisticas()
    {
        var stats = new
        {
            TotalProductos = await _context.Productos.CountAsync(p => p.Activo),
            TotalStock = await _context.Productos.Where(p => p.Activo).SumAsync(p => p.Stock),
            PrecioPromedio = await _context.Productos.Where(p => p.Activo).AverageAsync(p => p.Precio),
            ProductoMasCaro = await _context.Productos.Where(p => p.Activo).MaxAsync(p => p.Precio),
            ProductoMasBarato = await _context.Productos.Where(p => p.Activo).MinAsync(p => p.Precio)
        };

        return Ok(stats);
    }
}