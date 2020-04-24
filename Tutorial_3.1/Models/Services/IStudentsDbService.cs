using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tutorial_3._1.Models.Enrollment;

namespace Tutorial_3._1.Models.Services
{
    public interface IStudentsDbService
    {
        EnrollmentRequest EnrollStudent(EnrollmentRequest request);
        PromotionRequests PromoteStudent(PromotionRequests request);
        bool CheckIndex(string index);

    }
}
