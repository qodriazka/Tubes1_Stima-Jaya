using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using Microsoft.Extensions.Configuration;
using System.IO;

public class Drift : Bot
{
    static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Drift.json");
        var config = builder.Build();
        var botInfo = BotInfo.FromConfiguration(config);
        new Drift(botInfo).Start();
    }

    private Drift(BotInfo botInfo) : base(botInfo) {}

    public override void Run()
    {
        BodyColor = Color.FromArgb(64, 191, 202);
        TurretColor = Color.FromArgb(5, 166, 203);
        RadarColor = Color.FromArgb(0xFF, 0xD7, 0x00);
        BulletColor = Color.FromArgb(0xFF, 0x45, 0x00);
        TracksColor = Color.FromArgb(0x99, 0x33, 0x00);
        GunColor = Color.FromArgb(0xCC, 0x55, 0x00);
        while (IsRunning)
        {
            SetTurnRight(10_000);
            Forward(10_000);
            if (X < 100 || X > 700 || Y < 100 || Y > 500){
                TurnRight(10);
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        Console.WriteLine("I see a bot at " + e.X + ", " + e.Y);
        Fire(3);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Ouch! I hit a bot at " + e.X + ", " + e.Y);
        var bearing = Math.Abs(BearingTo(e.X, e.Y));
        if (bearing < 10){
            Fire(3);
        }
        if (e.IsRammed){
            TurnRight(10);
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {
        TurnRight(20);
        Run();
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }
}
