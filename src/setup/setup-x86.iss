// https://github.com/DomGries/InnoDependencyInstaller

#define AppName "Vajehdan"
#define AppExeName AppName+".exe"
#define dotnet_version "6.0.6"
#define MyAppURL "https://mrkou65.github.io/Vajehdan/"
#define Platform "x86"

//Return app version in SemVer (for example: 4.0.2.3 => 4.0.2)
#define AppVersion() \
   GetVersionComponents("..\x86\"+AppExeName, \
       Local[0], Local[1], Local[2], Local[3]), \
   Str(Local[0]) + "." + Str(Local[1]) + "." + Str(Local[2])

[Setup]  
AppId={{86847BF6-6691-4CF6-98D7-4692205872F7}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={userappdata}\{#AppName}
OutputDir=..\.
DefaultGroupName={#AppName}
OutputBaseFilename=VajehdanSetup-{#AppVersion}-{#Platform}
SetupIconFile=setup.ico
UninstallDisplayIcon={app}\{#AppExeName}
PrivilegesRequired=lowest
DisableFinishedPage=yes
DisableDirPage=yes
DisableReadyPage=yes
DisableReadyMemo=yes
DisableProgramGroupPage=yes


[Files]
Source: "..\{#Platform}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "windowsdesktop-runtime-{#dotnet_version}-win-{#Platform}.exe"; DestDir: "{tmp}"; Flags: ignoreversion deleteafterinstall; AfterInstall : InstallDotNet;

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{userdesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"

[Run]
Filename: {app}\{#AppExeName}; Flags: nowait postinstall skipifsilent

[Code]
procedure InstallDotNet;
var
  StatusText: string;
  ResultCode: Integer;
begin
  StatusText := WizardForm.StatusLabel.Caption;
  WizardForm.StatusLabel.Caption := 'Installing .NET Desktop Runtime {#dotnet_version}';
  WizardForm.ProgressGauge.Style := npbstMarquee;
  try
    Exec(ExpandConstant('{tmp}\windowsdesktop-runtime-{#dotnet_version}-win-{#Platform}.exe'), '/install /quiet /norestart', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
  finally
    WizardForm.StatusLabel.Caption := StatusText;
    WizardForm.ProgressGauge.Style := npbstNormal;
  end;
end;
