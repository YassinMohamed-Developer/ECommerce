using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Data.Entity
{
    public class OrderDetail : BaseEntity
    {
        public OrderHeader OrderHeader { get; set; }
        [Required]
        public int OrderHeaderId { get; set; }



        public Product Product { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int Count { get; set; }
        public double Price { get; set; }
    }
}
