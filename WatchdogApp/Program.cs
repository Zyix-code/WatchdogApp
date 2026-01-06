using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using Microsoft.Win32;
using OpenCvSharp;
using ZYIXSecurity.Properties;

namespace ZYIXSecurity
{
    static class UI 
    {
        public static void HeaderCiz(string baslik, ConsoleColor renk)
        {
            try { Console.Clear(); } catch { }
            Console.ForegroundColor = renk;

            string[] logo = {
                @"███████╗██╗   ██╗██╗██╗  ██╗",
                @"╚══███╔╝╚██╗ ██╔╝██║╚██╗██╔╝",
                @"  ███╔╝  ╚████╔╝ ██║ ╚███╔╝ ",
                @" ███╔╝    ╚██╔╝  ██║ ██╔██╗ ",
                @"███████╗   ██║   ██║██╔╝ ██╗",
                @"╚══════╝   ╚═╝   ╚═╝╚═╝  ╚═╝"
            };

            foreach (var l in logo) Ortala(l);
            Console.WriteLine();
            Ortala($"--- {baslik} ---");
            Console.WriteLine();
            Console.ResetColor();
        }

        public static void BilgiKutusu(string[] satirlar, ConsoleColor renk)
        {
            Console.ForegroundColor = renk;
            int w = 60;
            string top = "╔" + new string('═', w - 2) + "╗";
            string bot = "╚" + new string('═', w - 2) + "╝";

            Ortala(top);
            foreach (var s in satirlar)
            {
                string line = "║" + OrtaliMetin(s, w - 2) + "║";
                Ortala(line);
            }
            Ortala(bot);
            Console.ResetColor();
        }

        public static void Ortala(string text)
        {
            try
            {
                int w = Console.WindowWidth;
                int pos = (w - text.Length) / 2;
                if (pos < 0) pos = 0;
                Console.WriteLine(new string(' ', pos) + text);
            }
            catch { Console.WriteLine(text); }
        }

        public static string OrtaliMetin(string text, int width)
        {
            if (text.Length >= width) return text.Substring(0, width);
            int leftPad = (width - text.Length) / 2;
            int rightPad = width - text.Length - leftPad;
            return new string(' ', leftPad) + text + new string(' ', rightPad);
        }

        public static int PromptYaz(string prompt)
        {
            try
            {
                int w = Console.WindowWidth;
                int pos = (w - 40) / 2;
                if (pos < 0) pos = 0;
                Console.Write(new string(' ', pos) + prompt);
                return Console.CursorLeft;
            }
            catch
            {
                Console.Write(prompt);
                return prompt.Length;
            }
        }
    }

