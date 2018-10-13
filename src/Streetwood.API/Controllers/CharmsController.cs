﻿using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Streetwood.Infrastructure.Commands.Models;
using Streetwood.Infrastructure.Queries.Models.Charm;

namespace Streetwood.API.Controllers
{
    [Route("api/CharmsCategories/{id}/charms/")]
    [ApiController]
    public class CharmsController : ControllerBase
    {
        private readonly IMediator mediator;

        public CharmsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
            => Ok(await mediator.Send(new GetCharmsByCategoryIdQueryModel(id)));

        [HttpPost]
        public async Task<IActionResult> Post(Guid id, [FromBody] AddCharmCommandModel model)
        {
            await mediator.Send(model.AddCategoryId(id));
            return Accepted();
        }
    }
}