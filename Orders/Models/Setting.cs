using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Models
{

    [Table("Setting")]
    public class Setting : IEntity
    {
        public int id { get; set; }
        public string s_name { get; set; }
        public string s_value { get; set; }
        public string s_measure { get; set; }
        public string s_descript { get; set;}
    }
}
