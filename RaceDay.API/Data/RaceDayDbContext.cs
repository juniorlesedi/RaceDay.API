
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;
using System.Collections.Generic;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<EventOrganiser> Event_Organiser { get; set; }
        public DbSet<Participant> Participant { get; set; }
        public DbSet<RaceEvent> Event { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Registration> Registration { get; set; }
        public DbSet<Result> Result { get; set; }
        public DbSet<RaceDay.API.Models.Route> Routes { get; set; }
    }
}
