PUBLIC hook_connect


_DATA SEGMENT
_DATA ENDS
_TEXT SEGMENT

hook_connect PROC
	pushad
	pushfd

	popad
	popfd
hook_connect ENDP

_TEXT ENDS
END