    class Program
    {
        static string anaKlasor = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZYIX_Logs");
        static string fotoKlasoru = Path.Combine(anaKlasor, "IzinsizGirisler");
        static string logDosyasi = Path.Combine(anaKlasor, "Security.log");
        static int zamanAsimiSuresi = 30;

        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")] static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("kernel32.dll")] static extern uint GetCurrentThreadId();
        [DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")] static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")] static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        [DllImport("user32.dll")] static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")] static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hWnd);
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)] static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_APPWINDOW = 0x00040000;
        const int SW_HIDE = 0, SW_SHOW = 5, SW_RESTORE = 9;
        const uint SWP_NOSIZE = 1, SWP_NOMOVE = 2, SWP_SHOWWINDOW = 0x40;
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        static void Main(string[] args)
        {
            IntPtr consoleHandle = GetConsoleWindow();
            ShowWindow(consoleHandle, SW_SHOW);

            try
            {
                Console.Title = "ZYIX SECURITY CORE";
                Console.CursorVisible = true;

                DisableCloseButton(consoleHandle);
                TaskbardanGizle(consoleHandle);
                Console.TreatControlCAsInput = true;
                Console.CancelKeyPress += (s, e) => { e.Cancel = true; };

                KlasorleriHazirla();

                if (!IsAdmin())
                {
                    /*UI.HeaderCiz("YETKİ HATASI", ConsoleColor.Red);
                    UI.BilgiKutusu(new[] { "Yönetici izni bulunamadı.", "Lütfen programı YÖNETİCİ olarak çalıştırın." }, ConsoleColor.Red);
                    Console.ReadLine();
                    return;*/
                }

                if (string.IsNullOrEmpty(Settings.Default.MasterHash) || string.IsNullOrEmpty(Settings.Default.HedefUygulama))
                {
                    IlkKurulumEkrani();
                }

                BaslangicAyariniUygula(Settings.Default.BaslangicAktif);

                UI.HeaderCiz("SİSTEM AKTİF", ConsoleColor.Green);
                UI.BilgiKutusu(new[] {
                    $"HEDEF: {Settings.Default.HedefUygulama}",
                    "Koruma başlatılıyor...",
                    "Arka plana geçiliyor."
                }, ConsoleColor.Green);

                Thread.Sleep(2000);
                ShowWindow(consoleHandle, SW_HIDE);

                while (true)
                {
                    Console.Title = "ZYIX - Monitoring...";
                    IntPtr targetHandle = IntPtr.Zero;
                    uint targetPid = 0;
                    string hedef = Settings.Default.HedefUygulama;

                    EnumWindows((hWnd, lParam) =>
                    {
                        if (IsWindowVisible(hWnd))
                        {
                            int len = GetWindowTextLength(hWnd);
                            if (len > 0)
                            {
                                StringBuilder sb = new StringBuilder(len + 1);
                                GetWindowText(hWnd, sb, sb.Capacity);
                                if (sb.ToString().IndexOf(hedef, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    targetHandle = hWnd;
                                    GetWindowThreadProcessId(hWnd, out targetPid);
                                    return false;
                                }
                            }
                        }
                        return true;
                    }, IntPtr.Zero);

                    if (targetHandle != IntPtr.Zero)
                    {
                        ShowWindow(targetHandle, SW_HIDE);
                        ShowWindow(consoleHandle, SW_SHOW);
                        SetWindowPos(consoleHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                        ForceFocus(consoleHandle);

                        GuvenlikModu(consoleHandle, targetHandle, targetPid);
                    }
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                ShowWindow(consoleHandle, SW_SHOW);
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Clear();
                Console.WriteLine($"\n[KRİTİK HATA]: {ex.Message}");
                Console.ReadLine();
            }
        }
        static string GuvenliSifreAl(string promptText, bool maskele = false)
        {
            while (Console.KeyAvailable) Console.ReadKey(true);

            int baslangicX = UI.PromptYaz(promptText);
            int satirY = Console.CursorTop;

            StringBuilder sb = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return sb.ToString();
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (Console.CursorLeft > baslangicX)
                    {
                        if (sb.Length > 0) sb.Length--;
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    if (maskele) Console.Write("*");
                    else Console.Write(key.KeyChar);
                }
            }
        }

        static void IlkKurulumEkrani()
        {
            while (true)
            {
                UI.HeaderCiz("SİSTEM KURULUMU - ADIM 1/2", ConsoleColor.Green);
                UI.BilgiKutusu(new[] { "Yönetici şifresi belirleyin." }, ConsoleColor.White);
                Console.WriteLine("\n");

                string p1 = GuvenliSifreAl(">> Yeni Şifre: ", true);
                string p2 = GuvenliSifreAl(">> Tekrar Girin: ", true);

                if (!string.IsNullOrWhiteSpace(p1) && p1 == p2)
                {
                    SifreKaydet(p1);
                    break;
                }
                UI.Ortala(" [!] Şifreler eşleşmiyor veya boş.");
                Thread.Sleep(1500);
            }

            while (true)
            {
                UI.HeaderCiz("SİSTEM KURULUMU - ADIM 2/2", ConsoleColor.Green);
                UI.BilgiKutusu(new[] { "Korunacak uygulamanın tam adını girin." }, ConsoleColor.White);
                Console.WriteLine("\n");

                string app = GuvenliSifreAl(">> Uygulama Adı (örn: WhatsApp): ", false);

                if (!string.IsNullOrWhiteSpace(app))
                {
                    Settings.Default.HedefUygulama = app;
                    Settings.Default.Save();
                    LogYaz("KURULUM", $"Kurulum tamamlandı. Hedef: {app}");
                    break;
                }
                UI.Ortala(" [!] Boş bırakılamaz.");
                Thread.Sleep(1000);
            }
        }

        static void GuvenlikModu(IntPtr consoleHandle, IntPtr targetHandle, uint targetPid)
        {
            LockScreenCiz();

            while (true)
            {
                GorevYoneticisiniKapat();

                if (GetForegroundWindow() != consoleHandle)
                {
                    SetWindowPos(consoleHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                    ForceFocus(consoleHandle);
                    ShowWindow(targetHandle, SW_HIDE);
                }

                var sonuc = GuvenliInputOkuTimeout();

                if (sonuc.Durum == InputDurumu.Timeout)
                {
                    Console.Beep(500, 500);
                    try { Process.GetProcessById((int)targetPid).Kill(); } catch { }

                    LogYaz("TIMEOUT", "Süre doldu, hedef kapatıldı.");
                    SetWindowPos(consoleHandle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                    ShowWindow(consoleHandle, SW_HIDE);
                    return;
                }
                else if (sonuc.Durum == InputDurumu.F12)
                {
                    AdminPanelMenusu();
                    LockScreenCiz();
                }
                else if (sonuc.Durum == InputDurumu.MetinGirildi)
                {
                    if (Dogrula(sonuc.Metin))
                    {
                        KilitAcildi(consoleHandle, targetHandle);
                        return;
                    }
                    else if (!string.IsNullOrEmpty(sonuc.Metin))
                    {
                        HataEkrani();
                        LockScreenCiz();
                    }
                }
            }
        }
        static InputSonuc GuvenliInputOkuTimeout()
        {
            while (Console.KeyAvailable) Console.ReadKey(true);

            int startX = UI.PromptYaz(">> Şifre: ");
            StringBuilder sb = new StringBuilder();
            DateTime sonIslem = DateTime.Now;

            while (true)
            {
                GorevYoneticisiniKapat();

                TimeSpan gecenSure = DateTime.Now - sonIslem;
                int kalanSure = zamanAsimiSuresi - (int)gecenSure.TotalSeconds;
                Console.Title = $"ZYIX SECURITY - Kilitli | Kalan: {kalanSure}sn";

                if (kalanSure <= 0) return new InputSonuc { Durum = InputDurumu.Timeout };

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    sonIslem = DateTime.Now;

                    if ((key.Modifiers & ConsoleModifiers.Control) != 0) continue;

                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        return new InputSonuc { Metin = sb.ToString(), Durum = InputDurumu.MetinGirildi };
                    }
                    else if (key.Key == ConsoleKey.F12)
                    {
                        return new InputSonuc { Durum = InputDurumu.F12 };
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        // Sınır Koruması
                        if (Console.CursorLeft > startX)
                        {
                            if (sb.Length > 0) sb.Length--;
                            Console.Write("\b \b");
                        }
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        sb.Append(key.KeyChar);
                        Console.Write("*");
                    }
                }
                Thread.Sleep(20);
            }
        }

        static void AdminPanelMenusu()
        {
            UI.HeaderCiz("YÖNETİCİ DOĞRULAMA", ConsoleColor.Yellow);
            string pass = GuvenliSifreAl(">> Şifre: ", true);

            if (!Dogrula(pass))
            {
                HataEkrani();
                return;
            }

            while (true)
            {
                UI.HeaderCiz("KONTROL PANELİ", ConsoleColor.Yellow);
                string baslangic = Settings.Default.BaslangicAktif ? "[AÇIK]" : "[KAPALI]";
                string hedefApp = Settings.Default.HedefUygulama;

                string[] secenekler = {
                    "1. Şifre Değiştir",
                    $"2. Hedef Uygulama ({hedefApp})",
                    $"3. Başlangıç Ayarı {baslangic}",
                    "4. Log Kayıtlarını Gör",
                    "5. Çıkış"
                };

                UI.BilgiKutusu(secenekler, ConsoleColor.Yellow);
                Console.WriteLine("\n");

                string secim = GuvenliSifreAl(">> Seçiminiz: ", false);

                if (secim == "1")
                {
                    UI.HeaderCiz("ŞİFRE GÜNCELLEME", ConsoleColor.Yellow);
                    string yeni = GuvenliSifreAl(">> Yeni Şifre: ", true);

                    if (!string.IsNullOrWhiteSpace(yeni))
                    {
                        SifreKaydet(yeni);
                        Console.WriteLine();
                        UI.Ortala("[OK] Şifre güncellendi.");
                        Thread.Sleep(1500);
                    }
                }
                else if (secim == "2")
                {
                    UI.HeaderCiz("HEDEF GÜNCELLEME", ConsoleColor.Yellow);
                    UI.Ortala($"Mevcut: {Settings.Default.HedefUygulama}");
                    Console.WriteLine("\n");
                    string yeniHedef = GuvenliSifreAl(">> Yeni Hedef (örn: Notepad): ", false);

                    if (!string.IsNullOrWhiteSpace(yeniHedef))
                    {
                        Settings.Default.HedefUygulama = yeniHedef;
                        Settings.Default.Save();
                        Console.WriteLine();
                        UI.Ortala($"[OK] Güncellendi: {yeniHedef}");
                        Thread.Sleep(1500);
                    }
                }
                else if (secim == "3")
                {
                    bool yeni = !Settings.Default.BaslangicAktif;
                    Settings.Default.BaslangicAktif = yeni;
                    Settings.Default.Save();
                    BaslangicAyariniUygula(yeni);
                }
                else if (secim == "4")
                {
                    try { Process.Start("notepad.exe", logDosyasi); } catch { }
                }
                else if (secim == "5")
                {
                    break;
                }
            }
        }
        enum InputDurumu { MetinGirildi, F12, Timeout }
        class InputSonuc { public string Metin; public InputDurumu Durum; }

        static void LockScreenCiz()
        {
            UI.HeaderCiz("SİSTEM KİLİTLENDİ", ConsoleColor.Cyan);
            string durum = Settings.Default.BaslangicAktif ? "OTO: AÇIK" : "OTO: KAPALI";
            string hedef = Settings.Default.HedefUygulama.ToUpper();

            UI.BilgiKutusu(new[] {
                $"HEDEF: {hedef}",
                "Erişim Kısıtlandı.",
                "",
                $"[F12] Yönetici | {durum}"
            }, ConsoleColor.Cyan);
            Console.WriteLine("\n\n");
        }

        static void HataEkrani()
        {
            UI.HeaderCiz("İHLAL TESPİTİ", ConsoleColor.Red);
            UI.BilgiKutusu(new[] { "ERİŞİM REDDEDİLDİ", "Fotoğraf Alınıyor..." }, ConsoleColor.Red);
            Console.Beep(2000, 300);

            try
            {
                using (var capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW))
                {
                    if (capture.IsOpened())
                    {
                        using (Mat frame = new Mat())
                        {
                            capture.Read(frame);
                            Thread.Sleep(50);
                            capture.Read(frame);
                            if (!frame.Empty())
                            {
                                string yol = Path.Combine(fotoKlasoru, $"Intruder_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                                Cv2.ImWrite(yol, frame);
                                LogYaz("KAMERA", "Foto: " + yol);
                            }
                        }
                    }
                }
            }
            catch { }

            LogYaz("İHLAL", "Hatalı şifre girişi.");
            Thread.Sleep(1000);
        }

        static void KilitAcildi(IntPtr console, IntPtr target)
        {
            UI.HeaderCiz("ERİŞİM ONAYLANDI", ConsoleColor.Green);
            LogYaz("GIRIS", "Kilit kaldırıldı.");
            Thread.Sleep(800);

            SetWindowPos(console, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            ShowWindow(console, SW_HIDE);

            ShowWindow(target, SW_SHOW);
            ShowWindow(target, SW_RESTORE);
            SetForegroundWindow(target);

            while (IsWindowVisible(target)) Thread.Sleep(2000);
        }

        static void BaslangicAyariniUygula(bool aktif)
        {
            try
            {
                string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string batDosyasi = Path.Combine(startupPath, "ZYIX_Loader.bat");
                string exeYolu = Process.GetCurrentProcess().MainModule.FileName;

                if (aktif)
                {
                    string batIcerik = $"@echo off\r\nstart \"\" \"{exeYolu}\"";
                    File.WriteAllText(batDosyasi, batIcerik);
                }
                else
                {
                    if (File.Exists(batDosyasi)) File.Delete(batDosyasi);
                }
            }
            catch (Exception ex) { LogYaz("HATA", "Başlangıç: " + ex.Message); }
        }

        static bool Dogrula(string girilen)
        {
            string hash = Settings.Default.MasterHash;
            string inputHash = HashUret(girilen);
            if (hash.Length != inputHash.Length) return false;
            int diff = 0;
            for (int i = 0; i < hash.Length; i++) diff |= hash[i] ^ inputHash[i];
            return diff == 0;
        }

        static void SifreKaydet(string raw)
        {
            Settings.Default.MasterHash = HashUret(raw);
            Settings.Default.Save();
        }

        static string HashUret(string text)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] b = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                StringBuilder sb = new StringBuilder();
                foreach (byte x in b) sb.Append(x.ToString("x2"));
                return sb.ToString();
            }
        }

        static void GorevYoneticisiniKapat()
        {
            try { foreach (var t in Process.GetProcessesByName("Taskmgr")) t.Kill(); } catch { }
        }

        static void KlasorleriHazirla()
        {
            Directory.CreateDirectory(anaKlasor);
            Directory.CreateDirectory(fotoKlasoru);
        }

        static void LogYaz(string tur, string mesaj)
        {
            try { File.AppendAllText(logDosyasi, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{tur}] {mesaj}\n"); } catch { }
        }

        static void ForceFocus(IntPtr hWnd)
        {
            uint f = GetWindowThreadProcessId(GetForegroundWindow(), out _);
            uint a = GetCurrentThreadId();
            if (f != a) { AttachThreadInput(f, a, true); SetForegroundWindow(hWnd); AttachThreadInput(f, a, false); }
            else SetForegroundWindow(hWnd);
        }

        static void DisableCloseButton(IntPtr hWnd)
        {
            IntPtr hMenu = GetSystemMenu(hWnd, false);
            if (hMenu != IntPtr.Zero) DeleteMenu(hMenu, 0xF060, 0);
        }

        static void TaskbardanGizle(IntPtr hWnd)
        {
            int style = GetWindowLong(hWnd, GWL_EXSTYLE);
            style |= WS_EX_TOOLWINDOW;
            style &= ~WS_EX_APPWINDOW;
            SetWindowLong(hWnd, GWL_EXSTYLE, style);
        }

        static bool IsAdmin()
        {
            using (var id = WindowsIdentity.GetCurrent())
                return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}