
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Mypro.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Mypro.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connection;

        public HomeController(IConfiguration config)
        {
            _connection = config.GetConnectionString("DefaultConnection");
        }
        public IActionResult Rating() => View();

        // GET: Get all reviews
        [HttpGet]
        public JsonResult GetReviews()
        {
            var reviews = new List<Review>();
            using (var con = new SqlConnection(_connection))
            {
                con.Open();
                using (var cmd = new SqlCommand("EXEC sp_GetReviews", con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reviews.Add(new Review
                        {
                            Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                            Name = reader["Name"]?.ToString(),
                            Stars = reader["Stars"] != DBNull.Value ? Convert.ToInt32(reader["Stars"]) : 0,
                            ReviewText = reader["ReviewText"]?.ToString(),
                            CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.MinValue
                        });
                    }
                }
            }
            return Json(reviews);
        }

        // GET: Get summary
        [HttpGet]
        public JsonResult GetReviewSummary()
        {
            var summary = new ReviewSummary();
            using (var con = new SqlConnection(_connection))
            {
                con.Open();
                using (var cmd = new SqlCommand("EXEC sp_GetReviewSummary", con))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        summary.AverageRating = reader["AverageRating"] != DBNull.Value ? Convert.ToDecimal(reader["AverageRating"]) : 0;
                        summary.TotalReviews = reader["TotalReviews"] != DBNull.Value ? Convert.ToInt32(reader["TotalReviews"]) : 0;
                        summary.FiveStars = reader["FiveStars"] != DBNull.Value ? Convert.ToInt32(reader["FiveStars"]) : 0;
                        summary.FourStars = reader["FourStars"] != DBNull.Value ? Convert.ToInt32(reader["FourStars"]) : 0;
                        summary.ThreeStars = reader["ThreeStars"] != DBNull.Value ? Convert.ToInt32(reader["ThreeStars"]) : 0;
                        summary.TwoStars = reader["TwoStars"] != DBNull.Value ? Convert.ToInt32(reader["TwoStars"]) : 0;
                        summary.OneStar = reader["OneStar"] != DBNull.Value ? Convert.ToInt32(reader["OneStar"]) : 0;
                    }
                }
            }
            return Json(summary);
        }

        [HttpGet]
        public JsonResult AddReviewTest()
        {
            return Json(new { success = true, message = "Endpoint reachable" });
        }


        [HttpPost]
        public JsonResult AddReview([FromBody] Review model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data" });

            try
            {
                using (var con = new SqlConnection(_connection))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("EXEC sp_AddReview @Name, @Stars, @ReviewText", con))
                    {
                        cmd.Parameters.AddWithValue("@Name", model.Name ?? "");
                        cmd.Parameters.AddWithValue("@Stars", model.Stars);
                        cmd.Parameters.AddWithValue("@ReviewText", model.ReviewText ?? "");

                        cmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Review added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}



