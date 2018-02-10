PUBLIC asm_hook_connect;

_DATA SEGMENT
_DATA ENDS

_TEXT SEGMENT

EXTERN hook_connect: PROC
EXTERN connectTramp: qword

asm_hook_connect PROC
	jmp connectTramp;
asm_hook_connect ENDP

_TEXT ENDS
END