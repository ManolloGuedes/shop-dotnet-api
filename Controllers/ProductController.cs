using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

[Route("v1/products")]
public class ProductController : ControllerBase
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult<List<Product>>> Get(
        [FromServices] DataContext context
    )
    {
        var products = await context
            .Products
            .Include(product => product.Category)
            .AsNoTracking()
            .ToListAsync();
        return Ok(products);
    }

    [HttpGet]
    [Route("categories/{id:int}")]
    public async Task<ActionResult<Product>> GetByCategory(
        int id,
        [FromServices] DataContext context
    )
    {
        var product = await context
            .Products
            .Include(product => product.Category)
            .AsNoTracking()
            .Where(product => product.Category.Id == id)
            .ToListAsync();

        return Ok(product);
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<Product>> GetById(
        int id,
        [FromServices] DataContext context
    )
    {
        var product = await context
            .Products
            .Include(product => product.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id);

        return Ok(product);
    }

    [HttpPost]
    [Route("")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Product>> Post(
        [FromBody] Product product,
        [FromServices] DataContext context
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(category => category.Id == product.Category.Id);
            if (category == null)
            {
                return NotFound(new { message = "Categoria não encontrada" });
            }

            product.Category = category;

            context.Products.Add(product);

            await context.SaveChangesAsync();
            return Ok(product);
        }
        catch
        {
            return BadRequest(new { message = "Não foi possível criar o produto" });
        }
    }

    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<Product>> Put(
        int id,
        [FromBody] Product product,
        [FromServices] DataContext context
    )
    {
        if (id != product.Id)
            return NotFound(new { message = "Produto não encontrado" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            context.Entry<Product>(product).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return Ok(product);
        }
        catch
        {
            return BadRequest(new { message = "Não foi possível atualizar o produto" });
        }
    }

    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<Product>> Delete(
        int id,
        [FromServices] DataContext context
    )
    {
        var product = await context
            .Products
            .FirstOrDefaultAsync(product => product.Id == id);

        if (product == null)
            return NotFound(new { message = "Produto não encontrado" });

        try
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return Ok(new { message = "Produto removido com sucesso" });
        }
        catch
        {
            return BadRequest(new { message = "Não foi possível remover o produto" });
        }
    }
}