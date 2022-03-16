using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Web.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ErrorMessageController : Controller
    {
        private readonly IStringLocalizer<ErrorMessageController> _localizer;

        public ErrorMessageController(IStringLocalizer<ErrorMessageController> localizer)
        {
            _localizer = localizer;
        }

        public string TaskNotSelected()
        {
            return _localizer["Task not selected"];
        }

        public string TaskNotLeaf()
        {
            return _localizer["Task not leaf"];
        }
    }
}