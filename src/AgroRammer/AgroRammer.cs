using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class AgroRammer : Bot
{
    bool movingForward;

    static void Main(string[] args)
    {
        new AgroRammer().Start();
    }

    AgroRammer() : base(BotInfo.FromFile("AgroRammer.json")) { }

    public override void Run()
    {
        BodyColor = Color.Black;
        TurretColor = Color.Blue;
        RadarColor = Color.Red;
        BulletColor = Color.Green;
        ScanColor = Color.Red;
        GunColor = Color.Yellow;
        
        while (IsRunning)
        {
            // Tell the game that when we take move, we'll also want to turn right... a lot
            SetTurnLeft(10_000);
            // Limit our speed to 5
            MaxSpeed = 5;
            // Start moving (and turning)
            Forward(10_000);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {   

        if (e.Energy < Energy) // Jika energi musuh lebih besar dari bot kita
        {
            double bearingToEnemy = BearingTo(e.X, e.Y); // Hitung arah ke musuh
            SetTurnLeft(bearingToEnemy); // Putar ke arah musuh
            SetForward(100); // Maju untuk menabrak
            Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));
        }
        else
        {
            var bearingFromGun = GunBearingTo(e.X, e.Y);
            Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));
        }
    }

    private void SmartFire(double distance)
    {
        if (distance > 200 || Energy < 15)
            Fire(1);
        else if (distance > 50)
            Fire(2);
        else
            Fire(3);
    }


    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // Hindari dengan bergerak ke samping
        SetTurnRight(90);
        SetForward(120);
        
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // Putar 90 derajat dan bergerak menjauhi dinding
        SetTurnRight(90);
        SetForward(150);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        if (e.IsRammed) // Jika bot lain yang menabrak kita
        {
            if (e.Energy < 10) 
            {
                // Jika musuh hampir mati, dorong dan habisi dengan tabrakan
                SetForward(50);
                Fire(3);
            }
            else 
            {
                // Jika musuh lebih kuat, segera menghindar
                SetTurnRight(90);
                SetBack(100);
            }
        }
        else 
        {
            // Jika kita yang menabrak, mundur dan serang
            SetBack(50);
            Fire(2);
        }
    }
}
