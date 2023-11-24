using AspNetCore.Reporting;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using WebApp.Reports.Models;

namespace WebApp.Reports.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SqlConnectionStringBuilder _sqlConnectionStringBuilder;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _sqlConnectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = ".",
                InitialCatalog = "TestDb",
                UserID = "sa",
                Password = "sa@123"
            };
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [ActionName("export")]
        public IActionResult ExportPdf(string type)
        {
            using IDbConnection db = new SqlConnection(_sqlConnectionStringBuilder.ConnectionString);
            var lst = db.Query<BlogDataModel>("select * from tbl_blog").ToList();

            string mimetype = "";
            int extension = 1;
            var path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\BlogReport.rdlc";

            LocalReport lr = new LocalReport(path);
            lr.AddDataSource("ReportDataSet", lst);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("ReportTitle", "Blog List");

            if(type == "pdf")
            {
                ReportResult result = lr.Execute(RenderType.Pdf, extension, parameters, mimetype);
                return File(result.MainStream, "application/pdf", "BlogList.pdf");
            }
            else if(type == "excel")
            {
                ReportResult result = lr.Execute(RenderType.Excel, extension, parameters, mimetype);
                return File(result.MainStream, "application/msexcel", "BlogList.xls");
            }

            return Redirect("/Home");
        }
    }

    public class BlogDataModel
    {
        public int Blog_Id { get; set; }
        public string Blog_Title { get; set; }
        public string Blog_Author { get; set; }
        public string Blog_Content { get; set; }
    }
}
