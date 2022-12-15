using FileParser;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FilesController : ControllerBase
    {
        [HttpPost]
        public IActionResult Add(IFormFile file)
        {            
            var ms = new MemoryStream();
            var fs = file.OpenReadStream();
            var sr = new StreamReader(fs);
            sr.BaseStream.CopyTo(ms);
            var files = ms.ToArray();
            var parser = new Parser();            
            return Ok(parser.Parse(files));
        }
    }
}