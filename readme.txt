EOSMonitor
==========

Start mit der Version vom 2023-05-17

Finale Version auf Basis des EDSDK-Tutorials
EDSDK-Version 13.16.10
EdsImage-Version:  3.15.0.6400
Net 6.0

überflüssige Dateien aus CameraControl entfernt

Aktualisierungen:
=================
- Auf .Net 9.0:		umgestellt
- EDSDK.cs/:		aus dem CSharp-Sample des SDK übernommmen
- EdsImage.dll:		aus dem CSharp-Sample des SDK übernommmen
- Abhängigkeiten:	System.Management, System-Reactive.Linq aktualisiert
- Warnungen beseitigt, markiert mit "//!!

13.1.2025:  		
- (fast) alle Mitteilungen berücksichtigt
- Warnungen bis auf die von EDSDK.CS verursachten beseitigt
- nicht funktionsfähig:  Rechteck zur des Focus-Bereichs im Live Vie-Modus
			
17.01.2025: 
- Iso-, Av-, Tv-Einstellung an der Kamera und im EosMonitor fuktionieren
  in den Modi Av, Tv, M, B, P  und sind an der Kamera und im EosMonitor umschaltbar.
- gelegentlch wird beim Umschalten von M --> Av die Av-Liste nicht aktualisiert (Kamera lisfert Listenlänge 0)
- nach Programmende muss der Live-Modus imKamera-Menü wieder aktiviert werden.
- Das Focus-Rechteck im Live-Modus wird nicht angezeigt.

21.01. 
in:  class cmdSetPropertyIntegerData Fehler behoben:
     if("MainWindow.cameraManager.OpenSession()"  geändert in "MainWindow.cameraManager.IsSessionOpen"

24.1.
EVC - Steuerung funktioniert mit beiden Kameras !!!
Sicherung 2025.01.24-2

13.02.
- Koordination von LiveView uns TakePicture-Buttons funktioniert
- überflüssige Icons aus den Resourcen entfernt
- this-Qualifier entfernt.

8.2. 
An/Abschalten des EVF funktioniert
Zoom-Rechteck wird bei Start von der Kamera übernommmen,
wenn nicht, dann zentral gesetzt

Take Picture für die M, Av, Tv, P  funktionieren  für LiveView =on und off!!!!


Take Picture funktioniert auch für den Bulb mode.!!!!

Focus-Timer und UpdateFocusInfo:
Zoomerechteck und Fokuspunkte werden bei Drag korrekt mitgezogen!!!

Problem:  Blendenwerte (Av) werden nicht von der Av-Combobox übernommmen ???????
betrifft:
- Handle_ApertureValueComboBox_SelectionChanged, Z 630
- HandleAvValueChanged, Z 160
--> Version 17.01.2025

Version 25-03-07:
Av-Eistellung funktioniert mit+hne LiveViewProblem: 
Zu frühes Stop-LiveView wirkt nicht, mehrfach aber schon
	
Version 25-03-08:
Take Picture / LiveView-Wechsel funktionieren
Av-Einstellung von der Kamera aus funktioniert noch nicht.

Version 2025-03-15:
AF ein/ausschalten OK
FocusInfoTimer OK
Zoom-Button OK
Histogramm-Button OK
manuelle Focus-Steuerung funktioniert

Version 2025-03-21:
Take Picture funktioniert aber:  im Av-Modus lange Reaktionszeit
Die Version 2025-03-19_23:: ist diesbezüglich OK

Version 2025-03-20:
LiveView funktioniert auch im DSLR Bulb mode:
Ursache:   waitTime_FocusInfo     = 2000;

Version 2025-03-23 19:00:
TakePicture und LiveView funktionsfähig
eventuell: Interrupts AvChanged, TvChanged .... abschalten

Version 2025-03-23 19:00
TakePicture und LiveView funktionsfähig
Focus Info während Take Picture für DSLR abgeschaltet

Version 2025-03-23 19:00
Source code bereinigt
Test für DSLM und DSLR OK

Version 2025-03-28 19:00
Kamera-Wechsel funktioniert
TakePicture-Problem im Av-Modus gelöst ?? ( AF-Problem im Av-Modus !)

Version 2025-04-01 15:00
Af+Focus-Info funktionieren auch im Zusammenhang mit TakePicture aus Eos 60D unf Eos R6 !!!!!!

Version 2025-04-03 19:00
Av-Werte werden beim Startup jetzt korrekt geladen (durch Modifikation bei Startup-Prozess).
Combobox lässt jetzt nur noch einmalige Auswahl zu und meldet den Initialisierungsfortschritt
Combobox schiesst direkt nach der Auswahl: Initialisierung in einen Thread ausgelagert 

Version 2025-04-05 19:00
Exit-Button implementiert.
Batterie-Level-Anzeige funktioniert wieder: "(RaiseProperty (BatteryLevelChanged)" 

Version 2025-04-06 19:00
Restart button implementiert
Logo Monitor.png aktualisiert

Version 2025-04-15:
Focus Stacking funktioniert für die EOS R6


Version 2025-04-16:
Focus Stacking funktioniert mit der 60D:
aber wird trotzdem nicht aktviert!

Version 2025-04-18:
LiveView-Modus für AF-Steuerung und Stacking-Modus getrennt.

Version 2025-04-21:
keine ausstehenden Änderungen

Version 2025-04-25_final:
Programm umstrukturiert:  große Code-Dateien zerlegt

Version 2025-04-25_final:
Konfigurationsdatei angelegt:
Parameter:  _delay_DriveLens,_delay_TakePicture


Version 2025-05-01:
State Event Handler für "EDSDK.StateEvent_Shutdown" entfernt, da er
zu eine Fehelrmeldung "Unexpected Parameters" führt, wenn das USB-Kabel entfernt wird


Version 2025-05-02
In  "Handle_ExposureTimeValueComboBox_SelectionChanged(....)"  den Tv-Wert 
mit "SetPropertyIntegerData" eingestellt (wg. Timing/Busy-Problemen im Live Mode

Version 2025-05-02
Anpassung der Menüs für LiveView, Stacking bzgl. AF ON/OFF -Einstellung für DSLM bzw. DSLR-Kameras


Version 2025-05-02:
Test mit DSLM: alle Funktionen OK
DSLR:  hängt sich im LiveView-Modus nach Take Picture (?) auf

Version 2025-05-06:
- Nach Take Picture Delay von einigen Sekunden eingefügt.
- Gesperrte AF/Stacking/Meter-Buttons im LiveView- und Stackingmodus nur gedimmt 
  und deaktiviert statt komplett ausgeblendet. 
- AF-Modus Quick im LiveView-Betrieb wieder zugelassen. 

Version 2025-05-11:
Neues Design implementiert
Quellcode aufgeräumt
Installationsprojekt erstellt

Version 2025-05-11:
Anzeige der verbleibenden Bilder auf "save to camera"  beschränkt


Version 2025-05-13:
Fehler gesucht und behoben:
TakePicture_normalMod:
"cameraModel.LiveViewDevice = LiveViewDevice.None;" 
wurde herausgenommenen, da das die  LiveView nach einer Aufnahme stört/blockiert

Version 2025-05-15:
Fehler beim Abbruch in "OnWatcher_EventArrived" beseitigt.
LiveViewDevice beim Beenden auf die Kamera gelegt
Meldung "Set AF OFF" in einen Canvas in XML-Datei gelegt
Nach Einschalten von LiveView den Autofocus für die Anzeige noch einmal aktiviert

Installationsdatei EosMonitor.msi aktualieesiert

===================================


