﻿using System;

namespace MyHotel.Models
{
    public class GuestModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime RegisterDate { get; set; }

        public int? NullableInt { get; set; }
    }
}
