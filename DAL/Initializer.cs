﻿using PROJEKT_PZ_NK_v3.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace PROJEKT_PZ_NK_v3.DAL
{
    public class Initializer : DropCreateDatabaseIfModelChanges<OfferContext>
    {
        protected override void Seed(OfferContext context)
        {
            /*List<Animal> animals = new List<Animal>
            {
                new Animal {Name = "Meguniuniunia", Species = "Pies", Race ="Zmieszany mieszaniec przypomnijący krótkowłosego Owczarka",
                    Gender = "Kobietka", DateOfBirth = new DateTime(2015, 12, 25),
                    Description = "Cudowny Pies Meguniuniunia, bardzo lubi inne psy ale nie przepada za ludźmi i kotami."},
                new Animal {Name = "Figa", Species = "Kot", Race = "Długowłosy mieszaniec",
                    Gender = "Kobietka", DateOfBirth = new DateTime(2020, 12, 05),
                    Description = "Super pilnuje domu podczas nieobecności właścicieli. Lubi ludzi ale tylko tych, których odpowienio długo będzie obserwować i oczywiście obwochą czy aby na pewno nie są zagrożeniem"},
                new Animal {Name = "Puszka", Species = "Pies", Race ="Ratler",
                    Gender = "Kobietka", DateOfBirth =  new DateTime(2010, 06, 12),
                    Description = "Zdystansowana i bardzo posłuszna ale tylko swojej właścicielce."},
                new Animal {Name = "Kot", Species = "Kot", Race ="Mieszaniec",
                    Gender = "Kocur", DateOfBirth =  new DateTime(2021, 07, 12),
                    Description = "Okropna przylepa, przedewszystkim uwielbia głaskanie ale nie pogardzi również dobrą zabawą."}
            };
            context.Animals.AddRange(animals);

            List<Profile> profiles = new List<Profile>
            {
                new Profile{FirstName = "Natalia", LastName = "Koć", Login = "Tallusza",
                    Email = "tallusza@o2.pl",
                    Address = "Siedlce 2", PhoneNumber ="793871271",
                    Animals = new List<Animal>{animals[0]} },
                new Profile{FirstName = "Daria", LastName = "Sitkowska", Login = "Darcia",
                    Email = "darcia@o2.pl", 
                    Address = "Warszawa 5", PhoneNumber ="756852943",
                    Animals = new List<Animal>{animals[1]} },
                new Profile{FirstName = "Zofia", LastName = "Biernacka", Login = "Zosia",
                    Email = "zosia@o2.pl", 
                    Address = "Siedlce 8", PhoneNumber ="726159423",
                    Animals = new List<Animal>{animals[2], animals[3]} }
            };
            context.Profiles.AddRange(profiles);

            List<Offer> offers = new List<Offer>
            {
                new Offer {Title = "Wyprowadzanie psa", Description = "Szukam osoby, która chciałaby wyprowadzić mojego psa na spacer.",
                    Price = 30.00f, StartingDate = "05 lipca 2021 17:00", EndDate = "05 lipca 2021 19:00",
                    Animal = animals[0], Profile = profiles[0]},
                new Offer {Title = "Opieka nad kotem", Description = "Szukam osoby, która chciałaby przygarnąć do siebie mojego kota i zaopiekować się nim na kilka dni.",
                    Price = 100.00f, StartingDate = "05 lipca 2021 7:00", EndDate = "08 lipca 2021 20:00",
                    Animal = animals[1], Profile = profiles[1]},
                new Offer {Title = "Opieka nad kotem", Description = "Szukam osoby, która chciałaby przygarnąć do siebie mojego kota i zaopiekować się nim na kilka dni.",
                    Price = 150.00f, StartingDate = "20 lipca 2021 7:00", EndDate = "25 lipca 2021 20:00",
                    Animal = animals[1], Profile = profiles[1]}
            };
            context.Offers.AddRange(offers);

            context.SaveChanges();*/
        }
    }
}