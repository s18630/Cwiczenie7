using Cwiczenie5.DTOs;
using Cwiczenie5.DTOs.Requests;
using Cwiczenie5.DTOs.Responses;
using Cwiczenie5.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cwiczenie5.Services
{
    public class SqlServerStudentDbService : IStudentsDbService
    {

        private const string ConString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True";

        public bool isLogOk(LoginRequestDto request)
        {
           
            if(request.Haslo == null || request.NumerIndexu == null)
            {
                
               
                return false;
                
            }


            try
            {
                using (SqlConnection con = new SqlConnection(ConString))
                using (SqlCommand com = new SqlCommand())
                {
                    com.Connection = con;

                    com.CommandText = " SELECT IndexNumber from Student where IndexNumber =@index ";
                    com.Parameters.AddWithValue("index", request.NumerIndexu);

                    con.Open();
                    SqlDataReader dr = com.ExecuteReader();

                    if (!dr.Read())
                    {
                        return false;
                    }

                    dr.Close();

                }


                    using (SqlConnection con = new SqlConnection(ConString))
                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                  

            
                       com.CommandText = "SELECT Password FROM Logins WHERE Password = HASHBYTES('SHA2_512',@haslo) and IndexNumber=@index";
                    con.Open();

                    com.Parameters.AddWithValue("haslo", request.Haslo);
                   com.Parameters.AddWithValue("index", request.NumerIndexu);
                   


                    SqlDataReader dr = com.ExecuteReader();

                        if (!dr.Read())
                        {
                        return false;
                    }



                    dr.Close();

                }
                return true;


            }
            catch (Exception)
            {
                throw new Exception();
            }





        }

        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {
            var student = new Student();
            student.IndexNumber = request.IndexNumber;
            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.BirthDate = request.BirthDate;

            var response = new EnrollStudentResponse();


            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;

                com.CommandText = " select IdStudy from Studies where Name Like @name ";
                com.Parameters.AddWithValue("name", request.Studies);

                con.Open();
                var transacion = con.BeginTransaction();
                int IdEnrollment = 1;
                try
                {


                    com.Transaction = transacion;

                    SqlDataReader dr = com.ExecuteReader();
                    if (!dr.Read())
                    {
                        dr.Close();
                        transacion.Rollback();
                        //   return BadRequest("Studia nie istnieją");
                        throw new Exception("Studia nie istnieją");
                    }

                    int IdStudy = (int)dr["IdStudy"];
                    dr.Close();

                    com.CommandText = "Select IdEnrollment from Enrollment where Semester=1 and IdStudy= (select IdStudy from Studies where Name Like @name) order by StartDate DESC ";
                    com.Transaction = transacion;

                    dr = com.ExecuteReader();
                    //     int IdEnrollment = 1;

                    if (dr.Read())
                    {
                        IdEnrollment = (int)dr["IdEnrollment"];
                        dr.Close();

                    }
                    else if (!dr.Read())
                    {
                        dr.Close();
                        com.CommandText = "select IdEnrollment from Enrollment where IdEnrollment =(select max(IdEnrollment) from Enrollment )";
                        dr = com.ExecuteReader();
                        if (dr.Read())
                        {
                            int IdEnroll = (int)dr["IdEnrollment"];
                            IdEnrollment = IdEnroll + 1;
                        }

                        dr.Close();

                        com.CommandText = "INSERT INTO Enrollment(IdEnrollment, Semester, IdStudy, StartDate) VALUES (@IdEnrollment , 1, @IdStudy,  convert(datetime, @StartDate));";

                        com.Parameters.AddWithValue("IdStudy", IdStudy);
                        DateTime thisDay = DateTime.Today;
                        com.Parameters.AddWithValue("StartDate", thisDay);
                        com.Parameters.AddWithValue("IdEnrollment", IdEnrollment);


                        com.ExecuteNonQuery();
                        transacion.Commit();

                    }

                    com.CommandText = "Select IndexNumber from Student where IndexNumber =@IndexNumber";
                    com.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                    com.Transaction = transacion;

                    dr = com.ExecuteReader();


                    if (dr.Read())
                    {
                        dr.Close();
                        transacion.Rollback();
                        throw new Exception("Student o tym ID już istnieje");
                    }
                    else if (!dr.Read())
                    {
                        dr.Close();
                        com.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES(@IndexNumber, @FirstName , @LastName, convert(datetime,@BirthDate), @IdEnrollment)";
                        com.Parameters.AddWithValue("FirstName", student.FirstName);
                        com.Parameters.AddWithValue("LastName", student.LastName);
                        com.Parameters.AddWithValue("BirthDate", student.BirthDate);
                        com.Parameters.AddWithValue("IdEnrollment", IdEnrollment);

                        com.ExecuteNonQuery();
                    }

                    transacion.Commit();



                    com.CommandText = "SELECT StartDate from Enrollment where IdEnrollment=@IdEnrollment";

                    dr = com.ExecuteReader();
                    if (dr.Read())
                    {
                        response.StartDate = dr["StartDate"].ToString();
                    }
                    dr.Close();



                }
                catch (Exception ex)
                {
                    transacion.Rollback();
                    throw new Exception("Wystąpił błąd");
                }


            }

            response.IndexNumber = student.IndexNumber;
            response.Semester = "1";
            response.Studies = request.Studies;
            response.setConString(ConString);
           

            return  response;
        }




        

      


        public PromoteStudentsResponse PromoteStudents(PromoteStudentsRequest request)
        {
            PromoteStudentsResponse response = new PromoteStudentsResponse();

            
                using (SqlConnection con = new SqlConnection(ConString))
                using (SqlCommand com = new SqlCommand())
                {
                    com.Connection = con;

                    con.Open();
                    com.CommandText = "PromoteStudents";

                    com.CommandType = System.Data.CommandType.StoredProcedure;
                    com.Parameters.AddWithValue("@Studies", SqlDbType.NVarChar).Value = request.Studies;
                    com.Parameters.AddWithValue("@Semester", SqlDbType.Int).Value = request.Semester;

                    com.ExecuteNonQuery();
                //pokazuje sie bład zrobic coś gdu wiecej niż jedna wartość



                    con.Close();
                }

            



            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;

                con.Open();

                com.CommandText = " Select * from Enrollment where Semester =@NewSemester and IdStudy = (select IdStudy from Studies where Name =@Studies) " +
                    "and StartDate = (select max(StartDate) from Enrollment where Semester =@NewSemester and IdStudy = (select IdStudy from Studies where Name =@Studies))";

                com.Parameters.AddWithValue("NewSemester", request.Semester + 1);
                com.Parameters.AddWithValue("Studies", request.Studies);
               


              
                SqlDataReader dr = com.ExecuteReader();
               

                if (dr.Read())
                {
                    response.IdEnrollment = (int)dr["IdEnrollment"];
                    response.Semester = (int)dr["Semester"];
                    response.idStudy = (int)dr["IdStudy"];
                    response.StartDate = dr["StartDate"].ToString();


                    response.Studies = request.Studies;


                }
                else
                {
                    dr.Close();
                    throw new Exception();
                }

                dr.Close();




                con.Close();
            }
            


                response.setConString(ConString);

                //          Select* from Enrollment where Semester = 9 and IdStudy = 3 and StartDate = (select max(StartDate) from Enrollment where Semester = 9 and IdStudy = 3);

                //wyszukaj dodany właśńie semestr i zwróc obiekty

                return response;
            }





        public Student GetStudent(string index)
        {

            var student = new Student();


            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;

                com.CommandText = " SELECT FirstName, LastName, BirthDate  from Student where IndexNumber =@index ";
                com.Parameters.AddWithValue("index", index);

                con.Open();
                SqlDataReader dr = com.ExecuteReader();

                if (!dr.Read())
                {
                    return null;
                }


                student.IndexNumber = index;
                student.FirstName = dr["FirstName"].ToString();
                student.LastName = dr["LastName"].ToString();
                student.BirthDate = dr["BirthDate"].ToString();

                dr.Close();

                return student;

            };
        }

        public IEnumerable<Student> GetStudents()
        {
            var students = new List<Student>();



            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;

                com.CommandText = " SELECT IndexNumber ,FirstName, LastName, BirthDate  from Student";


                con.Open();
                SqlDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    var student = new Student();
                    student.IndexNumber = dr["IndexNumber"].ToString();
                    student.FirstName = dr["FirstName"].ToString();
                    student.LastName = dr["LastName"].ToString();
                    student.BirthDate = dr["BirthDate"].ToString();

                    students.Add(student);

                }

                return students;
            };

        }

        
    }
}

