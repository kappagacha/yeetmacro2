result := LetUserSelectRect()
message := "{""X"": " . result.x1 . ", ""Y"": " . result.y1 . ", ""W"": " . result.x2 - result.x1 . ", ""H"": " . result.y2 - result.y1 . "}"
stdout := FileOpen("*", 0x1)	; https://www.reddit.com/r/AutoHotkey/comments/rf9krl/stdout_to_console/
stdout.Write(message)
; FileAppend message

ExitApp

LetUserSelectRect()
{
    ;global windowX, windowY
    CoordMode, Mouse ; Required: change coord mode to screen vs relative.

    ToolTip, Left Click (Hold) and Drag to create a rectangle
    SetSystemCursor("IDC_CROSS")
    static r := 3
    ; Create the "selection rectangle" GUIs (one for each edge).
    Loop 4 {
        Gui, r%A_Index%: -Caption +ToolWindow +AlwaysOnTop
        Gui, r%A_Index%: Color, Red
    }
    ; Disable LButton.
    Hotkey, *LButton, lusr_return, On

    ; Wait for user to press LButton or Escape
	Loop {
		
		if GetKeyState("LButton", "P") {
			Break
		}
		if GetKeyState("Esc", "P") {
            abort := true
            Break
		}

		sleep, 50
	}

    if (!abort) {
        ; Get initial coordinates.
        MouseGetPos, xorigin, yorigin
        ; Set timer for updating the selection rectangle.
        SetTimer, lusr_update, 10
        ; Wait for user to release LButton.
        KeyWait, LButton
    }
    
    ; Re-enable LButton.
    Hotkey, *LButton, Off
    ; Disable timer.
    SetTimer, lusr_update, Off
    ; Destroy "selection rectangle" GUIs.
    Loop 4
        Gui, r%A_Index%: Destroy
    RestoreCursors()
    ToolTip
    ;CoordMode, Mouse
    return abort ? "" : { x1 : x1, y1 : y1, x2 : x2, y2 : y2 }
 
    lusr_update:
        CoordMode, Mouse ; Required: change coord mode to screen vs relative.
        MouseGetPos, x, y
        if (x = xlast && y = ylast)
            ; Mouse hasn't moved so there's nothing to do.
            return
        if (x < xorigin)
             x1 := x, x2 := xorigin
        else x2 := x, x1 := xorigin
        if (y < yorigin)
             y1 := y, y2 := yorigin
        else y2 := y, y1 := yorigin

        ; Update the "selection rectangle".
        Gui, r1:Show, % "NA X" x1 " Y" y1 " W" x2-x1 " H" r
        Gui, r2:Show, % "NA X" x1 " Y" y2-r " W" x2-x1 " H" r
        Gui, r3:Show, % "NA X" x1 " Y" y1 " W" r " H" y2-y1
        Gui, r4:Show, % "NA X" x2-r " Y" y1 " W" r " H" y2-y1

        ; x1 := x1 - windowX
        ; x2 := x2 - windowX
        ; y1 := y1 - windowY
        ; y2 := y2 - windowY
    lusr_return:
    return
}

;https://www.autohotkey.com/board/topic/32608-changing-the-system-cursor/
SetSystemCursor( Cursor = "", cx = 0, cy = 0 )
{
	BlankCursor := 0, SystemCursor := 0, FileCursor := 0 ; init
	
	SystemCursors = 32512IDC_ARROW,32513IDC_IBEAM,32514IDC_WAIT,32515IDC_CROSS
	,32516IDC_UPARROW,32640IDC_SIZE,32641IDC_ICON,32642IDC_SIZENWSE
	,32643IDC_SIZENESW,32644IDC_SIZEWE,32645IDC_SIZENS,32646IDC_SIZEALL
	,32648IDC_NO,32649IDC_HAND,32650IDC_APPSTARTING,32651IDC_HELP
	
	If Cursor = ; empty, so create blank cursor 
	{
		VarSetCapacity( AndMask, 32*4, 0xFF ), VarSetCapacity( XorMask, 32*4, 0 )
		BlankCursor = 1 ; flag for later
	}
	Else If SubStr( Cursor,1,4 ) = "IDC_" ; load system cursor
	{
		Loop, Parse, SystemCursors, `,
		{
			CursorName := SubStr( A_Loopfield, 6, 15 ) ; get the cursor name, no trailing space with substr
			CursorID := SubStr( A_Loopfield, 1, 5 ) ; get the cursor id
			SystemCursor = 1
			If ( CursorName = Cursor )
			{
				CursorHandle := DllCall( "LoadCursor", Uint,0, Int,CursorID )	
				Break					
			}
		}	
		If CursorHandle = ; invalid cursor name given
		{
			Msgbox,, SetCursor, Error: Invalid cursor name
			CursorHandle = Error
		}
	}	
	Else If FileExist( Cursor )
	{
		SplitPath, Cursor,,, Ext ; auto-detect type
		If Ext = ico 
			uType := 0x1	
		Else If Ext in cur,ani
			uType := 0x2		
		Else ; invalid file ext
		{
			Msgbox,, SetCursor, Error: Invalid file type
			CursorHandle = Error
		}		
		FileCursor = 1
	}
	Else
	{	
		Msgbox,, SetCursor, Error: Invalid file path or cursor name
		CursorHandle = Error ; raise for later
	}
	If CursorHandle != Error 
	{
		Loop, Parse, SystemCursors, `,
		{
			If BlankCursor = 1 
			{
				Type = BlankCursor
				%Type%%A_Index% := DllCall( "CreateCursor"
				, Uint,0, Int,0, Int,0, Int,32, Int,32, Uint,&AndMask, Uint,&XorMask )
				CursorHandle := DllCall( "CopyImage", Uint,%Type%%A_Index%, Uint,0x2, Int,0, Int,0, Int,0 )
				DllCall( "SetSystemCursor", Uint,CursorHandle, Int,SubStr( A_Loopfield, 1, 5 ) )
			}			
			Else If SystemCursor = 1
			{
				Type = SystemCursor
				CursorHandle := DllCall( "LoadCursor", Uint,0, Int,CursorID )	
				%Type%%A_Index% := DllCall( "CopyImage"
				, Uint,CursorHandle, Uint,0x2, Int,cx, Int,cy, Uint,0 )		
				CursorHandle := DllCall( "CopyImage", Uint,%Type%%A_Index%, Uint,0x2, Int,0, Int,0, Int,0 )
				DllCall( "SetSystemCursor", Uint,CursorHandle, Int,SubStr( A_Loopfield, 1, 5 ) )
			}
			Else If FileCursor = 1
			{
				Type = FileCursor
				%Type%%A_Index% := DllCall( "LoadImageA"
				, UInt,0, Str,Cursor, UInt,uType, Int,cx, Int,cy, UInt,0x10 ) 
				DllCall( "SetSystemCursor", Uint,%Type%%A_Index%, Int,SubStr( A_Loopfield, 1, 5 ) )			
			}          
		}
	}	
}

RestoreCursors()
{
	SPI_SETCURSORS := 0x57
	DllCall( "SystemParametersInfo", UInt,SPI_SETCURSORS, UInt,0, UInt,0, UInt,0 )
}