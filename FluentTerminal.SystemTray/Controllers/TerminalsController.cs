using FluentTerminal.Models;
using FluentTerminal.SystemTray.Services;
using System;
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
        [Route("terminals/{id}/resize")]
        public void Resize(int id, [FromBody]TerminalSize size)
        {
            _terminalsManager.ResizeTerminal(id, size);
        }

        [HttpPost]
        [Route("terminals")]
        public CreateTerminalResponse Create([FromBody]CreateTerminalRequest request)
        {
            return _terminalsManager.CreateTerminal(request);
        }
    }
}
