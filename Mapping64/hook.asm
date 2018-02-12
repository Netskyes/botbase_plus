PUBLIC asm_hook_connect;

_DATA SEGMENT
_DATA ENDS

_TEXT SEGMENT

EXTERN hook_connect: PROC
EXTERN send_sock_patch: PROC
EXTERN connectTramp: qword

EXTERN sockBackup: qword
EXTERN sockAddrNew: qword
EXTERN sockAddrBackup: qword
EXTERN retAddr: qword


asm_hook_connect PROC
	push rax;
	push r9;
	push r10;
	push r11;
	pushfq;
	push r8;

	push rdx;
	pop sockAddrBackup;
	push rdx;
	push rcx;

	call hook_connect;

	pop rcx;

	add rsp, 8;
	push sockAddrNew;

	pop rdx;
	pop r8;
	popfq;
	pop r11;
	pop r10;
	pop r9;
	pop rax;
	
	pop retAddr;
	call connectTramp;
	push retAddr;

	push rcx;
	push rdx;
	push r8;
	push r9;
	push r10;
	pushfq;

	mov rcx, sockBackup;
	call send_sock_patch;

	popfq;
	pop r10;
	pop r9;
	pop r8;
	pop rdx;
	pop rcx;

	mov rax, 0;

	ret;

asm_hook_connect ENDP

_TEXT ENDS
END