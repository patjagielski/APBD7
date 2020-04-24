using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tutorial_3._1.Models;
using Microsoft.Extensions.Configuration;
using Tutorial_3._1.DTO;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Tutorial_3._1.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public StudentController(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        //APBD7
        [HttpPost]
        public IActionResult Login(LoginRequestDTO loginRequestDTO)
        {
            var salt = CreateSalt();
            var password = CreateHash(loginRequestDTO.password, salt);
            var flag = true;
            var answer = 0;
            using (var sqlConnection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = sqlConnection;
                    command.CommandText = $"Select count(IndexNumber) as counting from Student where IndexNumber = '{loginRequestDTO.login}'" +
                                          $" and StudentPassword = '{loginRequestDTO.password}';";

                    sqlConnection.Open();
                    var response = command.ExecuteReader();
                    while (response.Read())
                    {
                        answer = Convert.ToInt32(response["counting"]);
                        if (answer > 0)
                        {
                            flag = true;

                        }
                        else
                        {
                            flag = false;
                            return Ok("Nothing here");

                        }
                    }
                    sqlConnection.Close();
                }
            }

            if (flag)
            {

                var claims = new[]
                {
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(ClaimTypes.Name, "bob123"),
                        new Claim(ClaimTypes.Role, "admin"),
                        new Claim(ClaimTypes.Role, "employee"),
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Secrekey"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken
                    (
                    issuer: "Gakko",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: credentials
                    );
                var token1 = new JwtSecurityTokenHandler().WriteToken(token);
                var refreshToken1 = Guid.NewGuid();
                Console.WriteLine(token1);
                Console.WriteLine(refreshToken1);
                InsertToken(refreshToken1.ToString(), loginRequestDTO.password, loginRequestDTO.login, password);


                return Ok(new
                {
                    token1,
                    refreshToken1
                });
            }
            else
            {
                return NotFound();
            }

        }
        public void InsertToken(string token, String password, String login, string hashedpassword)
        {
            using (var sqlConnection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    sqlConnection.Open();
                    SqlTransaction transaction;
                    transaction = sqlConnection.BeginTransaction();
                    command.Connection = sqlConnection;
                    command.Transaction = transaction;
                    command.Connection = sqlConnection;
                    command.CommandText = $"Update Student set refreshToken = '{token}' " +
                                    $"where StudentPassword = '{password}' and IndexNumber = '{login}'";

                    
                    var response = command.ExecuteReader();

                    
                }
                using (var command = new SqlCommand())
                {
                    
                    command.CommandText = $"Update Student set StudentPassword = '{hashedpassword}' " +
                                    $"where refreshToken = '{token}' and IndexNumber = '{login}';";
                    transaction.Commit();
                    var response = command.ExecuteReader();

                    sqlConnection.Close();
                }
            }
        }

        //Task2
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken(LoginRequestDTO loginRequestDTO)
        {

            var refreshTokendb = "";
            using (var sqlConnection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = sqlConnection;
                    sqlConnection.Open();
                    command.CommandText = $"Select '{loginRequestDTO.refreshToken}' as token from Student where IndexNumber = '{loginRequestDTO.login}';";

                    var response = command.ExecuteReader();
                    while (response.Read())
                    {
                        refreshTokendb = response["token"].ToString();
                    }

                }
                sqlConnection.Close();
            }
            var claims = new[]
           {
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(ClaimTypes.Name, "bob123"),
                        new Claim(ClaimTypes.Role, "admin"),
                        new Claim(ClaimTypes.Role, "employee")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Secrekey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
                (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials
                );

            var token1 = new JwtSecurityTokenHandler().WriteToken(token);
            if (refreshTokendb == loginRequestDTO.refreshToken)
            {
                return Ok(new
                {
                    token1

                });
            }
            else
            {
                return BadRequest("Such token is not in the database");
            }
        }

        public string CreateHash(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                password: value,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 20000,
                numBytesRequested: 256 / 8
                );
            return Convert.ToBase64String(valueBytes);
        }

        public static string CreateSalt()
        {
            byte[] randomBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
        [HttpGet]
        public IActionResult CreateStudent()
        {
            
            var students = new List<Student>();
            using (var sqlConnection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = sqlConnection;
                    string query = "select s.FirstName, s.LastName, s.IndexNumber, s.Birthdate, st.Name as Studies, e.Semester," +
                                " from Student s join Enrollment e " +
                                " on e.IdEnrollment = s.IdEnrollment join Studies st" +
                                " on st.IdStudy = e.IdStudy;";
                    command.CommandText = (query);
                    sqlConnection.Open();
                    var response = command.ExecuteReader();
                    while (response.Read())
                    {
                        var st = new Student();
                        st.FirstName = response["FirstName"].ToString();
                        st.LastName = response["LastName"].ToString();
                        st.Studies = response["Studies"].ToString();
                        st.IndexNumber = response["IndexNumber"].ToString();
                        st.BirthDate = DateTime.Parse(response["Birthdate"].ToString());
                        st.Semester = int.Parse(response["Semester"].ToString());

                        students.Add(st);
                    }
                }
                return Ok(students);
            }
        }
        [HttpGet("{id}")]
        public IActionResult GetStudent(string id)
        {
            var studies = new Studies();
            var studyList = new List<Studies>();
            using (var client = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
            {
                using (var con = new SqlCommand("StudentsForStudies", client) {CommandType = System.Data.CommandType.StoredProcedure})
                {
                    con.Parameters.Add(new SqlParameter("@Studies", studies));
                    con.Parameters.Add(new SqlParameter("@Studies", studies));
                    client.Open();
                    con.Connection = client;
                    con.CommandText = "select a.IndexNumber, a.FirstName, a.LastName,b.semester,c.Name" +
                                " from Student a join Enrollment b" +
                                " on a.IdEnrollment = b.IdEnrollment join Studies c" +
                                " on c.idStudy = b.IdStudy where a.IndexNumber = @id;";
                    con.Parameters.AddWithValue("id", id);
                    
                    var reader = con.ExecuteReader();
                    while (reader.Read())
                    {
                        studies.FirstName = reader["FirstName"].ToString();
                        studies.LastName = reader["LastName"].ToString();
                        studies.IndexNumber = reader["IndexNumber"].ToString();
                        studies.Semester = reader["Semester"].ToString();
                        studies.Study = reader["Study"].ToString();
                        studyList.Add(studies);
                    }

                }

            }
            return Ok(studyList);

        }

        [HttpDelete]
        public IActionResult DeleteStudent()
        {
            string temp = "";
            using (var client = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
            {
                
                using(var command = new SqlCommand())
                {
                    command.Connection = client;
                    string query = "Delete Student;";
                    command.CommandText = (query);
                    client.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        temp = $"{ reader["Student"]}";
                    }
                }

            }
            return Ok("Table Student Deleted");

        }

       




       
      

    }
}
