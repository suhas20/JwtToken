using System.ComponentModel.DataAnnotations;

namespace JwtToken4.Model
{
    public class Student
    {
        [Key]
        public int Roolno { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
    }
}
