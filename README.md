# 🛡️ WatchdogApp – Akıllı Masaüstü Güvenlik Sistemi

<p align="center">
  <img src="https://media.giphy.com/media/v1.Y2lkPTc5MGI3NjExM2Q3ZzF6eXJ6eXJ6eXJ6eXJ6eXJ6eXJ6eXJ6eXJ6eXJ6eSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/LfpjDCLkxZheU/giphy.gif" width="180px">
</p>

<p align="center">
  <b>.NET C# ve Win32 API ile geliştirilmiş, hedef odaklı ileri düzey güvenlik uygulaması.</b><br>
  Belirlenen uygulamaları (örn: WhatsApp) kilitler, izinsiz girişlerde kanıt toplar ve kendini gizler.
</p>

---

## 🚀 Özellikler

- ✔ **Hedef Kilitleme:** Kullanıcının belirlediği uygulama açıldığı an ekran kilitlenir ve uygulama gizlenir.
- ✔ **Hayalet Modu (Ghost Mode):** Uygulama **Görev Çubuğunda (Taskbar)** ve **Alt-Tab** menüsünde görünmez.
- ✔ **Kanıt Toplama:** Hatalı şifre girildiğinde webcam üzerinden sessizce fotoğraf çeker (`/IzinsizGirisler`).
- ✔ **Akıllı Bekçi (Smart Watchdog):** Sadece kilit ekranı aktifken **Görev Yöneticisi'ni (TaskMgr)** engeller.
- ✔ **Zaman Aşımı (Timeout):** 30 saniye boyunca işlem yapılmazsa güvenlik gereği hedef uygulama tamamen kapatılır.
- ✔ **Güvenli Giriş:** Prompt korumalı, maskeli ve sınırlandırılmış konsol girişi.
- ✔ **Yönetici Paneli:** Şifre değiştirme, hedef uygulama belirleme ve başlangıç ayarları (Startup).

<p align="center">
  <img src="https://img.shields.io/badge/C%23-239120?logo=c-sharp&logoColor=white&style=flat-square">
  <img src="https://img.shields.io/badge/.NET_Framework-512BD4?logo=dotnet&logoColor=white&style=flat-square">
  <img src="https://img.shields.io/badge/Win32_API-0078D6?logo=windows&logoColor=white&style=flat-square">
  <img src="https://img.shields.io/badge/OpenCV-5C3EE8?logo=opencv&logoColor=white&style=flat-square">
  <img src="https://img.shields.io/badge/License-GPLv3-blue.svg?style=flat-square">
</p>

---

## 🧠 Sistem Nasıl Çalışır?

### 🔹 Dinleme Modu
- Uygulama arka planda (Background) ve tamamen gizli (Hidden) olarak çalışır.
- Sistem kaynaklarını tüketmeden `Win32 API` çağrıları ile aktif pencereleri tarar.

### 🔹 Tespit ve Kilit
- Hedef uygulama (örn: `WhatsApp`, `Notepad`) tespit edildiği an:
  1. Hedef pencere `SW_HIDE` komutu ile gizlenir.
  2. Watchdog güvenlik ekranı `TopMost` (En Üstte) olarak açılır.
  3. Klavye ve Fare odağı güvenlik ekranına hapsedilir (Aggressive Focus).

### 🔹 İhlal Durumu
- Yanlış şifre girilirse `OpenCvSharp` kütüphanesi ile anlık fotoğraf çekilir ve loglanır.
- Kullanıcı bilgisayar başında değilse (30 sn timeout), hedef uygulama `Process.Kill()` ile sonlandırılır.

---

## 🛠️ Kurulum ve Derleme

### 1️⃣ Gereksinimler
- Visual Studio 2022 veya üzeri
- .NET Framework 4.7.2+
- Webcam (Fotoğraf özelliği için gereklidir)

### 2️⃣ NuGet Paketleri
Projeyi derlemeden önce aşağıdaki paketlerin kurulu olduğundan emin olun:
```bash
Install-Package OpenCvSharp4
Install-Package OpenCvSharp4.runtime.win

### 3️⃣ İlk Çalıştırma
Programı tam yetki ile çalışması için Yönetici Olarak (Run as Administrator) başlatın.

İlk Kurulum: Uygulama ilk açılışta sizden bir Yönetici Şifresi ve Hedef Uygulama İsmi isteyecektir.

Ayarlar: Bu bilgiler Properties.Settings içerisinde güvenli bir şekilde saklanır.

Aktivasyon: Kurulumdan sonra uygulama kendini gizler ve nöbet moduna geçer.

⚖️ Lisans
Bu proje GNU General Public License v3.0 ile lisanslanmıştır. Bu yazılım tamamen eğitim ve kişisel güvenlik amaçlı geliştirilmiştir. Kötüye kullanımda sorumluluk kullanıcıya aittir.

🤝 İletişim
<p align="left"> <a href="https://discordapp.com/users/481831692399673375"><img src="https://img.shields.io/badge/Discord-Zyix%231002-7289DA?logo=discord&style=flat-square"></a> <a href="https://www.youtube.com/channel/UC7uBi3y2HOCLde5MYWECynQ?view_as=subscriber"><img src="https://img.shields.io/badge/YouTube-Subscribe-red?logo=youtube&style=flat-square"></a> <a href="https://www.reddit.com/user/_Zyix"><img src="https://img.shields.io/badge/Reddit-Profile-orange?logo=reddit&style=flat-square"></a> </p>
