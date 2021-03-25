using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OldMusicBox.EIH.ServerDemo.Models
{
    public class AccountLogonModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string GivenName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string PESEL { get; set; }
    }
}