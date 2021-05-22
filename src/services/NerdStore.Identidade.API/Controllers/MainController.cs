using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NerdStore.Identidade.API.Controllers
{
    [ApiController]
    public class MainController : ControllerBase
    {
        protected ICollection<string> Errors = new List<string>();

        protected ActionResult CustomResponse(object result = null)
        {
            if(this.ValidOperation())
            {
                return Ok(result);
            }

            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                { "Messages", this.Errors.ToArray() }
            }));
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            IEnumerable<ModelError> errors = modelState.Values.SelectMany(e => e.Errors);

            foreach (var error in errors)
            {
                this.AddError(error.ErrorMessage);
            }

            return this.CustomResponse();
        }

        private bool ValidOperation()
        {
            return this.Errors.Any();
        }

        protected void AddError(string error)
        {
            this.Errors.Add(error);
        }
    }
}
