using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Sniper : Bot
{
    // Menunjukkan apakah bot dalam keadaan "mengintip" (melakukan scan sambil bergerak sepanjang dinding)
    bool peek;
    // Jarak untuk bergerak sepanjang dinding (diatur ke dimensi arena maksimum)
    double moveAmount;

    static void Main()
    {
        new Sniper().Start();
    }

    // Konstruktor yang memuat konfigurasi bot dari JSON
    Sniper() : base(BotInfo.FromFile("Sniper.json")) { }

    public override void Run()
    {
        // Mengatur warna untuk identifikasi visual
        BodyColor = Color.Black;
        TurretColor = Color.Black;
        RadarColor = Color.Orange;
        BulletColor = Color.Cyan;
        ScanColor = Color.Cyan;

        // Mengatur jarak gerakan ke dimensi arena maksimum
        moveAmount = Math.Max(ArenaWidth, ArenaHeight);
        peek = false;

        // Awalnya, berputar untuk menghadap dinding. Gunakan sisa dari arah saat ini modulo 90.
        TurnRight(Direction % 90);
        Forward(moveAmount);

        // Putar senjata untuk menghadap ke dalam (putar senjata 90° ke kanan), kemudian belok kanan untuk sejajar dengan dinding
        peek = true;
        TurnGunRight(90);
        TurnRight(90);

        // Loop utama: terus berpatroli di perimeter
        while (IsRunning)
        {
            // Mengintip sambil bergerak sepanjang dinding agar pemindaian tetap aktif
            peek = true;
            Forward(moveAmount);
            peek = false;
            TurnRight(90);
        }
    }

    // Ketika robot lain dipindai, robot berhenti untuk menembak prediksi tembakan dan kemudian melanjutkan gerakan.
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Hentikan gerakan saat ini untuk memungkinkan tembakan yang akurat.
        Stop();

        // --- Perhitungan Tembakan Prediksi ---
        double bulletPower = 2; 
        double bulletSpeed = 20 - 3 * bulletPower; // rumus kecepatan peluru

        // Hitung jarak ke musuh
        double distance = DistanceTo(e.X, e.Y);
        // Perkirakan waktu yang diperlukan peluru untuk mencapai target
        double time = distance / bulletSpeed;

        // Prediksi posisi musuh di masa depan (dengan asumsi Kecepatan dan Arah konstan).
        // Catatan: e.Direction adalah dalam derajat.
        double enemyDirectionRad = ToRadians(e.Direction);
        double predictedX = e.X + e.Speed * Math.Cos(enemyDirectionRad) * time;
        double predictedY = e.Y + e.Speed * Math.Sin(enemyDirectionRad) * time;

        // Tentukan sudut dari posisi robot saat ini ke posisi yang diprediksi.
        double angleToPredicted = Math.Atan2(predictedY - Y, predictedX - X) * 180 / Math.PI;
        // Hitung sudut minimal untuk memutar senjata (berdasarkan GunDirection saat ini).
        double gunTurn = NormalizeAngle(angleToPredicted - GunDirection);
        TurnGunRight(gunTurn);

        // Tembakkan peluru dengan kekuatan yang dipilih.
        Fire(bulletPower);

        // Secara opsional, jika mengintip diaktifkan, paksa pemindaian ulang segera.
        if (peek)
            Rescan();

        // Lanjutkan berpatroli di dinding dengan melanjutkan gerakan maju
        Resume();
    }

    // Ketika bertabrakan dengan robot lain, cukup mundur sedikit untuk menghindari terjebak.
    public override void OnHitBot(HitBotEvent e)
    {
        var bearing = BearingTo(e.X, e.Y);
        if (bearing > -90 && bearing < 90)
            Back(100);
        else
            Forward(100);
    }

    // Metode pembantu: Mengkonversi derajat ke radian.
    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    
    // Metode pembantu: Menormalkan sudut ke rentang [-180, 180] derajat.
    private double NormalizeAngle(double angle)
    {
        while (angle > 180)
            angle -= 360;
        while (angle < -180)
            angle += 360;
        return angle;
    }
}