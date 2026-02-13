using Microsoft.AspNetCore.Mvc;
using Service;

namespace WebApiCommercial.Controllers
{
    public class BankAccountController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        public BankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
