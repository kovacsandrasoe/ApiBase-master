using ApiBase.Data;
using ApiBase.Models;
using ApiBase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiBase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApiDbContext _database;
        ILogger<TodoController> _logger;
        private readonly IHubContext<EventHub> _hub;

        public PersonController(UserManager<AppUser> userManager, ApiDbContext database, ILogger<TodoController> logger, IHubContext<EventHub> hub)
        {
            _userManager = userManager;
            _database = database;
            _logger = logger;
            _hub = hub;
        }


        [HttpGet]
        public JsonResult GetPersons()
        {
            return new JsonResult(_database.People);
        }


        [HttpPost]
        public async Task<JsonResult> CreatePerson([FromBody] Person person)
        {
            _database.People.Add(person);
            _database.SaveChanges();

            await _hub.Clients.All.SendAsync("PersonAdded", person);

            return new JsonResult(_database.People.FirstOrDefault(t => t.Id == person.Id));
        }

        [HttpDelete("{id}")]
        public async Task<JsonResult> DeletePerson(int id)
        {
            var personToDelete = _database.People.FirstOrDefault(t => t.Id == id);
            _database.People.Remove(personToDelete);
            _database.SaveChanges();

            await _hub.Clients.All.SendAsync("PersonDeleted", personToDelete);

            return new JsonResult(Ok());
        }

    }
}
