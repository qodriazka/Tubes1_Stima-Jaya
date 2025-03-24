using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class HitnRun : Bot
{
    int turnDirection = 1; // searah jarum jam (-1) atau berlawanan arah jarum jam (1)

    // Metode utama untuk memulai bot kita
    static void Main(string[] args)
    {
        new HitnRun().Start();
    }

    // Konstruktor, yang memuat file pengaturan bot
    HitnRun() : base(BotInfo.FromFile("HitnRun.json")) { }

    // Dipanggil ketika ronde baru dimulai -> inisialisasi dan lakukan beberapa gerakan
    public override void Run()
    {
        // Atur warna
        BodyColor = Color.FromArgb(0x99, 0x99, 0x99);   // abu-abu terang
        TurretColor = Color.FromArgb(0x88, 0x88, 0x88); // abu-abu
        RadarColor = Color.FromArgb(0x66, 0x66, 0x66);  // abu-abu gelap

        // Putar senjata secara terus menerus untuk pemindaian.
        while (IsRunning)
        {
            TurnLeft(5 * turnDirection);
        }
    }

    // Ketika bot lain terpindai, putuskan apakah akan menyerang atau melarikan diri.
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Jika energi musuh lebih rendah dari energi kita, serang.
        if (e.Energy <= Energy)
        {
            // Hadapi musuh dan maju ke arahnya.
            TurnToFaceTarget(e.X, e.Y);
            double distance = DistanceTo(e.X, e.Y);
            Forward(distance + 5);
        }
        // Jika tidak, melarikan diri.
        else
        {
            // Berputar menjauhi musuh dan mundur.
            TurnAway(e.X, e.Y);
            double distance = DistanceTo(e.X, e.Y);
            Forward(distance + 5);
        }

        // Lanjutkan pemindaian untuk bot lainnya.
        Rescan();
    }

    // Ketika bertabrakan dengan bot lain, cukup mundur.
    public override void OnHitBot(HitBotEvent e)
    {
        Back(40);
    }

    // Memutar bot untuk menghadap target yang berada di posisi (x, y)
    private void TurnToFaceTarget(double x, double y)
    {
        var bearing = BearingTo(x, y);
        // Secara opsional sesuaikan turnDirection jika diperlukan.
        turnDirection = (bearing >= 0) ? 1 : -1;
        TurnLeft(bearing);
    }

    // Memutar bot menjauhi target yang berada di posisi (x, y) dengan berputar 180° menjauhinya.
    private void TurnAway(double x, double y)
    {
        var bearing = BearingTo(x, y);
        // Untuk melarikan diri, tambahkan 180 derajat ke bearing.
        TurnLeft(bearing + 180);
    }
}