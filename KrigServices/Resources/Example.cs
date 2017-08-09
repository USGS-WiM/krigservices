using System;
using System.ComponentModel.DataAnnotations;
namespace KrigServices.Resources
{
    public class Example
    {
        [Required]
        public string prop1 { get; set; }
        [Required]
        public DateTime prop2 { get; set; }
        [Required]
        public double prop3 { get; set; }
        [Required]
        public int prop4 { get; set; }
    }
}
