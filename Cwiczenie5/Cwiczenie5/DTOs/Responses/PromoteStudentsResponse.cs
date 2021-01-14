using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cwiczenie5.DTOs.Responses
{
    public class PromoteStudentsResponse
    {


     
        public int  Semester { get; set; }

        public string StartDate { get; set; }

        public string Studies { get; set; }

        public int IdEnrollment { get; set; }

        public int idStudy { get; set; }


        private string ConString;


        public string getConnectionString()
        {
            return ConString;
        }

        public void setConString(string ConString)
        {
            this.ConString = ConString;

        }

    }
}
