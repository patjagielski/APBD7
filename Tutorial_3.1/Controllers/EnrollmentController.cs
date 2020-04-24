using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutorial_3._1.Models;
using Tutorial_3._1.Models.Enrollment;
using Tutorial_3._1.Models.Services;

namespace Tutorial_3._1.Controllers
{
    
    [ApiController]
    [Route("api/enrollments")]
    [Authorize(Roles = "employee")]
    
    public class EnrollmentController:ControllerBase
    {
        private IStudentsDbService _service;
        public EnrollmentController(IStudentsDbService service)
        {
            _service = service;
        }
        [HttpPost]
        public IActionResult EnrollStudent(EnrollmentRequest request)
        {
           EnrollmentRequest helpme = _service.EnrollStudent(request);
            if(helpme == null)
            {
                return BadRequest();
            }
            else
            {
                return Ok(helpme);
            }
            
            
        }

        [HttpPost("promotions")]
        public IActionResult PromoteStudent(PromotionRequests requests)
        {
            PromotionRequests response = _service.PromoteStudent(requests);
            if(response != null)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest();
            }
            
        }



    }
}
