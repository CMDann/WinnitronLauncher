; PICO8 RUN SCRIPT

#Persistent
#MaxHotkeysPerInterval 200


; RUN THE GAME
Run, C:/WINNITRON_UserData/Options/Pico8/nw.exe
SetTitleMatchMode 2
idleLimit:= 10000 ; three seconds
SetTimer, InitialWait, 30000
MouseMove, 3000, 3000, 0

; This is the function that quits the app
KillApp()
{
	WinClose, PICO-8
	WinKill, PICO-8
	ExitApp
}

InitialWait:
	SetTimer,  CloseOnIdle, % idleLimit+150
return

; This is the timer
CloseOnIdle:
	if (A_TimeIdle>=idleLimit)
	{
		KillApp()
		SetTimer,CloseOnIdle, Off
	}
	else
	{
		SetTimer,CloseOnIdle, % idleLimit-A_TimeIdle+150
	}
return

; Do this stuff when Esc is pressed
~Esc::
	If escIsPressed
		return
	escIsPressed := true
	SetTimer, WaitForESCRelease, 3000		; 3 seconds
return

; Do this stuff when Esc is UP
~Esc Up::
	SetTimer, WaitForESCRelease, Off
	escIsPressed := false
return

WaitForESCRelease:
	SetTimer, WaitForESCRelease, Off
	KillApp()
return

; KEYMAPS BELOW
/::m
.::n
a::s
s::d
d::f
w::e
`::q
1::w