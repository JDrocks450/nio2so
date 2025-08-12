using Microsoft.AspNetCore.Mvc;
using nio2so.DataService.API.Databases;
using nio2so.DataService.Common.Types;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace nio2so.DataService.API.Controllers
{
    [Route("api/configure")]
    [ApiController]
    public class ConfigurationController : DataServiceControllerBase
    {
        private ConfigurationDataService dataService => GetUnderlyingDataService<ConfigurationDataService>();

        public ConfigurationController(ILogger<ConfigurationController> logger) : base() {
            var settings = dataService.GetCurrentSettings();
            if (settings.FirstRunExperience) // check for OOBE
                WelcomeWagonProgram(logger);
        }

        void WelcomeWagonProgram(ILogger<ConfigurationController> logger)
        {
            var settings = dataService.GetCurrentSettings();
            logger.LogWarning($"=========\n\nWelcome to nio2so!\nIt looks like this is your first time running your server.\n\n" +
                    $"PLEASE CHECK YOUR SETTINGS AT: \n{dataService.SettingsPath}\n\n" +
                    $"These settings will take effect whenever your server is restarted.\n" +
                    $"Changes to Data Service files (*.json) cannot be made while the server is running.\n" +
                    $"The next time you run your server, the data service will be created with default values.\n" +
                    $"Character Blob files need to be created through CAS. For now, I have given you some charblob files in a ZIP file to use -- or you can make your own people.\n" +
                    $"\n\nTHE SERVER IS NOW PAUSED." +
                    $"Please edit your settings file now. Make your changes, and then restart the server. Happy hacking! - Bisquick :)");
            settings.FirstRunExperience = false;
            try
            {
                dataService.SetCurrentSettings(settings).Wait();
            }
            catch (Exception ex)
            {
                logger.LogError($"Writing your settings file: \n{ex}");
            }
            //wait forever
            while (true)
            {
                Task.Delay(100000);
            }
        }

        // GET api/<ConfigurationController>/settings
        [HttpGet("settings")]
        public ActionResult<ServerSettings> GetServiceSettings() => dataService.GetCurrentSettings();

        // POST api/<ConfigurationController>/settings
        [HttpPost("settings")]
        public async Task<ActionResult> PostServiceSettings([FromBody] ServerSettings NewSettings)
        {
            try
            {
                await dataService.SetCurrentSettings(NewSettings);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("status")]
        public ActionResult GetStatus() => Ok();

        // GET api/<ConfigurationController>/settings/voltron
        [HttpGet("settings/voltron")]
        public ActionResult<VoltronServerSettings> GetVoltronSettings() => dataService.GetCurrentSettings().VoltronSettings;
    }
}
