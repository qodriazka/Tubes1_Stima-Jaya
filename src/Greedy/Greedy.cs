using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Greedy : Bot
{
    int turnDirection = 1; // clockwise (-1) or counterclockwise (1)

    // The main method starts our bot
    static void Main(string[] args)
    {
        new Greedy().Start();
    }

    // Constructor, which loads the bot settings file
    Greedy() : base(BotInfo.FromFile("Greedy.json")) { }

    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {
        // Set colors
        BodyColor = Color.FromArgb(0x99, 0x99, 0x99);   // lighter gray
        TurretColor = Color.FromArgb(0x88, 0x88, 0x88); // gray
        RadarColor = Color.FromArgb(0x66, 0x66, 0x66);  // dark gray

        // Continuously spin the gun for scanning.
        while (IsRunning)
        {
            TurnLeft(5 * turnDirection);
        }
    }

    // When another bot is scanned, decide whether to attack or run away.
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // If the enemy's energy is lower than ours, attack.
        if (e.Energy <= Energy)
        {
            // Face the enemy and drive towards it.
            TurnToFaceTarget(e.X, e.Y);
            double distance = DistanceTo(e.X, e.Y);
            Forward(distance + 5);
        }
        // Otherwise, run away.
        else
        {
            // Turn away from the enemy and back up.
            TurnAway(e.X, e.Y);
            double distance = DistanceTo(e.X, e.Y);
            Forward(distance + 5);
        }

        // Continue scanning for other bots.
        Rescan();
    }

    // When colliding with another bot, simply back up.
    public override void OnHitBot(HitBotEvent e)
    {
        Back(40);
    }

    // Turns the bot to face the target located at (x, y)
    private void TurnToFaceTarget(double x, double y)
    {
        var bearing = BearingTo(x, y);
        // Optionally adjust turnDirection if needed.
        turnDirection = (bearing >= 0) ? 1 : -1;
        TurnLeft(bearing);
    }

    // Turns the bot away from the target located at (x, y) by turning 180° away from it.
    private void TurnAway(double x, double y)
    {
        var bearing = BearingTo(x, y);
        // To run away, add 180 degrees to the bearing.
        TurnLeft(bearing + 180);
    }
}