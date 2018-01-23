using FluentTerminal.SystemTray.DataTransferObjects;
using FluentTerminal.SystemTray.Services;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace FluentTerminal.SystemTray.Controllers
{
    public class TerminalsController : ApiController
    {
        private TerminalsManager _terminalsManager;

        public TerminalsController(TerminalsManager terminalsManager)
        {
            _terminalsManager = terminalsManager ?? throw new ArgumentNullException(nameof(terminalsManager));
        }

        [HttpPost]
        [Route("terminals")]
        public string Post()
        {
            var cols = int.Parse(this.Request.GetQueryNameValuePairs().SingleOrDefault(q => q.Key == "cols").Value);
            var rows = int.Parse(this.Request.GetQueryNameValuePairs().SingleOrDefault(q => q.Key == "rows").Value);
            var options = new TerminalOptions
            {
                Columns = cols,
                Rows = rows
            };
            return _terminalsManager.CreateTerminal(options);
        }
    }
}
