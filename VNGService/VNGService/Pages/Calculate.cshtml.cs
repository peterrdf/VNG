using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNGService.Pages
{
    public class CalculateModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnGetCubeVolume(double length)
        {
            var volume = length * length * length;
            return new JsonResult(new { Volume = volume });
        }
    }
}
