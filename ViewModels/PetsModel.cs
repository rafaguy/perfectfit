using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    public class PetsModel
    {
        public int petIndex { get; set; }

        public string petType { get; set; }

        //public string PetPhotoUrl { get; set; }

        public string petName { get; set; }

        //Male = 2; Female = 1
        public string petGender { get; set; }

        public int petBirthDay { get; set; }

        public int petBirthMonth { get; set; }

        public int petBirthYear { get; set; }

        public string petBirthDate { get; set; } 
       public string petBreed { get; set; }
        public int petAloneSitter { get; set; }

        // yes = 1, no = 0
        public string isProductTriedBefore { get; set; }

        public string isPetSterilized { get; set; }

        public int PetBreedId { get; set; }

        //wet = 1, dry = 2, mixed = 3, unknown = 0
        public int PetFood { get; set; }

        //For CAT
        public string petSpendTime { get; set; }

        //For CAT ; short = 1, medium = 3, long = 2
        public int petCoat { get; set; }

        //For DOG
        public int petWeigh { get; set; }

        //For DOG, low = 1, normal = 2, high = 3
        public int petActive { get; set; }

        public string petPhotos { get; set; }

        public int petId { get; set; }

        public int petAgeIndex { get; set; }

        //Photo index
       // public int petLadderAge { get; set; }

    }
}