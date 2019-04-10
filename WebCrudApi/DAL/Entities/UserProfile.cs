using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrudApi.DAL.Entities
{
    [Table("tblUserProfiles")]
    public class UserProfile
    {
        [Key, ForeignKey("User")]
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Salary { get; set; }
        public int Age { get; set; }
        public virtual DbUser User { get; set; }
    }
}
