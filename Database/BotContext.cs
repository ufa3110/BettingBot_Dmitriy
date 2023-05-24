using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingBot.Database
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;

    public class BotContext : DbContext
    {
        public DbSet<Bet> Bets { get; set; }
        public DbSet<BK> BKs { get; set; }

        public string DbPath { get; }

        public BotContext(DbContextOptions<BotContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=bot;Username=postgres;Password=password");
        }
    }

    public class Bet
    {
        /// <summary>
        /// Идентификатор записи.
        /// </summary>
        public int BetId { get; set; }

        /// <summary>
        /// Название спорта ставки.
        /// </summary>
        public string Sport { get; set; }

        /// <summary>
        /// Названия команд.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Список Букмекерских контор с исходами и коэффициентами.
        /// </summary>
        public List<BK> Bks { get; set; }
    }

    public class BK
    {
        /// <summary>
        /// Идентификатор записи.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название Букмекерской конторы.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Исход матча (ставка).
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Коэффициент ставки.
        /// </summary>
        public string Coefficient { get; set; }
    }
}
