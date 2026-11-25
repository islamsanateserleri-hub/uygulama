; Hikâye Stüdyosu kurulum betiği (Inno Setup 6.3+). Unity menüsü: Hikaye > Kurulum Dosyası Oluştur
; Kurulum kullanıcıya özeldir (yönetici izni gerekmez). Hikâyeler, sesler, videolar (Belgeler\HikayeStudyosu)
; ve ayarlar/API anahtarı (kayıt defteri) güncellemede ve kaldırmada SİLİNMEZ.
; AppId DEĞİŞTİRİLMEMELİ: yeni sürümün eskisinin üzerine kurulmasını bu sağlar.

#ifndef Surum
  #define Surum "4.0.1"
#endif
#define Ad "Hikâye Stüdyosu"
#define Exe "HikayeStudyo.exe"

[Setup]
AppId={{7C1B6E52-9D3A-4F0B-A7C4-2E8D5B9F1A63}
AppName={#Ad}
AppVersion={#Surum}
AppVerName={#Ad} {#Surum}
AppPublisher=HikayeStudyosu
DefaultDirName={localappdata}\Programs\HikayeStudyosu
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\Build\Kurulum
OutputBaseFilename=HikayeStudyosu_Kurulum_{#Surum}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#Exe}
UninstallDisplayName={#Ad}
CloseApplications=yes

[Languages]
Name: "turkce"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "masaustu"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[InstallDelete]
; Güncellemede eski sürümün artık dosyaları kalmasın (kullanıcı verisi burada tutulmaz).
Type: filesandordirs; Name: "{app}\HikayeStudyo_Data"

[Files]
; Unity çalışma zamanı + FFmpeg (HikayeStudyo_Data\StreamingAssets\ffmpeg) tek pakette
Source: "..\Build\HikayeStudyo\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*_BackUpThisFolder_ButDontShipItWithYourGame,*_BurstDebugInformation_DoNotShip,\Kayitlar,\arayuz_log.txt,\ffkayit.txt,\ffkayit_sonuc.txt,*.log,*.pdb"

[Icons]
Name: "{autoprograms}\{#Ad}"; Filename: "{app}\{#Exe}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#Ad}"; Filename: "{app}\{#Exe}"; WorkingDir: "{app}"; Tasks: masaustu

[Run]
Filename: "{app}\{#Exe}"; WorkingDir: "{app}"; Description: "{cm:LaunchProgram,{#Ad}}"; Flags: nowait postinstall skipifsilent
