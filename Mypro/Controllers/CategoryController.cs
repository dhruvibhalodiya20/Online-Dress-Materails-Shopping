using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mypro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mypro.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext abc;

        public CategoryController(ApplicationDbContext context)
        {
            abc = context;
        }

        public async Task<IActionResult> Details(string name, List<string> colors, string baseColor)
        {
            if (string.IsNullOrEmpty(name))
                return NotFound();

            var category = await abc.Categories
               .FirstOrDefaultAsync(c => c.CategoryName.Trim().ToLower() == name.Trim().ToLower());


            if (category == null)
                return NotFound();

            // Get all images for this category first
            var imagesList = await abc.CategoryImages
                .AsNoTracking()
                .Where(img => img.CategoryId == category.CategoryId)
                .ToListAsync();

           
            if (colors != null && colors.Any())
            {
                imagesList = imagesList
                    .Where(img => !string.IsNullOrEmpty(img.Color) && colors.Contains(img.Color))
                    .ToList();
            }

            var images = imagesList.Select(img => img.ImagePath).ToList();

            var availableColors = imagesList
                .Select(img => string.IsNullOrEmpty(img.Color) ? "Unknown" : img.Color)
                .Distinct()
                .ToList();

            // Generate palette
            List<string> generatedPalette = new();
            if (!string.IsNullOrEmpty(baseColor))
            {
                for (int i = 1; i <= 5; i++)
                    generatedPalette.Add(ShadeColor(baseColor, i * -15));
            }

            ViewBag.CategoryName = category.CategoryName;
            ViewBag.ImagesList = imagesList; 

            ViewBag.AvailableColors = availableColors;
            ViewBag.SelectedColors = colors ?? new List<string>();
            ViewBag.GeneratedPalette = generatedPalette;

            return View();
        }

        public async Task<IActionResult> ImageDetails(int id)
        {
            if (id <= 0)
                return NotFound();

            var image = await abc.CategoryImages
                .Include(c => c.Category)
                .FirstOrDefaultAsync(i => i.ImageId == id);

            if (image == null)
                return NotFound();

            return View(image);
        }


        [HttpPost]
        public JsonResult GeneratePalette(string baseColor)
        {
            List<string> colors = new();
            if (!string.IsNullOrEmpty(baseColor))
            {
                for (int i = 1; i <= 5; i++)
                    colors.Add(ShadeColor(baseColor, i * -15));
            }
            return Json(colors);
        }

        private string ShadeColor(string color, int percent)
        {
            int R = Convert.ToInt32(color.Substring(1, 2), 16);
            int G = Convert.ToInt32(color.Substring(3, 2), 16);
            int B = Convert.ToInt32(color.Substring(5, 2), 16);

            R = Math.Max(0, Math.Min(255, (R * (100 + percent)) / 100));
            G = Math.Max(0, Math.Min(255, (G * (100 + percent)) / 100));
            B = Math.Max(0, Math.Min(255, (B * (100 + percent)) / 100));

            return $"#{R:X2}{G:X2}{B:X2}";
        }
    }
}
