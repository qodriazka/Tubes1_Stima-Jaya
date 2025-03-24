using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Sniper : Bot
{
    // Indicates whether the bot is in a “peeking” state (scanning while moving along the wall)
    bool peek;
    // The distance to move along the wall (set to the maximum arena dimension)
    double moveAmount;

    static void Main()
    {
        new Sniper().Start();
    }

    // Constructor that loads the bot configuration from JSON
    Sniper() : base(BotInfo.FromFile("Sniper.json")) { }

    public override void Run()
    {
        // Set colors for visual identification
        BodyColor = Color.Black;
        TurretColor = Color.Black;
        RadarColor = Color.Orange;
        BulletColor = Color.Cyan;
        ScanColor = Color.Cyan;

        // Set the move amount to the maximum arena dimension
        moveAmount = Math.Max(ArenaWidth, ArenaHeight);
        peek = false;

        // Initially, turn to face a wall. Use the remainder of the current direction modulo 90.
        TurnRight(Direction % 90);
        Forward(moveAmount);

        // Turn the gun to face inward (turn gun right 90°), then turn right to align with the wall
        peek = true;
        TurnGunRight(90);
        TurnRight(90);

        // Main loop: continuously patrol the perimeter
        while (IsRunning)
        {
            // Peek while moving along the wall so that scanning remains active
            peek = true;
            Forward(moveAmount);
            peek = false;
            TurnRight(90);
        }
    }

    // When another bot is scanned, the bot stops to fire a predicted shot and then resumes movement.
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Stop any current movement to allow for an accurate shot.
        Stop();

        // --- Predicted Shot Calculation ---
        double bulletPower = 2; 
        double bulletSpeed = 20 - 3 * bulletPower; // bullet speed formula

        // Calculate the distance to the enemy
        double distance = DistanceTo(e.X, e.Y);
        // Estimate the time required for the bullet to reach the target
        double time = distance / bulletSpeed;

        // Predict the enemy's future position (assuming constant Speed and Direction).
        // Note: e.Direction is in degrees.
        double enemyDirectionRad = ToRadians(e.Direction);
        double predictedX = e.X + e.Speed * Math.Cos(enemyDirectionRad) * time;
        double predictedY = e.Y + e.Speed * Math.Sin(enemyDirectionRad) * time;

        // Determine the angle from the bot's current position to the predicted position.
        double angleToPredicted = Math.Atan2(predictedY - Y, predictedX - X) * 180 / Math.PI;
        // Calculate the minimal angle to turn the gun (based on current GunDirection).
        double gunTurn = NormalizeAngle(angleToPredicted - GunDirection);
        TurnGunRight(gunTurn);

        // Fire the bullet with the chosen power.
        Fire(bulletPower);

        // Optionally, if peeking is enabled, force an immediate rescan.
        if (peek)
            Rescan();

        // Resume patrolling the wall by reissuing the forward movement.
        Resume();
    }

    // When colliding with another bot, simply back up a bit to avoid being stuck.
    public override void OnHitBot(HitBotEvent e)
    {
        var bearing = BearingTo(e.X, e.Y);
        if (bearing > -90 && bearing < 90)
            Back(100);
        else
            Forward(100);
    }

    // Helper method: Converts degrees to radians.
    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    
    // Helper method: Normalizes an angle to the range [-180, 180] degrees.
    private double NormalizeAngle(double angle)
    {
        while (angle > 180)
            angle -= 360;
        while (angle < -180)
            angle += 360;
        return angle;
    }
}
