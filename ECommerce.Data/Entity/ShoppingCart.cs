using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Data.Entity
{
    public class ShoppingCart : BaseEntity
    {
        [ValidateNever]
        public Product Product { get; set; }
        public int ProductId {  get; set; }

        [Range(1,100,ErrorMessage ="Please Enter The Value between 1 and 1000")]
        public int Count {  get; set; }

        [ValidateNever]
        public Applicationuser Applicationuser { get; set; }

        public string ApplicationuserId {  get; set; }


        [NotMapped]
        public double Price { get; set; }   
    }
}
