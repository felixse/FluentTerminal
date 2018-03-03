using FluentTerminal.Models;
using FluentTerminal.SystemTray.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace FluentTerminal.SystemTray.Controllers
{
    public class KeyBindingsController : ApiController
    {
        private readonly ToggleWindowService _toggleWindowService;

        public KeyBindingsController(ToggleWindowService toggleWindowService)
        {
            _toggleWindowService = toggleWindowService;
        }

        [HttpPost]
        [Route("keybindings/togglewindow")]
        public void PostToggleWindowKeyBindings([FromBody]IEnumerable<KeyBinding> keyBindings)
        {
            _toggleWindowService.SetHotKeys(keyBindings);
        }
    }
}
