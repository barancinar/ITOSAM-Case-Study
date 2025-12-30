using ItoCase.Core.DTOs;
using ItoCase.Core.Interfaces;
using ItoCase.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ItoCase.Web.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ChartService _chartService;

        public BookController(IBookService bookService, ChartService chartService)
        {
            _bookService = bookService;
            _chartService = chartService;
        }

        // 1. Sayfayı Gösteren Metot (GET)
        public IActionResult Index()
        {
            return View();
        }

        // 2. Dosya Yüklemeyi Karşılayan Metot (POST)
        [HttpPost]
        [Authorize(Roles = "Admin,Uzman")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Lütfen bir Excel dosyası seçiniz.";
                ViewBag.Status = "error";
                return View("Index");
            }

            try
            {
                // Dosyayı geçici bir yere (Temp) kaydedelim
                var tempFilePath = Path.GetTempFileName();

                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Servisi çağırıp işi ona yıkıyoruz (Controller iş yapmaz!)
                await _bookService.ImportExcelToDatabaseAsync(tempFilePath);

                ViewBag.Message = "Harika! Excel verileri başarıyla veritabanına aktarıldı.";
                ViewBag.Status = "success";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Hata oluştu: {ex.Message}";
                ViewBag.Status = "error";
            }

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> GetBooksAjax([FromForm] DataTableRequestDto request)
        {
            // DataTables parametreleri Form verisi olarak geliyor
            var result = await _bookService.GetBooksForDataTableAsync(request);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string strategy)
        {
            var data = await _chartService.GetDataByStrategyAsync(strategy);
            return Json(data);
        }
    }
}