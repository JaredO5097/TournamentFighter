﻿namespace TournamentFighter.Models
{
    public readonly record struct Move(string Name, string Message, int BaseDamage, int BaseAccuracy, int Priority, 
        Status Status = Status.None)
    {
        public static readonly Move None = new("None", "isn't doing anything? Interesting...", 0, 0, Priority: 0);

        public static readonly Move LongShot = new("Long Shot", "fired an arrow from afar!", 90, 95, Priority: 0, Status.Bleed);
        public static readonly Move Punch = new("Punch", "threw a hard punch!", 70, 100, Priority: 0);
        public static readonly Move SwordSlash = new("Sword Slash", "sliced the air in two!", 85, 100, Priority: 0, Status.Bleed);
        public static readonly Move AxeSwing = new("Axe Swing", "cleaved the air with their axe!", 90, 95, Priority: -1);
        public static readonly Move RoaringMoon = new("Roaring Moon", "spun their axe then attacked!", 95, 80, Priority: 0, 
            Status.Immobile);
        public static readonly Move JumpKick = new("Jump Kick", "brought down a strong kick!", 80, 100, Priority: 1);
        public static readonly Move ShortShot = new("Short Shot", "quickly fired an arrow!", 70, 95, Priority: 1);
        public static readonly Move Counter = new("Counter", "dodged then attacked!", 100, 100, Priority: -1);

        public static readonly Move IceSpike = new("Ice Spike", "raised a massive ice spike from below!", 80, 90, Priority: 0,
            Status.Immobile);
        public static readonly Move FlameImplosion = new("Flame Implosion", "collapsed a sphere of fire!", 90, 80, Priority: 0,
            Status.Burn);
        public static readonly Move ThousandCuts = new("Thousand Cuts", "summoned a fierce sandstorm!", 65, 100, Priority: 0,
            Status.Bleed);
        public static readonly Move CursedExcalibur = new("Cursed Excalibur", "summoned a mighty sword!", 95, 80, Priority: 0);
        public static readonly Move FocusedLaser = new("Focused Laser", "focused a powerful laser!", 85, 85, Priority: 0,
            Status.Burn);
        public static readonly Move CrushingGrip = new("Crushing Grip", "squeezed the life out of their target!", 85, 95, Priority: 0);
        public static readonly Move GroundSlam = new("Ground Slam", "leaped into the air and slammed into the ground!",
            90, 90, Priority: 0);
        public static readonly Move RedirectLightning = new("Redirect Lightning", "aimed and released a bolt of lightning!",
            100, 85, Priority: -1);
    }
}
