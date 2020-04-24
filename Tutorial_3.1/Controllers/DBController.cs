using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tutorial_3._1.Models;
using Tutorial_3._1.Models.Enrollment;
using Tutorial_3._1.Models.Services;
namespace Tutorial_3._1.Controllers
{
    
    public class DBController : IStudentsDbService
    {
        public bool CheckIndex(string index)
        {
            using (var client  = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
            {
                
                using (var con = new SqlCommand())
                {
                    client.Open();
                    con.Connection = client;
                    con.CommandText = "select IndexNumber from Student where IndexNumber = @index;";
                    con.Parameters.AddWithValue("index", index);

                    var reader = con.ExecuteReader();
                    if(reader.Read())
                    {
                        //returns true if the index number exists
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                   
                }
                
            }
        }

        public EnrollmentRequest EnrollStudent(EnrollmentRequest request)
        {
            using (var sqlConnection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
            {
                sqlConnection.Open();
                SqlTransaction transaction;
                using (var command = new SqlCommand())
                {
                    
                    transaction = sqlConnection.BeginTransaction();
                    command.Connection = sqlConnection;
                    command.Transaction = transaction;
                    /*_service.EnrollStudent(request);*/
                    if (request.NullEmptyCheck())
                    {
                        var StudyNames = "";
                        var check = false;
                        command.Connection = sqlConnection;
                        string query = "select Name " +
                                       "from Studies";
                        command.CommandText = (query);
                       
                        var response = command.ExecuteReader();
                        while (response.Read())
                        {
                            StudyNames = StudyNames + $"{response["Name"].ToString()}";
                        }

                        
                        if (StudyNames.Contains(request.Studies))
                        {
                            check = true;
                        }
                        else
                        {
                            check = false;
                        }
                    

                        if (check)
                        {
                            var index = 0;
                            var checkId = false;
                            query = "select count(IndexNumber)" +
                                           "from Studies" +
                                           $"where IndexNumber = '{request.IndexNumber}'";
                            command.CommandText = (query);
                            
                            
                            while (response.Read())
                            {
                                index = int.Parse($"{response["IndexNumber"]}");
                            }
                            if (index >= 1)
                            {
                                checkId = false;
                            }
                            else
                            {
                                checkId = true;
                            }

                            //Check if index number is unique
                            //If record doesn't exist then create new 
                            if (checkId)
                            {
                                var Semestercount = 0;

                                var student = new Student();
                                student.IndexNumber = request.IndexNumber;
                                student.FirstName = request.FirstName;
                                student.LastName = request.LastName;
                                student.Studies = request.Studies;
                                student.BirthDate = request.Birthdate;
                                //student.Semester = CheckSemester(request.Studies);
                                
                               
                                query = "select count(b.IdStudy) " +
                                               "from Studies b join Enrollment a " +
                                               $"on a.IdStudy = b.IdStudy where b.Name = '{request.Studies}'";
                                command.CommandText = (query);
                                while (response.Read())
                                {
                                    Semestercount = int.Parse($"{response["count(IdStudy)"]}");
                                }

                                var Semester = "";
                                if (Semestercount > 0)
                                {
                                    
                                    
                                    query = "select b.IdStudy " +
                                                   "from Studies b join Enrollment a " +
                                                   $"on a.IdStudy = b.IdStudy where b.Name = '{request.Studies}'" +
                                                   " and a.Semester = 1";
                                    command.CommandText = (query);
                                    
                                    while (response.Read())
                                    {
                                        Semester = $"{response["count(IdStudy)"]}";
                                    }
                                }
                                else
                                {
                                    //rollback here
                                    query = "insert into Enrollment(IdEnrollment, Semester, IdStudy, StartDate) values " +
                                                  "((max(IdEnrollment)+1), 1," +
                                                  $" (Select IdStudy from Studies where Name = '{request.Studies}'), GetDate())";
                                    command.CommandText = (query);
                                    
                                   
                                }
                                


                                return request;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                    

                }
                try
                {
                    transaction.Commit();
                }
                finally
                {
                    transaction.Rollback();
                }

            }
        }

       /* public bool checkIndex(EnrollmentRequest request)
        {
            var index = 0;
            using (var sqlConnection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                   
                    command.Connection = sqlConnection;
                    string query = "select count(IndexNumber)" +
                                   "from Studies" +
                                   $"where IndexNumber = '{request.IndexNumber}'";
                    command.CommandText = (query);
                    sqlConnection.Open();
                    var response = command.ExecuteReader();
                    while (response.Read())
                    {
                        index =int.Parse($"{response["IndexNumber"]}");
                    }
                    if(index >= 1)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
        public bool checkStudies(EnrollmentRequest studies)
        {
            var StudyNames = " ";
            using (var sqlConnection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = sqlConnection;
                    string query = "select Name " +
                                   "from Studies";
                    command.CommandText = (query);
                    sqlConnection.Open();
                    var response = command.ExecuteReader();
                    while (response.Read())
                    {
                        StudyNames = StudyNames + $"{response["Name"].ToString()}";
                    }
                }
                sqlConnection.Close();
                if (StudyNames.Contains(studies.Studies))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public int CheckSemester(string request)
        {
            var Semester = "";
            using (var sqlConnection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    sqlConnection.Open();
                    command.Connection = sqlConnection;
                    string query = "select b.IdStudy " +
                                   "from Studies b join Enrollment a " +
                                   $"on a.IdStudy = b.IdStudy where b.Name = '{request}'";
                    command.CommandText = (query);
                    var response = command.ExecuteReader();
                    while (response.Read())
                    {
                        Semester = $"{response["IdStudy"]}";
                    }
                    sqlConnection.Close();
                    return int.Parse(Semester);
                }


            }

        }*/

        public PromotionRequests PromoteStudent(PromotionRequests request)
        {
            if (request.Check())
            {

                using (var sqlConnection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19696;Integrated Security=True"))
                {
                    sqlConnection.Open();
                    SqlTransaction transaction;
                    using (var command = new SqlCommand("Promotions", sqlConnection))
                    {
                        transaction = sqlConnection.BeginTransaction();
                        command.Connection = sqlConnection;
                        command.Transaction = transaction;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@Studies", request.Studies));
                        command.Parameters.Add(new SqlParameter("@Semester", request.Semester));
                        
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        
                    }
                    sqlConnection.Close();
                    
                    return request;
                }
            }
            else
            {
                return null;
            }

        }


    }
}
