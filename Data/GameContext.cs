﻿using Microsoft.EntityFrameworkCore;
using TournamentFighter.Models;

namespace TournamentFighter.Data
{
    public class GameContext: DbContext
    {
        public GameContext(DbContextOptions options): base(options) { }

        public DbSet<Move> Moves { get; set; }
        public DbSet<Character> Characters { get; set; }
    }
}
